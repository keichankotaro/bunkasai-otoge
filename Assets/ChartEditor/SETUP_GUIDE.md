# Chart Editor セットアップ手順

譜面エディタを使用するには、以下の手順に従ってUnityシーンをセットアップしてください。

## 1. シーンの準備
1. Unityエディタで `Assets/ChartEditor.unity` シーンを開きます。
2. ヒエラルキー（Hierarchy）上で右クリックし、`Create Empty` を選択して空のGameObjectを作成します。
3. 作成したGameObjectの名前を `ChartEditorSystem` に変更します。

## 2. スクリプトのアタッチ
1. `ChartEditorSystem` GameObjectを選択します。
2. インスペクター（Inspector）の `Add Component` ボタンを押し、`ChartEditorManager` スクリプトを検索して追加します。

## 3. 設定の確認
`ChartEditorManager` コンポーネントの設定を確認します：
- **Initial Chart Path**: デフォルトで読み込むチャートのパス（例: `Assets/Resources/Charts/test-chart.json`）。
- **Audio Source**: 空欄の場合、自動的に追加されますが、必要に応じて既存のAudioSourceを割り当ててください。

## 4. エディタの使用方法
ゲームを再生（Play）すると、画面左側にツールバーが表示されます。

- **Load/Save**: チャートの読み込みと保存。
- **Play/Pause (Space)**: 楽曲の再生と一時停止。
- **Mode**:
    - **Normal**: 音ゲーのような縦スクロール表示。
    - **Score**: 横スクロールの楽譜（リズム譜）表示。
- **Lane**: 配置するレーンの選択（1〜4）。
- **Note Type**: ノーツの種類（Tap/Long）。
- **Mouse Operations**:
    - **左クリック**: ノーツの配置。
    - **右クリック**: （未実装）ノーツの削除。
    - **Ctrl + ホイール**: 時間軸の拡大縮小。

## 注意事項
- 現状はプロトタイプのため、UIは簡易的な `OnGUI` で描画されています。
- 保存時は既存のファイルを上書きする場合があるので注意してください。
