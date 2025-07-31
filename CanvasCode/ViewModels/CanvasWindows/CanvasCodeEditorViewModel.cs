using System.Collections.Generic;
using AvaloniaEdit.Document;
using CanvasCode.Models.CommandPalettes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasCodeEditorViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	public CanvasWindowViewModel ParentWindow { get; }

	[ObservableProperty] private TextDocument currentDocument;
	
	public CanvasCodeEditorViewModel() { //FOR DESIGN VIEW ONLY
		ParentWindow = null!;
		CurrentDocument = new TextDocument("public string GetTitle() {\n\treturn  \"Code Editor\";\n}\n\npublic void SetData(object data) {\n\t//TODO WIP\n}\n\npublic List<CommandPaletteItem> GetQuickActions() {\n\treturn [\n\t\tnew CommandPaletteItem(\"Open File\", command: new RelayCommand(OpenFile))\n\t];\n}");
	}
	
	public CanvasCodeEditorViewModel(CanvasWindowViewModel parentWindow) {
		ParentWindow = parentWindow;
		CurrentDocument = new TextDocument("public string GetTitle() {\n\treturn  \"Code Editor\";\n}\n\npublic void SetData(object data) {\n\t//TODO WIP\n}\n\npublic List<CommandPaletteItem> GetQuickActions() {\n\treturn [\n\t\tnew CommandPaletteItem(\"Open File\", command: new RelayCommand(OpenFile))\n\t];\n}");
	}
	
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