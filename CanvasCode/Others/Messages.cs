using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Others;

public sealed record EnterFullscreenMessage(ICanvasWindowContentViewModel ContentToDisplay);
public sealed record ExitFullscreenMessage();
public sealed record FolderContentsChangedMessage(string path);
public sealed record FileRenamedMessage(string parentPath, string oldFullPath, string newFullPath);