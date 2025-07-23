using System.Collections.Generic;
using CanvasCode.Models.CommandPalettes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasCodeEditorViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	[ObservableProperty] private string codeText = "";
	[ObservableProperty] private string fileName = "untitled";
	
	public string GetTitle() {
		return  "Code Editor";
	}

	public void SetData(object data) {
		//TODO WIP
	}

	public List<CommandPaletteItem> GetQuickActions() {
		return [
			new CommandPaletteItem("Open File", command: new RelayCommand(OpenFile))
		];
	}

	public void OpenFile() {
		// TODO WIP
	}
}