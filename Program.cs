﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using DotNETSora.Models;

// --- Main Program (Top-Level Statements for .NET 8) ---

// 環境変数から値を取得
var Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
var ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

// APIバージョンは引き続き定数として扱う
const string ApiVersion = "2025-01-01";

if (string.IsNullOrEmpty(Endpoint) || string.IsNullOrEmpty(ApiKey))
{
    Console.WriteLine("エラー: 環境変数 AZURE_OPENAI_ENDPOINT または AZURE_OPENAI_KEY が設定されていません。");
    Console.WriteLine("続行する前に、これらの環境変数を設定してください。");
    return;
}

// トップレベルで async/await を直接使用可能
try
{
    using var client = new HttpClient();
    
    // ヘッダーの設定 (Pythonのheadersに対応)
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    // 1. Create a video generation job
    var createUrl = $"{Endpoint}/openai/v1/video/generations/jobs?api-version={ApiVersion}";
    var requestBody = new VideoGenerationRequest();
    
    var json = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    Console.WriteLine("--- 1. ジョブを作成中 ---");
    Console.WriteLine($"エンドポイント: {Endpoint}");
    var response = await client.PostAsync(createUrl, content);
    response.EnsureSuccessStatusCode(); // HTTPステータスが2xxでなければ例外をスロー (raise_for_statusに相当)

    var responseJson = await response.Content.ReadAsStringAsync();
    JobStatusResponse? jobCreationResponse = JsonSerializer.Deserialize<JobStatusResponse>(responseJson);
    
    var jobId = jobCreationResponse?.Id;

    if (string.IsNullOrEmpty(jobId))
    {
        throw new Exception("ジョブIDがレスポンスから取得できませんでした。");
    }
    
    Console.WriteLine($"ジョブが作成されました: {jobId}");
    Console.WriteLine($"レスポンスJSON: {responseJson}");

    // 2. Poll for job status
    var statusUrl = $"{Endpoint}/openai/v1/video/generations/jobs/{jobId}?api-version={ApiVersion}";
    string status = string.Empty;
    JobStatusResponse? jobStatus = null;

    Console.WriteLine("\n--- 2. ステータスを確認中 (5秒ごとにポーリング) ---");
    while (status != "succeeded" && status != "failed" && status != "cancelled")
    {
        await Task.Delay(5000); // 5秒待機 (time.sleep(5)に相当)

        var statusResponse = await client.GetAsync(statusUrl);
        statusResponse.EnsureSuccessStatusCode();
        
    var statusJson = await statusResponse.Content.ReadAsStringAsync();
    // ポーリング応答を最新のステータスで上書き
    jobStatus = JsonSerializer.Deserialize<JobStatusResponse>(statusJson);

    status = jobStatus?.Status ?? string.Empty;
        Console.WriteLine($"ジョブステータス: {status}");
    }

    // 3. Retrieve generated video
    if (status == "succeeded")
    {
        Console.WriteLine("✅ ビデオ生成が成功しました。");
        
        var generations = jobStatus?.Generations;
        if (generations?.Count > 0)
        {
            var generationId = generations[0].Id;
            var videoUrl = $"{Endpoint}/openai/v1/video/generations/{generationId}/content/video?api-version={ApiVersion}";
            var outputFilename = "output.mp4";

            Console.WriteLine($"ビデオコンテンツをダウンロード中...");
            
            // ビデオのバイナリデータを取得
            var videoResponse = await client.GetAsync(videoUrl);
            videoResponse.EnsureSuccessStatusCode();
            
            var videoBytes = await videoResponse.Content.ReadAsByteArrayAsync();

            // ファイルに保存
            await File.WriteAllBytesAsync(outputFilename, videoBytes);
            
            Console.WriteLine($"生成されたビデオは「{outputFilename}」として保存されました。");
        }
        else
        {
            throw new Exception("ジョブ結果に生成されたビデオが見つかりませんでした。");
        }
    }
    else
    {
        throw new Exception($"ジョブは成功しませんでした。ステータス: {status}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTPリクエストエラー: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"エラーが発生しました: {ex.Message}");
}
