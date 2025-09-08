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
		
		CanvasShader.SetUniform("posx", (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("posy", (float)MainCanvas.OffsetY);
	}

	
	
	private bool shaderDragDropEffectStarted = false;
	public void StartShaderDragDropEffect(DragEventArgs e) {
		var pos = e.GetPosition(this);
		CanvasShader.SetUniform("hover_posx", (float)pos.X - (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("hover_posy", (float)pos.Y - (float)MainCanvas.OffsetY);
		
		shaderDragDropEffectStarted = true;
		
		CanvasShader.SetUniform("hover_start_time", App.GetCurrentTime());
		CanvasShader.SetUniform("hover_end_time", App.GetCurrentTime() + 10000f);
		CanvasShader.AnimationFrameRate = 144;
	}
	public void UpdateShaderDragDropEffect(DragEventArgs e) {
		var pos = e.GetPosition(this);
		CanvasShader.SetUniform("hover_posx", (float)pos.X - (float)MainCanvas.OffsetX);
		CanvasShader.SetUniform("hover_posy", (float)pos.Y - (float)MainCanvas.OffsetY);
		CanvasShader.SetUniform("hover_end_time", App.GetCurrentTime() + 10000f);
	}
	public void StopShaderDragDropEffect() {
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