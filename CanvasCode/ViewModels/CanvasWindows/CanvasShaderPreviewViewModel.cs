using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasShaderPreviewViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	[ObservableProperty] private string shaderPath;

	public string GetTitle() {
		return "Shader Preview";
	}
}