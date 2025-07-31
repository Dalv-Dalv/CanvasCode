using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CanvasCode.ViewModels.Dialogs;
using DialogViewModel = CanvasCode.ViewModels.Dialogs.DialogViewModel;

namespace CanvasCode.Views.Dialogs;

public partial class InputNameDialogView : UserControl {
	public InputNameDialogView() {
		InitializeComponent();
	}
	
	private void Thumb_OnDrag(object? sender, VectorEventArgs e) {
		if (DataContext is not DialogViewModel vm) return;
		
		var delta = e.Vector;
		
		vm.Position += delta;
	}

	private void TextBox_OnKeyDown(object? sender, KeyEventArgs e) {
		if (DataContext is not InputNameDialogViewModel vm) return;

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

	private void NameTextBox_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
		NameTextBox.Focus();
	}
}