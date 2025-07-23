using System;
using System.Collections.Generic;
using CanvasCode.Models.CommandPalettes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasShaderPreviewViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	[ObservableProperty] private string shaderPath;

	public string GetTitle() {
		return "Shader Preview";
	}

	public void SetData(object data) {
		// TODO WIP
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