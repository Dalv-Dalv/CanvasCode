using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindows;

public class CanvasFolderTreeViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	public ObservableCollection<string> Files { get; } = [];

	public string GetTitle() {
		return "Folder Tree";
	}
}