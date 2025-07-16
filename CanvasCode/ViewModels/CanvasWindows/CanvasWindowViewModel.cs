using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindows;

public enum CanvasWindowType {
	CodeEditor, ShaderPreview, FolderTree
}


public partial class CanvasWindowViewModel : ViewModelBase {
	[ObservableProperty] Point position;
	[ObservableProperty] Size size;
	[ObservableProperty] string title;

	[ObservableProperty] CanvasWindowType selectedType;
	public IEnumerable<CanvasWindowType> AvailableTypes { get; } = Enum.GetValues<CanvasWindowType>();
	
	[ObservableProperty] ICanvasWindowContentViewModel currentContent;
	Dictionary<CanvasWindowType, ICanvasWindowContentViewModel> contentStates;

	public CanvasWindowViewModel() {
		//TODO: Dont store the entire view model, store only the bare minimum for reinitializing the viewModels upon switching
		
		contentStates = new Dictionary<CanvasWindowType, ICanvasWindowContentViewModel> {
			{CanvasWindowType.CodeEditor, new CanvasCodeEditorViewModel()},
			{CanvasWindowType.ShaderPreview, new CanvasShaderPreviewViewModel()},
			{CanvasWindowType.FolderTree, new CanvasFolderTreeViewModel()}
		};

		SelectedType = CanvasWindowType.CodeEditor;
		OnSelectedTypeChanged(CanvasWindowType.CodeEditor);
	}

	partial void OnSelectedTypeChanged(CanvasWindowType value) {
		CurrentContent = contentStates[value];
		Title = CurrentContent.GetTitle();
	}
}