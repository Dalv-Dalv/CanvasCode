# CanvasCode

A desktop app for developers who think spatially. Inspired by [PureRef](https://www.pureref.com/) and [The Farmer Was Replaced](https://store.steampowered.com/app/2060160/The_Farmer_Was_Replaced/), CanvasCode replaces the traditional single-window IDE layout with a free-form canvas where you can arrange code editors, file trees, and shader previews however you like.

> **Very early in development** expect rough edges and lots of missing features.

---

## What it is

Instead of docked panels and fixed layouts, CanvasCode gives you a canvas. You drag floating windows around, resize them freely, pin them to the side, and stack tabs inside each one. Each window can independently show a code editor, a folder tree, or a live GLSL shader preview. Files can be dropped directly from the OS or from within the file tree.

---

## Building & Running

**Requirements**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Windows (currently the only supported platform windowing service is Windows-specific)

**Clone**
```bash
git clone <repo-url>
cd CanvasCode-master
```

**Run (debug)**
```bash
dotnet run --project CanvasCode/CanvasCode.csproj
```

**Build (release)**
```bash
dotnet build CanvasCode/CanvasCode.csproj -c Release
```

**Publish self-contained**
```bash
dotnet publish CanvasCode/CanvasCode.csproj -c Release -r win-x64 --self-contained
```

The project references a local copy of `AvaloniaEdit` (in `AvaloniaEdit-master/`) rather than the NuGet package, so that folder must be present alongside `CanvasCode/`. Everything else is pulled via NuGet on first build.

---

## What's working

### Canvas
- Free-form canvas with pan and zoom
- Floating windows, drag to move, drag edges/corners to resize
- Z-order management (bring to front on focus)
- Pin a window to the sidebar so it stays fixed while the canvas moves
- Fullscreen mode, expand any window content to fill the whole screen

### Window tabs
- Each canvas window supports multiple tabs
- Tabs preserve their scroll/caret state when switching (via a cache with LRU eviction)
- Closing the last tab closes the window

### Quick actions menu
- Per-window popup palette with general and content-specific actions
- Toggle header visibility, toggle fullscreen, open new tab, close window
- Sub-menus (e.g. pick new tab type from a nested list)

### Code editor
- Powered by a local build of [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
- Open files via the quick actions menu or by dropping a file onto the canvas / folder tree
- Syntax highlighting (C# `.xshd` definition bundled)
- Tab state persists the open file path when switching away and back

### Folder tree
- Open any folder via the quick actions menu or drag-and-drop from the OS
- Lazy-loading: children are only read from disk when you expand a node
- Loaded children are cached; collapsed nodes schedule a prune after 10 minutes of inactivity
- Alphabetical sort with directories listed first
- Live file system watcher — tree updates automatically when files change on disk
- Periodic revalidation every 5 seconds detects if the root folder has been deleted or moved
- Context menu per node: Add File, Add Folder, Rename, Delete, Refresh
- Drag files between nodes to move them on disk
- Drop files/folders from the OS into the tree root

### Shader preview
- Real-time GLSL fragment shader rendering via Avalonia's composition layer (SkiaSharp backend)
- Animated (`iTime` uniform), configurable frame rate
- Stretch modes (Uniform, Fill, etc.)
- Custom uniforms dictionary

### Dialogs
- `InputNameDialog` — text input with validation (used for create file/folder and rename)
- `ConfirmDialog` — yes/no prompt

### Drag and drop
- Drop a **folder** anywhere on the canvas → opens a Folder Tree window at the drop position
- Drop a **file** anywhere on the canvas → opens a Code Editor window at the drop position
- Drop files **onto a folder tree node** → moves them on disk into that directory

---

## What's missing / WIP

| Area | Status | Notes |
|---|---|---|
| Save / Save As | ❌ Missing | No write-back to disk; editor is effectively read-only |
| Unsaved changes tracking | ⚠️ WIP | No dirty flag, no prompt when closing a modified tab |
| Multiple folder roots | ⚠️ WIP | Folder Tree only supports one root at a time; multiple roots are stubbed |
| Syntax highlighting per file type | ⚠️ Hardcoded | C# highlighter is always loaded regardless of the file extension |
| Canvas state persistence | ❌ Missing | Window positions, sizes, and tabs are not saved on exit |
| Command palette search | ❌ Missing | Quick actions are index-navigated only; no text filtering |
| Shader ↔ editor integration | ❌ Missing | No live-edit loop between the code editor and the shader preview |
| Cross-platform support | ❌ Missing | `WindowsWindowingService` is Windows-only; no Linux/macOS implementation |
| Tab close crash edge case | ⚠️ Known bug | `ActiveTabIndex` adjustment after closing a tab may behave incorrectly in some orderings |
| Accessibility | ❌ Not started | No keyboard-only navigation for the canvas, no screen-reader support |

---

## Tech stack

| | |
|---|---|
| UI framework | [Avalonia 11](https://avaloniaui.net/) |
| Text editor | [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (local build) |
| MVVM | [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) |
| Canvas pan/zoom | [PanAndZoom](https://github.com/wieslawsoltes/PanAndZoom) |
| DI / hosting | Microsoft.Extensions.Hosting |
| Shader rendering | SkiaSharp via Avalonia composition |
| Target framework | .NET 9 / WinExe |

---

## License

MIT
