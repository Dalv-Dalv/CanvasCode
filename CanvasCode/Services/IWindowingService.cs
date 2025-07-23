namespace CanvasCode.Services;

public interface IWindowingService {
	void EnterFullscreen();
	void ExitFullscreen();
	bool IsFullscreen { get; }
}