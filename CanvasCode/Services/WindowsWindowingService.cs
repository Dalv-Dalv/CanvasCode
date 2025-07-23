using Avalonia.Controls;

namespace CanvasCode.Services;

public class WindowsWindowingService(Window mainWindow) : IWindowingService {
	private readonly Window mainWindow = mainWindow;

	public bool IsFullscreen => mainWindow.WindowState == WindowState.FullScreen;

	private WindowState? prevState = null;
	public void EnterFullscreen() {
		prevState = mainWindow.WindowState;
		mainWindow.WindowState = WindowState.FullScreen;
	}
	public void ExitFullscreen() {
		mainWindow.WindowState = prevState ?? WindowState.Normal;
		prevState = null;
	}
}