using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CanvasCode.ViewModels.Dialogs;
using DialogViewModel = CanvasCode.ViewModels.Dialogs.DialogViewModel;

namespace CanvasCode.Views.Dialogs;

public partial class ConfirmDialogView : UserControl {
	public ConfirmDialogView() {
		InitializeComponent();
	}
	
	private void Thumb_OnDrag(object? sender, VectorEventArgs e) {
		if (DataContext is not DialogViewModel vm) return;
		
		var delta = e.Vector;
		
		vm.Position += delta;
	}

	private void OnKeyDown(object? sender, KeyEventArgs e) {
		if (DataContext is not ConfirmDialogViewModel vm) return;
		
		switch (e.Key) {
			case Key.Enter:
				vm.ConfirmAsync();
				e.Handled = true;
				break;
			
			case Key.Escape:
				vm.Close();
				e.Handled = true;
				break;
		}
	}
}