using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindow;

public enum WindowType { File, Folder, ShaderPreview, Split }

public abstract partial class CanvasWindowViewModelBase : ViewModelBase {
	[ObservableProperty] private double posX;
	[ObservableProperty] private double posY;
	[ObservableProperty] private string title;
	[ObservableProperty] private WindowType type;
}