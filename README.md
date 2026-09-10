# SattoMDV — さっと表示する Markdown Viewer

[English](README.en.md) | **日本語**

Windows向けの、文章を読むためのMarkdownビューア。C# / .NET 10 / WPF / WebView2 / Markdigで実装しています。

## ダウンロードと必要環境

Windows 10/11のx64環境向けです。ARM64と32bit Windows向けの配布版はありません。

1. [GitHubのReleases](https://github.com/Yasosuke/SattoMDV/releases/latest)を開き、最新版の **Assets** から `SattoMDV-1.1.0-win-x64.zip` をダウンロードします。`Source code` は開発者向けのソースコードです。
2. [Microsoft公式の.NET 10ダウンロードページ](https://dotnet.microsoft.com/ja-jp/download/dotnet/10.0)で **.NET Desktop Runtime → Windows → x64** をインストールしてください。実行にはSDKは不要ですが、通常の.NET RuntimeやASP.NET Core Runtimeだけでは不足します。既にDesktop Runtime 10がある場合は再インストール不要です。
3. [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/ja-jp/microsoft-edge/webview2/)が未導入の場合は、Evergreen Runtimeをインストールしてください。
4. ZIPを右クリックして「すべて展開」を選び、展開先の `SattoMDV.exe` を起動します。ZIP内から直接起動したり、EXEだけを取り出したりしないでください。

アプリ本体のインストーラーはありません。共有ランタイムは配布ZIPに含まれません。配布物はコード署名されていないため、Windowsが警告を表示する場合があります。入手先とファイル名を確認してください。

## 使い方

配布ZIPを展開し、`SattoMDV.exe` をダブルクリックしてください。移動する場合は **展開したフォルダー全体** をコピーしてください。

- **右クリック → ファイルを開く** または **Ctrl+O** で `.md` / `.markdown` / `.txt` を開きます。上部ツールバーはありません。
- 本文・アウトライン・下部の案内欄を含むウィンドウ内全体へのドラッグ＆ドロップ、実行ファイルへのドロップ、コマンドライン引数に対応します。
- **F5** で再読込、**Ctrl+F** で本文検索。
- ウィンドウを閉じると終了します。トレイ、常駐サービス、自動起動、ファイル監視、バックグラウンド更新はありません。

```powershell
.\dist\SattoMDV\SattoMDV.exe "C:\Documents\読書メモ.md"
```

Windowsの「プログラムから開く」でSattoMDV.exeを選ぶと、Markdownを直接開けます。関連付けの自動変更は行いません。

## 表示言語

右クリック →「テーマ・表示設定」を開き、「読み心地を整える」の下にある **English / 日本語** のラジオボタンで切り替えます。設定画面はその場で切り替わり、「保存して適用」でメニュー・案内文・初期ページにも反映され、次回起動でも維持されます。「キャンセル」では元の言語と設定に戻ります。既定は日本語です。

文書の内容、CSS、ファイルパス、フォント名は変更しません。Windows標準のダイアログやシステム由来のエラーメッセージは、Windowsの表示言語に従う場合があります。

## テーマと表示設定

設定画面の一番上に「一括設定」があります。文字色・背景色・文字サイズ・フォントを本文全体に指定できます。要素別に指定した項目は個別設定が優先されます。全体で統一する場合は、個別側の該当項目を空欄（色は「テーマを継承」）にしてください。

「本文の上余白」と「アウトラインの上余白」は、それぞれ0～500pxで設定できます。左右・下側の余白は変わりません。本文の既定値は44px、アウトラインは20pxです。本文の上余白は文書先頭の余白で、スクロール中に固定表示される帯ではありません。

フォント一覧は、システムのフォントファミリーだけでなく、日本語などの別名、書体のWin32ファミリー名、ユーザー別インストール先と登録されたフォントファイルも確認します。フォントファミリーは太字・斜体の各ファイルと同じ件数にはなりません。フォント管理ソフト内で無効なものや、ブラウザーが対応しない形式は使えない場合があります。フォントの追加後はアプリを再起動してください。

右クリック →「テーマ・表示設定」でObsidianの `.obsidian/themes/テーマ名/theme.css` を選択してください。ライト／ダークはチェックボックスで切り替えます。サンプルの `samples/ink.theme.css` も使用できます。

「要素ごとの見た目」には、本文・見出し・リンク・引用・リスト・コード・表などの設定行があります。文字色と背景色のボタンからカラーピッカー（パレット・RGBスライダー・カラーコード入力）を開けます。文字サイズはプルダウン、フォントはPCのインストール済みフォント一覧から選び、「保存して適用」を押します。文字サイズとフォントは直接入力も可能です。空欄はテーマを継承し、カラーピッカーの「テーマを継承」で色の指定を解除できます。

## アウトライン

右クリック →「アウトラインを表示」で、左側に見出し一覧が現れます。見出しを選ぶと本文の該当箇所へ移動します。h1～h6の階層を字下げで表示し、文書の切替・再読込で更新します。「アウトラインを非表示」で本文だけに戻せます。表示状態は次回起動にも引き継がれます。

下部には常に「ファイルをひらくにはウィンドウへドロップしてください/右クリックでメニュー表示」を表示します。開いているファイルのフルパスはタイトルバーで確認できます。右クリック →「このファイルの場所を開く」で、そのファイルが入っているフォルダーをエクスプローラーで開きます。ファイルを開いていないときは無効になります。

### 要素別設定の対応表

| 要素 / セレクター | 対象 |
| --- | --- |
| body | 文書全体の基準 |
| p | 段落 |
| h1 ～ h6 | 見出しレベル1～6 |
| a / strong / em | リンク / 太字 / 斜体 |
| blockquote | 引用 |
| ul / ol / li | 箇条書き / 番号付きリスト / リスト項目 |
| li::marker | リストの記号 |
| code / pre | インラインコード / コードブロック |
| table / th / td | 表 / 見出しセル / 通常セル |
| hr / img | 区切り線 / 画像 |

サイズは `20px`、`1.2em` など、色は `#334455`、`red`、`transparent` など、フォントは `Yu Mincho`、`Meiryo`、`"Yu Gothic UI", sans-serif` などを指定します。PCにインストールされたフォントが使えます。

最下行に任意のCSSセレクターを追加でき、描画されるすべてのHTML要素と疑似要素を指定できます。不要な行は左端の行選択欄を選んでDelete。入力したCSS値の妥当性はブラウザーが判断し、無効な値は無視されます。

「追加CSS」では、行間・幅・余白・枠線なども自由に編集できます。

```css
body { --file-line-width: 680px; --line-height-normal: 2; }
.markdown-rendered p { letter-spacing: 0.03em; }
blockquote { border-left-color: #b08050; }
```

適用順序は **標準CSS → Obsidianテーマ → 一括設定 → 要素別設定 → 上余白設定 → 追加CSS**。要素別設定には `!important` が付くため、追加CSSで同じ項目を上書きする場合は同等以上の詳細度と `!important` が必要です。要素別設定の空欄化でもテーマに戻せます。「標準に戻す」はテーマ選択だけを解除します。

設定は `%LOCALAPPDATA%/SattoMDV/settings.json` に保存され、次回起動でも維持されます。テーマCSSは元のファイルを参照します。テーマを移動・削除した場合は設定から選び直してください。WebView2のキャッシュは同じ場所の `WebView-Native-v1` フォルダーです。

SattoMDV側の設定がまだない場合は、旧版の `%LOCALAPPDATA%/SMDV/settings.json`、次に `%LOCALAPPDATA%/MDV/settings.json` を探して引き継ぎます。次に保存するとSattoMDV側に保存します。旧設定は削除しません。開発用の保存先指定は `SATTOMDV_DATA_DIR`、互換用に `SMDV_DATA_DIR` と `MDV_DATA_DIR` も利用できます。ブラウザーキャッシュはSattoMDV専用の保存先となり、旧版との同時起動時も競合しません。

## 対応範囲

通常のMarkdownに加え、表、チェックリスト、取り消し線、ハイライト、見出しIDをサポート。数式、シンタックスハイライト、Obsidianのプラグイン、Wikiリンク、埋め込み、コールアウト専用記法は未対応です。

Obsidianの主要なCSS変数と閲覧用クラスを用意していますが、Obsidian本体のDOMや全変数を完全に再現するものではありません。編集画面やStyle Settingsプラグイン専用の設定、テーマ独自のDOMに依存する装飾には非対応です。テーマは閲覧本文に適用され、Windowsのツールバーや設定ダイアログは対象外です。

Markdown内の生HTMLとスクリプトは無効です。画像は文書と同じフォルダーおよびその配下の相対パス、またはdata URLに対応します。ネット上の画像・CSS・フォント、CSSの外部importは読み込みません。通常のWebリンクはクリックすると既定ブラウザーで開きます。

## ダウンロードと必要環境

Windows 10/11のx64環境向けです。ARM64と32bit Windows向けの配布版はありません。

1. [GitHubのReleases](https://github.com/Yasosuke/SattoMDV/releases/latest)を開き、最新版の **Assets** から `SattoMDV-1.1.0-win-x64.zip` をダウンロードします。`Source code` は開発者向けのソースコードです。
2. [Microsoft公式の.NET 10ダウンロードページ](https://dotnet.microsoft.com/ja-jp/download/dotnet/10.0)で **.NET Desktop Runtime → Windows → x64** をインストールしてください。実行にはSDKは不要ですが、通常の.NET RuntimeやASP.NET Core Runtimeだけでは不足します。既にDesktop Runtime 10がある場合は再インストール不要です。
3. [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/ja-jp/microsoft-edge/webview2/)が未導入の場合は、Evergreen Runtimeをインストールしてください。
4. ZIPを右クリックして「すべて展開」を選び、展開先の `SattoMDV.exe` を起動します。ZIP内から直接起動したり、EXEだけを取り出したりしないでください。

アプリ本体のインストーラーはありません。共有ランタイムは配布ZIPに含まれません。配布物はコード署名されていないため、Windowsが警告を表示する場合があります。入手先とファイル名を確認してください。

## 動作について

- Windows 10/11（x64）、.NET 10 Desktop Runtime、Microsoft Edge WebView2 Runtime。
- アプリのウィンドウを先に作り、WebView2初期化と文書読込を非同期で実行します。ローカルサーバーやNode.jsは起動しません。
- WebView2は表示中に複数の子プロセスを使います。ウィンドウ終了時にWebView2をDisposeし、子プロセスも終了します。

## ビルドと検証

```powershell
.\build.ps1
.\test.ps1
.\test-language.ps1
```

.NET 10 SDKが必要です。初回ビルド時はNuGetから依存ライブラリを取得します。テストは実アプリを起動し、標準表示、Obsidian形式のテーマ、文字色・背景・フォント・サイズの優先順位、65,000段落の文書、生HTMLの無効化を検証します。UIテストはブラウザーへの右クリック入力、アウトラインの切替・移動・保存、カラーピッカーとプルダウンの選択・保存、本文への実ファイルドロップ入力と、案内欄・アウトラインからのWPFドロップイベント経路を検証します。結果と描画画像は `.test-data` に保存します。`SATTOMDV_DATA_DIR` 環境変数で設定保存先を分離するため、テストは通常のユーザー設定を変更しません。

技術資料：[WebView2 WPF](https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/wpf)、[Obsidian CSS変数](https://docs.obsidian.md/Reference/CSS%20variables/About%20styling)。


## 設定の削除・アンインストール

アプリを終了し、展開したフォルダーを削除してください。設定とキャッシュも削除する場合は、エクスプローラーのアドレス欄に `%LOCALAPPDATA%\SattoMDV` と入力し、そのフォルダーを削除します。共有ランタイムは他のアプリでも使用されるため、SattoMDVの削除時に削除する必要はありません。

## ライセンス

SattoMDVのソースコードと同梱サンプルは[MITライセンス](LICENSE)で公開しています。改変・再配布・商用利用が可能です。著作権表示とライセンス文を維持してください。

依存ライブラリにはそれぞれのライセンスが適用されます。[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)と[licenses](licenses/)に原文と出典をまとめています。利用者が追加するObsidianテーマやフォントは同梱していません。それらを再配布する場合は、各配布元の条件を別途確認してください。


