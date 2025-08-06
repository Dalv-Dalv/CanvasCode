namespace CanvasCode.Models.CanvasWindows;

public interface ICanvasContentState { }

public struct CanvasCodeEditorState : ICanvasContentState {
	public string? FilePath;
}

public struct CanvasFolderTreeState : ICanvasContentState {
	public string? RootPath;
}