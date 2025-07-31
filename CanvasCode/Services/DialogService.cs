using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CanvasCode.ViewModels;
using CanvasCode.ViewModels.Dialogs;
using CanvasCode.Views;

namespace CanvasCode.Services;

public class DialogService {
	public async Task ShowDialog<TDialog>(TDialog dialog) 
		where TDialog : DialogViewModel {
		if (MainWindow.Instance.DataContext is not MainWindowViewModel vm) return;

		vm.CurrentDialog = dialog;
		dialog.Show();

		var prevFocused = MainWindow.Instance.FocusManager?.GetFocusedElement();

		await dialog.WaitAsync();

		Dispatcher.UIThread.Post(() => {
			prevFocused?.Focus();
		}, DispatcherPriority.Input);
	}
}