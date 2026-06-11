# YonaYona Durable Functions Night

YonaYona Azure Club 第 17 回 **YonaYona Durable Functions Night** 登壇用の資料と
デモコードです。

- **イベント**: <https://yonayona.connpass.com/event/393817/>
- **日時**: 2026/06/12 (金) 21:00〜22:00（オンライン / Teams）
- **セッション**: 「Durable Functions 入門 〜 Durable Agent への道」（20 分）
- **登壇者**: Kazuki Ota（[@okazuki](https://x.com/okazuki)）
- **ハッシュタグ**: `#YonaAz`

## 構成

| パス | 内容 |
|------|------|
| `slides/` | 登壇用 PowerPoint 資料（`slides.pptx`） |
| `demo/`   | Aspire AppHost + Azure Functions + Durable Task Scheduler Emulator のデモ |
| `demo/AppHost/`         | Aspire AppHost プロジェクト |
| `demo/ServiceDefaults/` | Aspire ServiceDefaults |
| `demo/Functions/`       | Azure Functions（isolated worker, .NET 10） |

## 前提

- .NET 10 SDK
- [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/cli/install) 13.3.x 以上
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4
- Docker Desktop（DTS Emulator と Azurite を起動するため）

## デモの動かし方

```pwsh
# リポジトリ直下で
cd demo
aspire run

# Aspire ダッシュボードが開く
# scheduler / azurite / funcapp のリソースが Running になる
```

別ターミナルで HTTP トリガーを叩く：

```pwsh
# Aspire ダッシュボードで funcapp のエンドポイントを確認してから
curl.exe -X POST http://localhost:<funcapp-port>/api/StartCreation
```

返却される `statusQueryGetUri` でステータス確認可能。
DTS ダッシュボード（Aspire ダッシュボード上の "Scheduler Dashboard" リンク）で
オーケストレーションの進行を可視化できる。
Tokyo → Seattle → London の作成 Activity が順次完了すると、Human-in-the-loop の外部イベント待ちになる。
承認または拒否は次の HTTP トリガーで送信する：

```pwsh
curl.exe -X POST http://localhost:<funcapp-port>/api/ApproveCreation/<instance-id> `
  -H "Content-Type: application/json" `
  -d '{ "decision": "OK" }'

curl.exe -X POST http://localhost:<funcapp-port>/api/ApproveCreation/<instance-id> `
  -H "Content-Type: application/json" `
  -d '{ "decision": "NG" }'
```

### Durable Functions の "Durable" を体感する

1. HTTP リクエストでオーケストレーション開始
2. 進行中に **Aspire ダッシュボードから `funcapp` だけ Stop**
3. DTS ダッシュボードに履歴が残っていることを確認
4. `funcapp` を **Start** → 続きから再開され、Human-in-the-loop の外部イベント待ちになる

> ⚠️ DTS Emulator はインメモリストレージ。`scheduler` や `azurite` を停止すると
> 履歴が消えるので、デモ中は **`funcapp` のみ** 操作する。

## ライセンス

MIT License
