# SattoMDV — A simple Markdown viewer

[日本語](README.md) | **English**

SattoMDV is a Windows Markdown viewer designed for comfortable, distraction-free reading. **Satto (さっと)** is a Japanese onomatopoeic expression for a quick, light movement—reflecting the idea of opening a document and getting straight to reading.

## Download and setup

1. Open the [latest release](https://github.com/Yasosuke/SattoMDV/releases/latest) and download **SattoMDV-1.1.1-win-x64.zip** from **Assets**. The files labeled **Source code** are for developers.
2. Install **.NET Desktop Runtime 10 for Windows x64** from [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) if it is not already installed. Choose **Desktop Runtime**, not just .NET Runtime or ASP.NET Core Runtime. The SDK is not needed to run the app.
3. Install the [Microsoft Edge WebView2 Evergreen Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) if needed.
4. Extract the entire ZIP, then run **SattoMDV.exe** from the extracted folder. Keep the files together; do not run the app from inside the ZIP or copy only the EXE.

The release targets **Windows 10/11, x64**. There are no ARM64 or 32-bit builds. The app has no installer; shared runtimes are installed separately and are not included in the ZIP. The executable is unsigned, so Windows may display a warning. Check the download source and file name before proceeding.

## Choose a language

The default interface is Japanese. Right-click the viewer and select **テーマ・表示設定** (Theme & appearance). Under **読み心地を整える**, select the **English** radio button. The settings dialog changes immediately. Click **Save & apply** to switch the viewer and remember the language for next time.

Choose **日本語** to switch back. **Cancel** discards the language change and other unsaved edits. This changes application captions and the built-in welcome page; your documents, CSS, file paths and font names remain unchanged. Native Windows dialogs and system-provided error messages may follow your Windows language.

## Read a document

- Drop a `.md`, `.markdown` or `.txt` file anywhere in the window, or use **Right-click → Open file** / **Ctrl+O**.
- You can also drop a document onto the executable or pass its path as a command-line argument.
- Press **F5** to reload and **Ctrl+F** to search the document.
- Use **Show outline / Hide outline** in the right-click menu to toggle a heading list. Click a heading to jump to it.
- **Open file location** opens the document's containing folder in Explorer. It is disabled until a file is open.
- The title bar shows the current document's full path.

There is no top toolbar. Closing the window exits the app; there is no tray app, background service, automatic startup, file watching or background updater.

```powershell
.\SattoMDV.exe "C:\Documents\notes.md"
```

You can select SattoMDV.exe in Windows **Open with** to open Markdown files directly. The app does not change file associations automatically.

## Themes and appearance

Open **Theme & appearance** from the right-click menu. Load an Obsidian theme's `.obsidian/themes/<theme name>/theme.css`, or try the original sample `samples/ink.theme.css`. Use **Dark mode** to select the dark appearance. **Use default theme** clears the selected theme file.

**Global appearance** sets text color, background, font size and font for all document elements. Element-specific settings take priority. Leave a value blank to inherit the theme; choose **Inherit theme** in a color picker to clear a color override.

Choose colors with a palette, RGB sliders or a color code. Font size and font controls are editable dropdowns. Fonts come from those installed on your computer, including localized family names where available. Aliases and styles do not correspond one-to-one with font files. Disabled fonts or formats unsupported by the rendering engine may be unavailable. Restart the app after installing fonts.

**Document top margin** and **Outline top margin** independently accept 0–500 px. They affect only the top; default values are 44 and 20 px. The document margin is at the beginning of the document, not a fixed strip during scrolling.

### Element settings

| Selector | Content |
| --- | --- |
| body / p | Document / paragraphs |
| h1–h6 | Headings |
| a / strong / em | Links / bold / italic |
| blockquote | Quotes |
| ul / ol / li / li::marker | Lists, items and markers |
| code / pre | Inline code / code blocks |
| table / th / td | Tables and cells |
| hr / img | Horizontal rules / images |

You can add arbitrary CSS selectors in the final row, including pseudo-elements. Select a row header and press Delete to remove it. Invalid CSS values are ignored by the rendering engine.

Use **Custom CSS** for line height, width, spacing, borders and other properties:

```css
body { --file-line-width: 680px; --line-height-normal: 2; }
.markdown-rendered p { letter-spacing: 0.03em; }
blockquote { border-left-color: #b08050; }
```

Styles are applied in this order: built-in CSS → Obsidian theme → global appearance → element settings → top margins → custom CSS. Element overrides use `!important`; overriding them in custom CSS requires sufficient specificity and `!important`, or clearing the corresponding element value.

## Supported Markdown and limitations

The app supports standard Markdown plus tables, task lists, strikethrough, highlighting and automatic heading IDs. Math rendering, syntax highlighting, Obsidian plugins, wiki links, embeds and dedicated callout syntax are not supported.

Common Obsidian CSS variables and reading-view classes are provided, but compatibility is partial. Themes relying on Obsidian's exact DOM, editor interface or Style Settings plugin may look different. Theme CSS affects document rendering, not native settings dialogs.

Raw HTML and scripts in Markdown are disabled. Images can use relative paths within the document's folder or its subfolders, or data URLs. Remote images, CSS and fonts, and external CSS imports, are blocked. Ordinary web links open in the default browser.

## Settings and removal

Settings are stored at `%LOCALAPPDATA%\SattoMDV\settings.json`. WebView2 caches are stored in the adjacent `WebView-Native-v1` folder. Theme files are referenced in their original location; select the theme again if you move or delete it.

When no SattoMDV settings exist, settings from earlier SMDV or MDV versions are imported if available. The old files are retained.

To remove the app, close it and delete its extracted folder. To remove settings and caches too, enter `%LOCALAPPDATA%\SattoMDV` in Explorer's address bar and delete that folder. Shared runtimes may be used by other apps and need not be removed.

## Build and test

Install the **.NET 10 SDK**, download or clone the source, and run the following in PowerShell from the repository root:

```powershell
.\build.ps1
.\test.ps1
.\test-language.ps1
```

The executable is generated in `dist/SattoMDV`. The first build downloads NuGet dependencies. Tests launch the real WPF/WebView2 application and require an interactive Windows desktop. Test data is isolated under `.test-data` using `SATTOMDV_DATA_DIR`; normal user settings are not modified. Legacy `SMDV_DATA_DIR` and `MDV_DATA_DIR` overrides are also supported.

## License

SattoMDV and its original samples are released under the [MIT License](LICENSE), allowing modification, redistribution and commercial use while retaining the copyright and license text.

Dependencies retain their own licenses. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and [licenses](licenses/) for notices and original terms. Third-party Obsidian themes and fonts are not bundled; check their own terms before redistributing them.

