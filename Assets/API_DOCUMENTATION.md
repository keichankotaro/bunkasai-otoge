# 音ゲーAPI ドキュメント

このドキュメントは、音ゲーのユーザー認証とリザルト管理APIの仕様を記述します。

## 認証

`login.cgi`などを除く認証必須のエンドポイントでは、HTTP `Authorization` ヘッダーに `Bearer <token>` 形式でセッショントークンを含める必要があります。

---

## エンドポイント

### 1. ユーザー作成 (`create_user.cgi`)
- **メソッド:** `POST`
- **説明:** 新しいユーザーアカウントを作成します。
- **リクエストボディ:** `multipart/form-data`
  - `username` (string, required): ユーザーID (変更不可)
  - `player_name` (string, required): プレイヤー名
  - `password` (string, required): パスワード

### 2. ログイン (`login.cgi`)
- **メソッド:** `POST`
- **説明:** ログインしてセッショントークンを取得します。
- **リクエストボディ:** `multipart/form-data`
  - `username`, `password`

### 3. 自分のユーザー情報を取得 (`get_my_info.cgi`)
- **メソッド:** `GET`, **認証:** `Bearer`
- **説明:** ログイン中のユーザー情報を取得します。
- **レスポンス (成功):**
  ```json
  {
    "status": "success",
    "data": {
      "id": 1,
      "username": "testuser",
      "player_name": "Test Player",
      "created_at": "..."
    }
  }
  ```

### 4. プレイヤー名の更新 (`update_player_name.cgi`)
- **メソッド:** `POST`, **認証:** `Bearer`
- **説明:** プレイヤー名を変更します。
- **リクエストボディ:** `multipart/form-data`
  - `new_name` (string, required)

### 5. リザルトのアップロード (`upload_result.cgi`)
- **メソッド:** `POST`, **認証:** `Bearer`
- **説明:** 新しいスコアを投稿します。
- **リクエストボディ:** `multipart/form-data`
  - `song_title` (string, required)
  - `difficulty` (string, required): 'Easy', 'Hard', 'Master', 'Another'
  - `score` (integer, required): 0〜1,000,000

### 6. 最近のリザルト一覧を取得 (`get_my_results.cgi`)
- **メソッド:** `GET`, **認証:** `Bearer`
- **説明:** 直近30件のリザルトを新しい順に取得します。
- **レスポンス (成功):**
  ```json
  {
    "status": "success",
    "data": [
      {
        "song_title": "曲名A",
        "difficulty": "Master",
        "score": 980000,
        "submitted_at": "..."
      }
    ]
  }
  ```

### 7. 全曲ハイスコアを取得 (`get_my_high_scores.cgi`)
- **メソッド:** `GET`, **認証:** `Bearer`
- **説明:** 曲ごと・難易度ごとの自己ベストスコアを取得します。
- **レスポンス (成功):**
  ```json
  {
    "status": "success",
    "data": [
      {
        "song_title": "曲名A",
        "difficulty": "Master",
        "high_score": 980000
      },
      {
        "song_title": "曲名A",
        "difficulty": "Hard",
        "high_score": 950000
      }
    ]
  }
  ```

---

## ゲームクライアント向けAPI ガイド

このセクションは、音ゲーのクライアント（筐体やPCアプリなど）からサーバーAPIを利用するための技術仕様とワークフローを解説します。

### 基本情報

- **ベースURL:** `https://(あなたのドメイン)/文化祭音ゲー/userauth/`
- **リクエスト形式:** すべてのエンドポイントで `multipart/form-data` を使用します。
- **レスポンス形式:** すべてのレスポンスは `JSON` 形式です。

### 認証フロー

クライアントでのAPI利用は、セッショントークンに基づいています。

1.  **初回利用時:**
    1.  `create_user` でアカウントを作成します。
    2.  `register_felica` で、作成したアカウントにFelicaカード情報とPINを紐付けます。
2.  **ログイン:**
    - `login_felica` を使用して、IDmとPINで認証し、セッショントークンを取得します。
3.  **APIの利用:**
    - 取得したトークンを、以降のすべてのリクエストのHTTPヘッダーに `Authorization: Bearer <token>` の形式で付与します。
4.  **トークン失効時:**
    - APIが `401 Unauthorized` やトークン無効のエラーを返した場合、再度 `login_felica` を実行して新しいトークンを取得し直します。

---

### エンドポイント詳細

