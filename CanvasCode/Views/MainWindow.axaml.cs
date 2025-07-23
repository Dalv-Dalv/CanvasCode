using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CanvasCode.ViewModels;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Views;

public partial class MainWindow : Window {
	public static MainWindow Instance { get; private set; }
	
	public MainWindow() {
		InitializeComponent();

		Instance = this;
		
		// var timer = new Timer(_ => {
		// 	Console.WriteLine($"Currently focused: {FocusManager?.GetFocusedElement()}");
		// }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
		
		AddHandler(DragDrop.DragOverEvent, DragOver);
		AddHandler(DragDrop.DropEvent, Drop);
		AddHandler(DragDrop.DragEnterEvent, DragEnter);
		AddHandler(DragDrop.DragLeaveEvent, DragLeave);
		
		CanvasShader.SetUniform("posx", (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("posy", (float)MainCanvas.OffsetY);
		CanvasShader.SetUniform("hover_posx", 0);
		CanvasShader.SetUniform("hover_posy", 0);
		CanvasShader.SetUniform("hover_mul", 0.0f);
	}

	protected override void OnResized(WindowResizedEventArgs e) {
		base.OnResized(e);
		CanvasShader.ShaderWidth = e.ClientSize.Width;
		CanvasShader.ShaderHeight = e.ClientSize.Height;
	}

	private bool isDraggingCanvas = false;
	private void MainCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (DataContext is not MainWindowViewModel vm) return;
		
		if (e.Properties.IsRightButtonPressed) {
			vm.SetLastRightClickPos(e.GetPosition(WindowsItemsControl));
		}
		
		if (!e.Properties.IsLeftButtonPressed) return;
		
		isDraggingCanvas = true;
	}
	private void MainCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.InitialPressMouseButton != MouseButton.Left) return;
		
		isDraggingCanvas = false;
	}
	private void MainCanvas_OnPointerMoved(object? sender, PointerEventArgs e) {
		if (!isDraggingCanvas || DataContext is not MainWindowViewModel vm) return;
		
		CanvasShader.SetUniform("posx", (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("posy", (float)MainCanvas.OffsetY);
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
	
	
	static Control? prevDragControl = null;
	
	private void DragEnter(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;

		if (c.DataContext is MainWindowViewModel) {
			var pos = e.GetPosition(this);
			CanvasShader.SetUniform("hover_posx", (float)pos.X - (float)MainCanvas.OffsetX);
			CanvasShader.SetUniform("hover_posy", (float)pos.Y - (float)MainCanvas.OffsetY);
			CanvasShader.SetUniform("hover_start_time", App.GetCurrentTime());
			CanvasShader.SetUniform("hover_mul", 1.0f);
			CanvasShader.AnimationFrameRate = 144;
		}
		
		c = c.FindAncestorOfType<Border>(includeSelf: true) ?? c;
		c.Classes.Add("dragOver");
		prevDragControl = c;
	}
	private void DragOver(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;

		// If dragging over shader
		if (c.DataContext is not MainWindowViewModel _) return; 
		
		var pos = e.GetPosition(this);
		CanvasShader.SetUniform("hover_posx", (float)pos.X - (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("hover_posy", (float)pos.Y - (float)MainCanvas.OffsetY);
	}
	private void DragLeave(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;

		if (c.DataContext is MainWindowViewModel) {
			CanvasShader.SetUniform("hover_mul", 0.0f);
			CanvasShader.AnimationFrameRate = 5;
		}
		
		c = c.FindAncestorOfType<Border>(includeSelf: true) ?? c;
		c.Classes.Remove("dragOver");
	}
	
	private void Drop(object? sender, DragEventArgs e) {
		prevDragControl?.Classes.Remove("dragOver");
		if (prevDragControl is { DataContext: MainWindowViewModel _ }) {
			CanvasShader.SetUniform("hover_mul", 0.0f);
		}
		
		var data = e.Data.Get(DataFormats.FileNames);
		if (data is not string[] files) return;
		
		if (e.Source is not Control c) return;
		
		switch (c.DataContext) {
			case FileNodeViewModel fvm:
				if (!fvm.Model.IsDirectory) break;
				
				App.FolderService.Move(files[0], fvm.Model.FullPath);
				break;
			
			case MainWindowViewModel mvm: {
				var attr = File.GetAttributes(files[0]);
				var type = attr.HasFlag(FileAttributes.Directory) ? CanvasWindowType.FolderTree : CanvasWindowType.CodeEditor;
			
				mvm.OpenNewWindow(e.GetPosition(WindowsItemsControl), files, type);
				break;
			}
		}
	}

	public void CloseWindow(CanvasWindowViewModel window) {
		if (DataContext is not MainWindowViewModel vm) return;
		vm.CloseWindow(window);
	}

	private void Canvas_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		Console.WriteLine("CANVAS POINTER PRESSED");
	}
}