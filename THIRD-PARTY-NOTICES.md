# Third-party notices

SattoMDV自身のMITライセンスは、下記の第三者コンポーネントの条件を置き換えません。配布時は本書およびlicensesフォルダーを保持してください。

| コンポーネント | バージョン | ライセンス原文 |
| --- | --- | --- |
| Markdig | 1.3.2 | [BSD-2-Clause](licenses/Markdig-LICENSE.txt) |
| Microsoft.Web.WebView2 SDK | 1.0.4191.47 | [BSD-3-Clause](licenses/WebView2-LICENSE.txt)、[第三者通知](licenses/WebView2-NOTICE.txt) |
| .NET apphost | 10.0.10 | [MIT](licenses/dotnet-LICENSE.txt)、[第三者通知](licenses/dotnet-THIRD-PARTY-NOTICES.txt) |

Markdigの原文は[採用バージョンのソース](https://github.com/xoofx/markdig/blob/fc705234fa211d179ee1d5e7656b51ab99f70ca9/license.txt)から取得しました。
WebView2の原文は[NuGetパッケージ1.0.4191.47](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.4191.47)に含まれるLICENSE.txtとNOTICE.txtを変更せず保存しています。
.NETの原文は[dotnet/runtime v10.0.10](https://github.com/dotnet/runtime/tree/v10.0.10)から取得しました。実行ファイルに含まれるapphost用です。

.NET Desktop RuntimeおよびWebView2 Evergreen Runtime自体は同梱せず、利用者がMicrosoftから別途導入します。ビルド環境や依存バージョンを変更して再配布する場合は、そのバージョンのライセンスと通知も確認・更新してください。

Obsidian本体、第三者のObsidianテーマ、フォントは同梱していません。samples/ink.theme.cssは本プロジェクト独自のサンプルです。
