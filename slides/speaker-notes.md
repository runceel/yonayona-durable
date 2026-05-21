# Speaker Notes — Durable Functions 入門 〜 Durable Agent への道

YonaYona Durable Functions Night（2026/06/12）20 分セッション用のスピーカーノート。
`slides.pptx` と並べて使う。各スライドで話す内容と注意事項をまとめている。

> 全体タイム配分（目安）: スライド 14 分 + デモ 6 分 = 20 分
> ハッシュタグ: `#YonaAz`

---

## Slide 1 — 表紙（〜0:30）

「皆さん、こんばんは。本日は YonaYona Durable Functions Night にお越しいただきありがとうございます。Microsoft の太田と申します。本セッションでは "Durable Functions 入門 〜 Durable Agent への道" というタイトルで、20 分お話しします。
後続の maki さんの Durable Agent セッションへの土台になるよう、基本のキから整理していきます」

---

## Slide 2 — 自己紹介（〜1:00）

「簡単に自己紹介します。日本マイクロソフトで Cloud Solution Architect をしています。専門は .NET、Azure、最近は生成 AI 周りも触っています。X やブログでも発信していますので、気軽にフォローいただければ」

---

## Slide 3 — アジェンダ（〜1:30）

「本日のアジェンダはこちらです。まず Durable Functions の概要、次に主要な 6 つのパターン、続いて最新の Durable Task Scheduler。そして 6 分のライブデモを挟み、最後にまとめという流れです」

---

## Slide 4 — セクション: Durable Functions とは（〜1:35）

（セクション切り替えのみ。一拍置く）

---

## Slide 5 — Functions の課題と解決（〜2:30）

「Azure Functions の課題を整理しましょう。普通の Functions は状態を持てず、タイムアウトもあり、複数の関数を順序立てて呼ぶには工夫が必要でした。Durable Functions はこの 4 つの課題を一発で解決します。**ステートが永続化される** ので落ちても続きから動き、**数日・数か月の長時間処理** も書ける。順序保証もあり、自動で再開してくれます」

---

## Slide 6 — アーキテクチャ（〜3:00）

「Durable Functions の登場人物は 4 つです。**オーケストレーター** は処理の流れだけを書きます。**アクティビティ** が実際の I/O や計算を担います。**バックエンド** が履歴を保存し、**クライアント** が外部から起動する役割です。
重要なのは "オーケストレーターは自分では仕事をしない。アクティビティを呼ぶだけ" という分担です。これが後で出てくる Event Sourcing と密接に関係します」

---

## Slide 7 — Event Sourcing で耐久性 ★核心★（〜4:30）

「ここが本日のメインメッセージです。**Durable Functions の "Durable" の正体は Event Sourcing です。**
全ての処理ステップを履歴として記録し、プロセスが落ちても、起動時に履歴を **replay** することで続きから動きます。開発者は state machine を一切意識せず、普通の async/await コードを書くだけ。
これが、数日・数か月かかる処理や、AI Agent のような不安定で長時間の処理に Durable Functions が向いている理由です。後ほどデモで、実際に Functions を止めて見せます」

---

## Slide 8 — セクション: 主要パターン（〜4:35）

（セクション切り替えのみ）

---

## Slide 9 — Function Chaining（〜5:30）

「最初は Function Chaining。関数を順番に呼んでいくパターンです。コードを見てください。普通の async/await にしか見えませんよね。でも各 `await` ポイントで履歴に記録されるので、どこで落ちても続きから動きます。
ユースケースは、注文受付 → 決済 → 配送のようなステップワイズなワークフロー。**Durable Agent では、エージェントのステップ実行に使えます**」

---

## Slide 10 — Fan-out / Fan-in（〜6:30）

「次は Fan-out / Fan-in。並列実行して結果を集約するパターン。`Task.WhenAll` で並列化し、`Sum` で集めるだけ。アイテムが 100 件あっても、Functions ランタイムが自動でスケールアウトして並列実行してくれます。
**Durable Agent で言えば、複数の Tool を並列に呼んで結果をまとめる** ようなケースに使います」

---

## Slide 11 — Async HTTP APIs（〜7:00）

「Async HTTP APIs。バッチを起動して ID を即返却、`/status` URL を呼んでポーリングするパターン。長時間処理を REST API で公開するときに便利で、Durable Functions では標準で組み込み済みです。
**Durable Agent では、エージェント実行状況の確認 API** にそのまま使えます」

