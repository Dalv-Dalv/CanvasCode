using System;
using System.Collections.Generic;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasShaderPreviewViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	public CanvasWindowViewModel ParentWindow { get; }
	[ObservableProperty] private string shaderPath;
	
	public event Action<ICanvasContentState>? OnStateChanged;

	public CanvasShaderPreviewViewModel(CanvasWindowViewModel parentWindow) {
		ParentWindow = parentWindow;
	}
	

	public string GetTitle() {
		return "Shader Preview";
	}

	public void SetData(object data) {
		// TODO
	}

	public ICanvasContentState? GetState() {
		// TODO
		return null;
	}
	public void SetState(ICanvasContentState state) {
		// TODO
	}


	public List<CommandPaletteItem> GetQuickActions() {
		return [
			new CommandPaletteItem("Pause/Unpause", command: new RelayCommand(TogglePauseState)),
			new CommandPaletteItem("Reset Time", command: new RelayCommand(ResetTime))
		];
	}

	public void TogglePauseState() {
		// TODO WIP
	}

	public void ResetTime() {
		// TODO WIP
	}
}