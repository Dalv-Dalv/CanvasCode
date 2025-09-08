namespace CanvasCode.Models.CanvasWindows;

public interface ICanvasContentState { }

public struct CanvasCodeEditorState(string? filePath) : ICanvasContentState {
	public string? FilePath = filePath;
}

public struct CanvasFolderTreeState(string? rootPath) : ICanvasContentState {
	public string? RootPath = rootPath;
}