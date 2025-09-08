using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Others;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasCodeEditorViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	public CanvasWindowViewModel ParentWindow { get; }

	[ObservableProperty] private TextDocument? currentDocument = new("Hello");
	[ObservableProperty] private string? pathToCurrentDocument;

	[ObservableProperty] private IHighlightingDefinition currentSyntaxHighlighting;

	public event Action<ICanvasContentState>? OnStateChanged;
	
	public CanvasCodeEditorViewModel() { //FOR DESIGN VIEW ONLY
		ParentWindow = null!;
		CurrentDocument = new TextDocument("public string GetTitle() {\n\treturn  \"Code Editor\";\n}\n\npublic void SetData(object data) {\n\t//TODO WIP\n}\n\npublic List<CommandPaletteItem> GetQuickActions() {\n\treturn [\n\t\tnew CommandPaletteItem(\"Open File\", command: new RelayCommand(OpenFile))\n\t];\n}");
	}
	
	public CanvasCodeEditorViewModel(CanvasWindowViewModel parentWindow) {
		ParentWindow = parentWindow;
	}
	
	public string GetTitle() {
		return  "Code Editor";
	}

	public void SetData(object data) {
		switch (data) {
			case string file:
				OpenDocumentFromPath(file);
				break;
			case string[] files:
				OpenDocumentFromPath(files[0]);
				break;
		}
	}

	public ICanvasContentState? GetState() {
		return new CanvasCodeEditorState { FilePath = PathToCurrentDocument };
	}
	public void SetState(ICanvasContentState state) {
		if (state is not CanvasCodeEditorState codeEditorState) return;
		if (codeEditorState.FilePath == null) return;

		OpenDocumentFromPath(codeEditorState.FilePath);
	}

	public List<CommandPaletteItem> GetQuickActions() {
		return [
			new CommandPaletteItem("Open File", command: OpenFileCommand)
		];
	}
	

	[RelayCommand]
	public async Task OpenFile() {
		App.Messenger.Send(new RequestFocusMessage(ParentWindow));
		
		var files = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
			Title = "Select file", AllowMultiple = false
		});
		if (files.Count <= 0) return;
		
		var path = files[0].TryGetLocalPath();
		if (path == null) return;

		OpenDocumentFromPath(path);
	}

	public void OpenDocumentFromPath(string path) {
		if (CurrentDocument != null) {
			//TODO: Check for unsaved changes
		}
		
		CurrentDocument = new TextDocument(File.ReadAllText(path));
		PathToCurrentDocument = path;

		CurrentSyntaxHighlighting = LoadHighlightingDefinition(path);
	}

	private IHighlightingDefinition LoadHighlightingDefinition(string filePath) {
		var assemblyName = typeof(App).Assembly.GetName().Name;
		var uri = new Uri($"avares://{assemblyName}/Assets/SyntaxHighlighters/CSharp-Mode.xshd");

		using var stream = AssetLoader.Open(uri);
		if (stream == null) throw new InvalidOperationException($"Resource not found at URI: {uri}");
		
		using var reader = new XmlTextReader(stream);
		var syntaxDefinition = HighlightingLoader.LoadXshd(reader);
		var highlighting = HighlightingLoader.Load(syntaxDefinition,  HighlightingManager.Instance);

		return highlighting;
	}

	partial void OnPathToCurrentDocumentChanged(string? value) {
		OnStateChanged?.Invoke(GetState());
	}
}