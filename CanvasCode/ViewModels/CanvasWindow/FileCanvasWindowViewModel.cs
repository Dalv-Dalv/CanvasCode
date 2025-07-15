using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindow;

public partial class FileCanvasWindowViewModel : CanvasWindowViewModelBase {
	[ObservableProperty] private FileNodeViewModel file;
	[ObservableProperty] private string fileContent;

	public FileCanvasWindowViewModel(FileNodeViewModel file) {
		Type = WindowType.File;
		this.file = file;

		fileContent = System.IO.File.ReadAllText(file.FilePath);
	}
}