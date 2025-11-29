# 💡 TurnOffTheLights

**現在開発中の3Dアクション（鬼ごっこ）ゲーム**
「子供が部屋の電気を次々とつけて回る」という日常の悩みをテーマにした、**FPS視点の電気消しバトル**です。
親（プレイヤー）となり、NavMeshで自律移動する子供（AI）がつけた電気をひたすら消して回ります。

<p align="left">
  <img src="https://img.shields.io/badge/Unity-6000.1.0f1-000000?style=for-the-badge&logo=unity&logoColor=white" />
  <img src="https://img.shields.io/badge/Status-In_Development-yellow?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Tech-NavMesh-blue?style=for-the-badge" />
</p>

---

## 🚧 Current Status
現在、コアメカニクスの実装を行っているプロトタイプ段階です。
**「NavMeshを用いたAIの巡回移動」**の実装が完了し、ゲームルールの構築を進めています。

---

## 🎯 Concept
**"The Endless Battle of Electricity Bill"**
子供は部屋のドアを開け放ち、電気をつけて去っていく……。
そんな親の葛藤をゲーム化しました。

* **Genre**: FPS Tag Game（一人称視点の鬼ごっこ）
* **Player**: 部屋を走り回り、スイッチをオフにする
* **Enemy (Child)**: 部屋を巡回し、スイッチをオンにする＆ドアを開けっ放しにする

---

## 🛠 Technical Challenge
本プロジェクトの主な学習テーマは **3D空間におけるAI制御** です。

### 🤖 NavMeshによる自律移動
敵キャラクター（子供）の動きを制御するために、Unity標準の **NavMesh (Navigation Mesh)** を初めて導入しました。
* 部屋の形状に合わせた移動可能エリアのベイク（Baking）
* `NavMeshAgent` を用いたターゲット（照明スイッチ）への経路探索
* 障害物を回避しながら次の目的地へ向かうロジックの実装

---

## 📝 To-Do List
- [x] FPS視点のプレイヤー移動
- [x] NavMeshによる敵の巡回移動
- [ ] 照明スイッチのインタラクション（On/Off機能）
- [ ] 「電気代」を模したスコアシステムの実装
- [ ] サウンド（スイッチ音、子供の足音）の追加

---

## 🛠 Environment
- Unity 6 (6000.1.0f1)
- GitHub
