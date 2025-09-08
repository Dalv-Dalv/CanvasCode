using System;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.ViewModels.CanvasWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public enum TabCallbackAction {
	ClickedOn, Close
}

public partial class CanvasWindowTabViewModel(Action<CanvasWindowTabViewModel, TabCallbackAction> callback, CanvasWindowType type, ICanvasContentState? state = null) : ObservableObject {
	[ObservableProperty] private CanvasWindowType type = type;
	[ObservableProperty] private ICanvasContentState? state = state;

	[ObservableProperty] private bool isOpen = false;


	public void SubscribeToViewModelEvents(ICanvasWindowContentViewModel viewModel) {
		viewModel.OnStateChanged += (newState) => {
			State = newState;
			Console.WriteLine("Viewmodel contents changed");
		};
	}
	
	[RelayCommand]
	private void ClickedOn() {
		callback?.Invoke(this, TabCallbackAction.ClickedOn);
	}
	

	[RelayCommand]
	private void Close() {
		callback?.Invoke(this, TabCallbackAction.Close);
	}
}