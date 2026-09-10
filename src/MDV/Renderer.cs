using System.IO;
using System.Text;
using Markdig;
namespace MDV;
public static class Renderer
{
    static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseTaskLists().UseAutoIdentifiers().UseEmphasisExtras().DisableHtml().Build();
    static readonly Lazy<string> BaseSheet = new(() => Sheet(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "reader.css"))));
    static string Sheet(string value) => "<link rel=\"stylesheet\" href=\"data:text/css;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) + "\">";
    public const string Welcome = """
        # 文章を、気持ちよく読む。

        **SattoMDV — さっと表示する Markdown Viewer** は、読むことに集中するための Markdown ビューアです。

        ファイルをウィンドウへドロップしてください。**右クリック** のメニュー、または **Ctrl + O** からも開けます。

        ## あなたに合う読み心地

        右クリックの「テーマ・表示設定」から Obsidian の `theme.css` を読み込めます。色はカラーピッカー、文字サイズとフォントはプルダウンで変更できます。

        > 少し大きな文字。心地よい余白。好きな色。
        > いつもの文章を、読みやすい姿に。

        ### できること

        - 見出し・強調・引用・リスト・リンク・画像
        - 表・チェックリスト・コードブロック
        - ライト／ダーク切替と独自 CSS
        - 右クリックでアウトラインを表示・非表示

        | 操作 | ショートカット |
        | --- | --- |
        | ファイルを開く | Ctrl + O |
        | 再読込 | F5 |
        | 本文を検索 | Ctrl + F |

        閉じると終了します。常駐プロセスやファイル監視はありません。
        """;
    public static string Render(string markdown, Preferences prefs) {
        var theme = string.IsNullOrWhiteSpace(prefs.ThemePath) ? "" : File.ReadAllText(prefs.ThemePath);
        var css = new StringBuilder();
        void Style(string selector, ElementStyle s) {
            css.Append(selector).Append('{');
            void Add(string name, string value) { if (!string.IsNullOrWhiteSpace(value)) css.Append(name).Append(':').Append(value).Append(" !important;"); }
            Add("color", s.Color); Add("background-color", s.Background); Add("font-size", s.Size); Add("font-family", s.Font);
            css.Append('}');
        }
        // Zero specificity allows individual element settings to override the common style.
        Style(":where(body, .markdown-rendered, .markdown-rendered *)", prefs.GlobalStyle);
        foreach (var s in prefs.Elements) {
            if (string.IsNullOrWhiteSpace(s.Selector)) continue;
            Style(s.Selector, s);
        }
        css.Append(".markdown-preview-view {padding-top:").Append(Preferences.NormalizeMargin(prefs.BodyTopMargin, 44).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("px !important;} .markdown-preview-section > :first-child {margin-top:0 !important;}");
        css.Append(prefs.CustomCss);
        return "<!doctype html><html lang=\"ja\"><head><meta charset=\"utf-8\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src data: 'unsafe-inline'; img-src https://assets.mdv.invalid data:; font-src data:; script-src 'none'; base-uri https://assets.mdv.invalid; form-action 'none'\"><base href=\"https://assets.mdv.invalid/\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" + BaseSheet.Value + Sheet(theme) + Sheet(css.ToString()) + "</head><body class=\"" + (prefs.Dark ? "theme-dark" : "theme-light") + "\"><div class=\"app-container\"><div class=\"workspace\"><div class=\"workspace-leaf-content\" data-type=\"markdown\"><div class=\"markdown-reading-view\"><main class=\"markdown-preview-view markdown-rendered\"><article class=\"markdown-preview-sizer markdown-preview-section\">" + Markdown.ToHtml(markdown, Pipeline) + "</article></main></div></div></div></div></body></html>";
    }
}