#### 1. アカウント作成 (`create_user.cgi`)

ゲームクライアント内で新規アカウントを作成する場合に使用します。

- **メソッド:** `POST`
- **リクエストボディ:**
  - `username` (string, required): ユーザーID (半角英数、変更不可)
  - `player_name` (string, required): プレイヤー名 (表示用、変更可)
  - `password` (string, required): アカウントのパスワード
- **レスポンス (成功):** `{"status": "success", "message": "User created successfully."}`
- **レスポンス (失敗):** `{"status": "error", "message": "Username already exists."}`

#### 2. Felicaカード登録 (`register_felica.cgi`)

作成したアカウントに、ゲームプレイで使用するFelicaカードとPINコードを紐付けます。

- **メソッド:** `POST`
- **説明:** `username`と`password`で本人確認を行った上で、Felica情報を登録します。
- **リクエストボディ:**
  - `username` (string, required): 登録対象のユーザーID
  - `password` (string, required): 本人確認用のパスワード
  - `felica_idm` (string, required): FelicaカードのIDm
  - `pin` (string, required): 4桁のPINコード
- **レスポンス (成功):** `{"status": "success", "message": "Felica information registered successfully."}`
- **レスポンス (失敗例):**
  - `{"status": "error", "message": "Invalid username or password."}`
  - `{"status": "error", "message": "This Felica card is already registered by another user."}`

#### 3. Felicaログイン (`login_felica.cgi`)

ゲームプレイ開始時の認証に使用します。成功すると、API操作に必要なセッショントークンが返されます。

- **メソッド:** `POST`
- **リクエストボディ:**
  - `felica_idm` (string, required): FelicaカードのIDm
  - `pin` (string, required): 4桁のPINコード
- **レスポンス (成功):**
  ```json
  {
    "status": "success",
    "token": "a1b2c3d4e5f6..."
  }
  ```
- **レスポンス (失敗):** `{"status": "error", "message": "Invalid Felica IDm or PIN."}`

#### 4. リザルト送信 (`upload_result.cgi`)

ゲームプレイの結果をサーバーに保存します。

- **メソッド:** `POST`
- **認証:** `Bearer` トークンが必須です。
- **リクエストボディ:**
  - `song_title` (string, required): プレイした曲の正式名称
  - `difficulty` (string, required): 難易度 ('Easy', 'Hard', 'Master', 'Another')
  - `score` (integer, required): スコア (0〜1,000,000)
- **レスポンス (成功):** `{"status": "success", "message": "Result uploaded successfully."}`
- **レスポンス (失敗):** `{"status": "error", "message": "Invalid or expired token."}`

---

### 実装ワークフロー例

**シナリオ：新規プレイヤーが初めてゲームをプレイし、リザルトを保存するまで**

1.  **【ゲーム画面】** プレイヤーに「新規登録」を選択させる。
2.  **【ゲーム画面】** ユーザーID、プレイヤー名、パスワードを入力させる。
3.  **【クライアント → API】** `POST /create_user.cgi` を呼び出し、アカウントを作成。
4.  **【ゲーム画面】** 「登録が完了しました。次にFelicaカードを登録します」と表示し、カードをタッチさせ、4桁のPINを入力させる。
5.  **【クライアント → API】** `POST /register_felica.cgi` を呼び出す。この時、先ほど入力されたユーザーIDとパスワードも一緒に送信して本人確認を行う。
6.  **【ゲーム画面】** 「カードの登録が完了しました。このカードとPINでログインできます」と表示。

**シナリオ：登録済みプレイヤーがログインしてプレイする**

1.  **【ゲーム画面】** 「Felicaカードをタッチしてください」と表示。
2.  **【ゲーム画面】** カードタッチ後、PINを入力させる。
3.  **【クライアント → API】** `POST /login_felica.cgi` を呼び出し、IDmとPINを送信。
4.  **【API → クライアント】** 成功レスポンスからセッショントークン (`a1b2c3d4...`) を受け取る。
5.  **【クライアント】** このトークンをメモリ上に保存する。
6.  **【ゲームプレイ】** プレイヤーが楽曲をプレイし、リザルトが確定する。
7.  **【クライアント → API】** `POST /upload_result.cgi` を呼び出す。この時、HTTPヘッダーに `Authorization: Bearer a1b2c3d4...` を付けて、曲名・難易度・スコアを送信する。
8.  **【ゲーム画面】** 「スコアを保存しました！」と表示。