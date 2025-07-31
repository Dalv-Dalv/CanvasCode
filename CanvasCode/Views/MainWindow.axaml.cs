using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CanvasCode.Others;
using CanvasCode.ViewModels;
using CanvasCode.ViewModels.CanvasWindows;
using CanvasCode.Views.CanvasWindows;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.Views;

public partial class MainWindow : Window {
	private Timer? titleBarTimer;
	private bool isCursorOverTitleBar = false;
	
	public static MainWindow Instance { get; private set; }
	
	public MainWindow() {
		InitializeComponent();

		Instance = this;
		
		titleBarTimer = new Timer(ShowTitleBar, null, Timeout.Infinite, Timeout.Infinite);
		
		AddHandler(DragDrop.DragOverEvent, DragOver);
		AddHandler(DragDrop.DropEvent, Drop);
		AddHandler(DragDrop.DragEnterEvent, DragEnter);
		AddHandler(DragDrop.DragLeaveEvent, DragLeave);
		
		CanvasShader.SetUniform("posx", (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("posy", (float)MainCanvas.OffsetY);
		CanvasShader.SetUniform("hover_posx", 0);
		CanvasShader.SetUniform("hover_posy", 0);
		CanvasShader.SetUniform("hover_start_time", 0);
		CanvasShader.SetUniform("hover_end_time", 0);
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

	
	
	static Control? prevDragControl = null;
	
	private void DragEnter(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;

		if (c.DataContext is CanvasFolderTreeViewModel ftvm) {
			ftvm.IsDraggingOverRoot = true;
		}else if (c.DataContext is FileNodeViewModel fnvm) {
			if (fnvm.Model.IsDirectory) fnvm.IsDropTarget = true;
			else if (fnvm.Parent != null) fnvm.Parent.IsDropTarget = true;
		}
		
		if (c.DataContext is MainWindowViewModel) {
			var pos = e.GetPosition(this);
			CanvasShader.SetUniform("hover_posx", (float)pos.X - (float)MainCanvas.OffsetX);
			CanvasShader.SetUniform("hover_posy", (float)pos.Y - (float)MainCanvas.OffsetY);
			StartShaderDragDropEffect();
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
		CanvasShader.SetUniform("hover_end_time", App.GetCurrentTime() + 10000f);
	}
	private void DragLeave(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;

		if (prevDragControl.DataContext is CanvasFolderTreeViewModel ftvm) {
			ftvm.IsDraggingOverRoot = false;
		}else if (prevDragControl.DataContext is FileNodeViewModel fnvm) {
			if (fnvm.Model.IsDirectory) fnvm.IsDropTarget = false;
			else if (fnvm.Parent != null) fnvm.Parent.IsDropTarget = false;
		}
		
		if (c.DataContext is MainWindowViewModel) {
			StopShaderDragDropEffect();
		}
		
		c = c.FindAncestorOfType<Border>(includeSelf: true) ?? c;
		c.Classes.Remove("dragOver");
	}
	
	private void Drop(object? sender, DragEventArgs e) {
		prevDragControl?.Classes.Remove("dragOver");
		if (prevDragControl is { DataContext: MainWindowViewModel _ }) {
			StopShaderDragDropEffect();
		}

		if (prevDragControl?.DataContext is FileNodeViewModel fnvm) {
			if (fnvm.Model.IsDirectory) fnvm.IsDropTarget = false;
			else if(fnvm.Parent != null) fnvm.Parent.IsDropTarget = false;
		}

		string[]? files = e.Data.Get("Files") as string[];
		if (files == null) {
			var data = e.Data.GetFiles();
			if (data is null) return;

			files = data
			        .Select(item => item.TryGetLocalPath())
			        .Where(path => path is not null)
			        .ToArray()!;	
		}

		if (files.Length <= 0) return;
		
		if (e.Source is not Control c) return;
		
		switch (c.DataContext) {
			case FileNodeViewModel fvm:
				var parentWindowView = c.FindAncestorOfType<CanvasWindowView>(includeSelf: true);

				if (parentWindowView?.DataContext is CanvasWindowViewModel parentWindowVM) {
					App.Messenger.Send(new RequestFocusMessage(parentWindowVM));
				}
				
				if (fvm.Model.IsDirectory) {
					App.FolderService.Move(files[0], fvm.Model.FullPath);
				} else {
					var parentPath = Path.GetDirectoryName(fvm.Model.FullPath);
					if (parentPath == null) return;
					App.FolderService.Move(files[0], parentPath);
				}
				
				break;
			
			case CanvasFolderTreeViewModel ftvm:
				ftvm.IsDraggingOverRoot = false;
				
				if (ftvm.OpenFolderRoots.Count <= 0) return;

				App.Messenger.Send(new RequestFocusMessage(ftvm.ParentWindow));
				
				App.FolderService.Move(files[0], ftvm.OpenFolderRoots[0].Model.FullPath);
				break;
			
			case MainWindowViewModel mvm: {
				var attr = File.GetAttributes(files[0]);
				var type = attr.HasFlag(FileAttributes.Directory) ? CanvasWindowType.FolderTree : CanvasWindowType.CodeEditor;
			
				mvm.OpenNewWindow(e.GetPosition(WindowsItemsControl), files, type);
				break;
			}
		}
	}

	private bool shaderDragDropEffectStarted = false;
	private void StartShaderDragDropEffect() {
		shaderDragDropEffectStarted = true;
		
		CanvasShader.SetUniform("hover_start_time", App.GetCurrentTime());
		CanvasShader.SetUniform("hover_end_time", App.GetCurrentTime() + 10000f);
		CanvasShader.AnimationFrameRate = 144;
	}

	private void StopShaderDragDropEffect() {
		shaderDragDropEffectStarted = false;
		CanvasShader.SetUniform("hover_end_time", App.GetCurrentTime());
		Task.Delay(1000).ContinueWith(_ => {
			if (shaderDragDropEffectStarted) return;
			Dispatcher.UIThread.Post(() => {
				CanvasShader.AnimationFrameRate = 5;
			});
		});	
	}
	
	public void CloseWindow(CanvasWindowViewModel window) {
		if (DataContext is not MainWindowViewModel vm) return;
		
		vm.CloseWindow(window);
	}

	private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) {
		WindowState = WindowState.Minimized;
	}

	private void TogleMaximized_OnClick(object? sender, RoutedEventArgs e) {
		ToggleMaximize();
	}

	private void CloseWindow_OnClick(object? sender, RoutedEventArgs e) {
		Close();
	}
	
	private void ToggleMaximize() {
		WindowState = WindowState ==  WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
	}

	private void TitleBarDrag_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		this.BeginMoveDrag(e);
	}
	
	private void Window_OnPointerMoved(object? sender, PointerEventArgs e) {
		var pos = e.GetPosition(this);
		const double titlebarHoverHeight = 20.0;
		const double titlebarExitHeight = 32.0;

		if (pos.Y < titlebarHoverHeight && !isCursorOverTitleBar) {
			isCursorOverTitleBar = true;
			titleBarTimer?.Change(400, Timeout.Infinite);
		}else if (pos.Y > titlebarExitHeight && isCursorOverTitleBar) {
			isCursorOverTitleBar = false;
			titleBarTimer?.Change(Timeout.Infinite, Timeout.Infinite);
		
			if (DataContext is not MainWindowViewModel vm) return;

			vm.IsTitleBarVisible = false;			
		}
	}
	
	private void ShowTitleBar(object? state) {
		if (!isCursorOverTitleBar) return;
		
		Dispatcher.UIThread.Post(() => {
			if (DataContext is not  MainWindowViewModel vm) return;
			
			vm.IsTitleBarVisible = true;
		});
	}
}