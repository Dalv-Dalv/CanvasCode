using System.Collections.ObjectModel;

namespace CanvasCode.ViewModels.CanvasWindow;

public class FolderCanvasWindowViewModel : CanvasWindowViewModelBase {
	public ObservableCollection<FileNodeViewModel> folders { get; }= [];

	public FolderCanvasWindowViewModel(FileNodeViewModel folder) {
		folders.Add(folder);
	}
}