---

## Slide 12 — Monitoring（〜7:30）

「Monitoring。定期的に外部リソースの状態を確認し、条件を満たしたら抜けるパターン。`CreateTimer` で待機します。Timer Trigger と違って "重ならない" "可変間隔" "条件で終了" が可能です。
**Durable Agent ではエージェントの自律的な監視** に使えます」

---

## Slide 13 — Human Interaction（〜8:00）

「Human Interaction。人間の承認を待つパターン。`WaitForExternalEvent` で外部からのシグナルを待ち、タイムアウトと組み合わせるのが定番です。
**Durable Agent における Human-in-the-loop** はまさにこのパターンです。エージェントが自動で進めている処理を、人間が承認 / 拒否するワークフロー」

---

## Slide 14 — Aggregator / Durable Entity（〜8:30）

「Aggregator、別名 Durable Entity。ステートフルな小さな単位、つまり "アクター" のようなもの。カウンターや集約処理、小規模なステートマシンとして使えます。
**Durable Agent のメモリやコンテキスト管理にもこの考え方が活きてきます**」

---

## Slide 15 — セクション: Durable Task Scheduler（〜8:35）

（セクション切り替え。"ここからは少しホットな話題"）

---

## Slide 16 — Durable Task Scheduler (DTS)（〜10:00）

「最近 GA した **Durable Task Scheduler、略して DTS**。Microsoft が提供するマネージドなバックエンドサービスです。
これまでの Azure Storage バックエンドと違って、**高性能・高スケール、Functions プロセスから完全に独立、そして専用の監視ダッシュボード付き**。
新規開発では DTS を選ぶのが Microsoft 推奨です」

---

## Slide 17 — Storage バックエンドとの比較（〜11:30）

「Azure Storage と DTS を比較してみます。Azure Storage は標準で使えて低コスト、中規模ならこれで十分。一方 DTS は **スループットで約 5 倍、低レイテンシ、専用ダッシュボード、フルマネージド**。
DTS は Managed Identity 認証のみで、接続文字列が要らないのもセキュリティ上の利点です。本番運用なら DTS を選びましょう」

---

## Slide 18 — Live Demo（〜11:35）

「ここからはお待ちかね、ライブデモです。Aspire で Functions と DTS Emulator を一緒に起動して、Functions だけ止めてみます」

> ⚠️ **デモ進行台本は別途 `demo/README.md` の「デモシナリオ」参照**
>
> ### デモ進行の要点（6 分）
> 1. (0:15) Aspire ダッシュボード表示、構成説明
> 2. (0:45) コードをチラ見せ
> 3. (1:30) HTTP POST で起動
> 4. (2:30) **funcapp を Stop**（"普通ならアウト、でも…"）
> 5. (3:00) DTS ダッシュボードで履歴を見せる
> 6. (3:30) **funcapp を Start**
> 7. (4:00) 続きから動くログを見せる
> 8. (4:30) 最終結果（平均気温）を見せる
> 9. (5:00) スライドに戻る
>
> ⚠️ **絶対に `scheduler` や `azurite` を止めないこと**（履歴消失）

---

## Slide 19 — まとめ + 橋渡し（〜13:00）

「まとめです。**Durable Functions の本質は Event Sourcing による耐久性**。落ちても続きから動く、だから長時間処理に最適。
そして、ここからが maki さんへのバトンです。次のセッションでは、Agent Framework × Durable Functions で実現する **Durable Agent** の世界。今日見ていただいたオーケストレーターの仕組みが、エージェントのステート管理を支えていることを感じていただけると思います」

---

## Slide 20 — Thank you（〜13:30）

「ご清聴ありがとうございました。本日のスライドとデモコードは GitHub に公開しています。ハッシュタグ `#YonaAz` でフィードバックいただければ嬉しいです。
それでは maki さん、よろしくお願いします！」

---

## 全体注意事項

- デモは Docker Desktop が起動済みであることを必ず事前確認
- Aspire ダッシュボードと DTS ダッシュボードのウィンドウは事前に並べておく
- HTTP リクエスト用のターミナルも別ウィンドウで用意
- **バックアップ録画** (`slides/recording.mp4`) を予め画面に出しておけば事故時の保険になる
- 質疑応答用に `research-notes.md` Appendix A1〜A8 を別タブで開いておくと安心
