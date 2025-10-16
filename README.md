
## このリポジトリについて

このリポジトリではAzure AI Foundryを利用してSoraによる動作生成を行うためのサンプルコードを提供しています。

## 実行結果

猫の動画（sample.mp4）が生成されます。

### プレビュー

以下はリポジトリ内の `sample.mp4` を参照する HTML5 動画プレーヤーの例です。GitHub のリポジトリビューではブラウザによっては直接再生されない場合があります（raw.githubusercontent.com 経由や GitHub Pages での公開を推奨）。動かない場合は下のダウンロードリンクを使ってください。

<video controls width="640">
	<source src="./sample.mp4" type="video/mp4">
	お使いのブラウザは video タグに対応していません。代わりにこちらからダウンロードしてください。
</video>

[Download sample.mp4](./sample.mp4)

## Azure環境のセットアップガイド

## Azure CLIをセットアップする

以下のコマンドを実行して、Azure CLIをインストールします。

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

インストール方法は[公式ドキュメント](https://learn.microsoft.com/ja-jp/cli/azure/install-azure-cli-linux?pivots=apt)を参照してください。

## Azure CLIでログインする

環境変数 `AZURE_TENANT_ID`が設定されている場合は、以下のコマンドでログインします。

```bash
az login --tenant $AZURE_TENANT_ID
```

### Azure CLIの動作確認

以下のコマンドでAzure CLIのバージョンとアカウント情報を確認します。

```bash
az version
az account list
```

## GitHub Codespacesの設定

`.env`でシークレットを管理する場合、以下のコマンドでCodespacesにシークレットを設定します。

```bash
gh secret set --app codespaces -f .env
```

シークレットの一覧を確認するには、以下のコマンドを実行します。

```bash
gh secret list --app codespaces
```

単一のシークレットを設定するには、以下のコマンドを使用します。

```bash
gh secret set --app codespaces SECRET_NAME
```

シークレットの削除は以下のコマンドで行います。

```bash
gh secret delete --app codespaces SECRET_NAME
```
