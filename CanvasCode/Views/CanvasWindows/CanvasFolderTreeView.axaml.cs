using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CanvasCode.ViewModels;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Views.CanvasWindows;

public partial class CanvasFolderTreeView : UserControl {
	private record DragEvent(PointerEventArgs e, IDataObject data, DragDropEffects effects);

	private DragEvent? currentDragEvent = null;

	public CanvasFolderTreeView() {
		InitializeComponent();
	}

	private void TreeView_OnKeyDown(object? sender, KeyEventArgs e) {
		if (sender is not TreeView treeView) return;
		if(e.Key != Key.F2) return;
		
		if(treeView.SelectedItem is not FileNodeViewModel vm) return;
		
		if (vm.StartRenameCommand.CanExecute(null)) {
			vm.StartRenameCommand.Execute(null);
		}
        
		e.Handled = true;
	}

	private void OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (sender is not Control { DataContext: FileNodeViewModel node } c) return;

		if (e.Properties.IsRightButtonPressed) {
			c.FindControl<Border>("PART_LayoutRoot")?.Focus();
		}
		
		if (!e.Properties.IsLeftButtonPressed) return;
		
		
		var dragData = new DataObject();
		dragData.Set("Files", new string[]{node.Model.FullPath});
		dragData.Set(DataFormats.FileNames, new[]{ node.Model.FullPath });

		currentDragEvent = new DragEvent(e, dragData, DragDropEffects.Move);
	}
	private void OnPointerMoved(object? sender, PointerEventArgs e) {
		if (currentDragEvent == null) return;
		
		var delta = currentDragEvent.e.GetPosition(null) - e.GetPosition(null);
		var sqrLen = delta.X * delta.X + delta.Y * delta.Y;
		if (sqrLen <= 5 * 5) return;

		DragDrop.DoDragDrop(currentDragEvent.e, currentDragEvent.data, currentDragEvent.effects);

		currentDragEvent = null;
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		currentDragEvent = null;
	}
}