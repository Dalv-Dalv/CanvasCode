using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasCodeEditorViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	[ObservableProperty] private string codeText = "";
	[ObservableProperty] private string fileName = "untitled";
	
	public string GetTitle() {
		return  "Code Editor";
	}
}