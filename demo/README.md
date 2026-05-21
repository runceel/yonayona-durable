# Demo: Aspire + Azure Functions + Durable Task Scheduler Emulator

YonaYona Durable Functions Night 登壇用ライブデモのソリューション。

> .NET 10 / Azure Functions（isolated worker）/ .NET Aspire 13.x / Durable Task Scheduler Emulator

## 構成

| プロジェクト | 役割 | ターゲット |
|--------------|------|-----------|
| `AppHost/`         | .NET Aspire AppHost。DTS Emulator + Azurite + Functions を統合起動 | net10.0 |
| `ServiceDefaults/` | Aspire 共通設定（このデモでは未使用、テンプレ既定） | net10.0 |
| `Functions/`       | Azure Functions プロジェクト。Weather Orchestrator | net10.0 |

## 起動

```pwsh
# Docker Desktop を起動しておくこと
cd D:\repos\runceel\yonayona-durable\demo
aspire run
```

`aspire run` を実行すると：

- DTS Emulator (`mcr.microsoft.com/dts/dts-emulator:latest`) コンテナを起動
- Azurite (`mcr.microsoft.com/azure-storage/azurite`) コンテナを起動
- Functions プロジェクトを `func host start` で起動
- Aspire ダッシュボードを開く（ターミナルに URL 表示）

ダッシュボードからは：

- 各リソースの **Stop / Start / Restart** ボタン
- `scheduler` リソースの **"Scheduler Dashboard"** / **"Task Hub Dashboard"** リンク（DTS 自前ダッシュボード）
- 各リソースのログ / コンソール / 環境変数

## デモシナリオ

「6 都市の天気を並列取得 → 平均気温を返す」を Fan-out / Fan-in で実装。
各都市の取得は `Task.Delay` で 5〜10 秒のランダム遅延 + ランダム気温（進捗を見せるため）。

### 1. オーケストレーション開始

ダッシュボードで `funcapp` のエンドポイント URL を確認し、HTTP POST を投げる：

```pwsh
# ポートはダッシュボードで確認（例: 7018）
curl -X POST http://localhost:7018/api/StartWeather
```

返却 JSON の `StatusQueryGetUri` で進捗を確認できる：

```json
{
  "Id": "abc123...",
  "StatusQueryGetUri": "http://localhost:7018/runtime/webhooks/durabletask/instances/abc123...?code=..."
}
```

### 2. "Durable" を体感する

1. オーケストレーション開始後、すぐに Aspire ダッシュボードで `funcapp` を **Stop**
2. DTS ダッシュボード（"Scheduler Dashboard" リンク）でインスタンスが残っていることを確認
3. `funcapp` を **Start**
4. `StatusQueryGetUri` を再取得 → `runtimeStatus: Completed` で結果が返ってくる

CLI からも操作可能：

```pwsh
aspire resource funcapp stop
aspire resource funcapp start
```

### 3. 結果の例

```json
{
  "name": "WeatherOrchestrator",
  "instanceId": "...",
  "runtimeStatus": "Completed",
  "output": {
    "Cities": [
      { "City": "東京",   "Temperature": 30 },
      { "City": "大阪",   "Temperature": -5 },
      { "City": "札幌",   "Temperature": -1 },
      { "City": "福岡",   "Temperature": 21 },
      { "City": "那覇",   "Temperature": 34 },
      { "City": "仙台",   "Temperature": 27 }
    ],
    "Average": 17.666666666666668
  }
}
```

## 仕組み

- `AppHost/AppHost.cs`:
  - `AddDurableTaskScheduler("scheduler").RunAsEmulator()` で DTS Emulator を起動
  - `scheduler.AddTaskHub("taskhub")` でタスクハブを定義
  - `AddAzureFunctionsProject<Projects.Functions>("funcapp").WithHostStorage(storage).WithReference(taskHub)` で接続情報を自動注入
  - DTS integration は **Experimental**（`#pragma warning disable ASPIREDURABLETASK001`）
- `Functions/host.json`:
  - `storageProvider.type: "azureManaged"` で DTS バックエンドを指定
  - `hubName: "%TASKHUB_NAME%"` で Aspire が注入する env を参照
- 接続情報は Aspire が以下の env を自動注入：
  - `DURABLE_TASK_SCHEDULER_CONNECTION_STRING = Endpoint=http://...;Authentication=None`
  - `TASKHUB_NAME = taskhub`

## ⚠️ デモ運用上の注意

DTS Emulator は **インメモリストレージ**。`scheduler` コンテナを Stop すると履歴が消える。
デモ中は **`funcapp` のみ** Stop / Start すること。

## 前提

- .NET 10 SDK
- Aspire CLI 13.3.x 以降
- Azure Functions Core Tools v4
- Docker Desktop
