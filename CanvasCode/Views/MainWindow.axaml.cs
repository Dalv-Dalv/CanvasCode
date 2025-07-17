using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CanvasCode.ViewModels;
using CanvasCode.ViewModels.CanvasWindows;
using Microsoft.VisualBasic;

namespace CanvasCode.Views;

public partial class MainWindow : Window {
	public MainWindow() {
		InitializeComponent();
		
		AddHandler(DragDrop.DragOverEvent, DragOver);
		AddHandler(DragDrop.DropEvent, Drop);
	}

	protected override void OnResized(WindowResizedEventArgs e) {
		base.OnResized(e);
		ShaderHost.ShaderWidth = e.ClientSize.Width;
		ShaderHost.ShaderHeight = e.ClientSize.Height;
	}

	private bool isDraggingCanvas = false;
	private void MainCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (!e.Properties.IsLeftButtonPressed) return;
		
		isDraggingCanvas = true;
	}
	private void MainCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.InitialPressMouseButton != MouseButton.Left) return;
		
		isDraggingCanvas = false;
	}
	private void MainCanvas_OnPointerMoved(object? sender, PointerEventArgs e) {
		if (!isDraggingCanvas || DataContext is not MainWindowViewModel vm) return;
		
		ShaderHost.SetUniform("posx", (float)MainCanvas.OffsetX);
		ShaderHost.SetUniform("posy", (float)MainCanvas.OffsetY);
	}
	private void MainCanvas_OnZoomChanged(object sender, ZoomChangedEventArgs e) {
		if (DataContext is not MainWindowViewModel vm) return;
	}

	
	
	private bool isDraggingWindow = false;
	private Point initialPos, initialClickPos;
	private void DebugBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (!e.Properties.IsLeftButtonPressed || sender is not Border canvasWindow) return;
		e.Handled = true; // Stop propagation so it doesnt drag the canvas as well
		isDraggingWindow = true;
		
		
		initialPos = new Point(Canvas.GetLeft(canvasWindow), Canvas.GetTop(canvasWindow));
		initialClickPos = e.GetPosition(null);
	}
	private void DebugBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.InitialPressMouseButton != MouseButton.Left || sender is not Border canvasWindow) return;
		
		isDraggingWindow = false;
	}
	private void DebugBorder_OnPointerMoved(object? sender, PointerEventArgs e) {
		if(!isDraggingWindow || sender is not Border canvasWindow) return;

		Canvas.SetLeft(canvasWindow, initialPos.X + (e.GetPosition(null).X - initialClickPos.X) * (1.0d / MainCanvas.ZoomX));
		Canvas.SetTop(canvasWindow, initialPos.Y + (e.GetPosition(null).Y - initialClickPos.Y) * (1.0d / MainCanvas.ZoomY));
	}

	
	
	
	private async void SolutionExplorerFile_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (e.Source is not Control { DataContext: FileNodeViewModel node }) return;

		var dragData = new DataObject();
		dragData.Set("file", node);
		
		var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
		Console.WriteLine(result);
	}
	private void DragOver(object? sender, DragEventArgs e) {
		Console.WriteLine("Drag over");
	}
	private void Drop(object? sender, DragEventArgs e) {
		Console.WriteLine("Drop");

		var data = e.Data.Get("file");
		if (data is not FileNodeViewModel vm) return;
		if (DataContext is not MainWindowViewModel mainvm) return;

		var attr = File.GetAttributes(vm.Model.FullPath);
		var type = attr.HasFlag(FileAttributes.Directory) ? CanvasWindowType.FolderTree : CanvasWindowType.CodeEditor;
		
		mainvm.OpenNewWindow(e.GetPosition(WindowsItemsControl) - new Point(150, 150), type);
		
		Console.WriteLine($"Drop succesful! {vm.Model.FullPath}");
	}
}