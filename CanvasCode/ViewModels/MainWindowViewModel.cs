using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CanvasCode.Models;
using CanvasCode.Others;
using CanvasCode.ViewModels.CanvasWindows;
using CanvasCode.ViewModels.Dialogs;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<EnterFullscreenMessage>, IRecipient<ExitFullscreenMessage>,
										   IDragDropInteractable {
	
	[ObservableProperty] private string? currentFolder = null;
	[ObservableProperty] private bool isFullscreen;
	[ObservableProperty] private bool isTitleBarVisible = false;
	
	[ObservableProperty] private ICanvasWindowContentViewModel? fullscreenContent;

	[ObservableProperty] private DialogViewModel? currentDialog;
	
	public ObservableCollection<CanvasWindowViewModel> Windows { get; } = [];
	public ObservableCollection<CanvasWindowViewModel> PinnedWindows { get; } = [];

	private Point lastRightClickPos;
	
	public MainWindowViewModel() {
		App.Messenger.RegisterAll(this);
		
		// var timer = new Timer((_) => {
		// 	Console.WriteLine($"FOCUS: {MainWindow.Instance.FocusManager.GetFocusedElement()}");
		// }, null, 0, 1000);
	}


	[RelayCommand]
	private void OpenNewWindow(object? parameter) {
		OpenNewWindow(lastRightClickPos, CanvasWindowType.FolderTree);
	}
	public void SetLastRightClickPos(Point pos) => lastRightClickPos = pos;
	public CanvasWindowViewModel OpenNewWindow(Point pos, CanvasWindowType type = CanvasWindowType.CodeEditor, bool centeredAtPos = true) {
		var size = new Size(500, 500);

		var newPos = centeredAtPos ? pos - new Point(size.Width / 2, size.Height / 2) : pos; 
		
		var window = new CanvasWindowViewModel(type) {
			Position = newPos,
			Size = size
		}; 
		
		Windows.Add(window);
		return window;
	}
	public CanvasWindowViewModel OpenNewWindow(Point pos, object data, CanvasWindowType type = CanvasWindowType.CodeEditor, bool centeredAtPos = true) {
		var window = OpenNewWindow(pos, type, centeredAtPos);
		window.SetData(data);

		return window;
	}

	public void PinWindow(CanvasWindowViewModel window) {
		Windows.Remove(window);
		PinnedWindows.Add(window);
	}

	public void UnpinWindow(CanvasWindowViewModel window) {
		PinnedWindows.Remove(window);
		Windows.Add(window);
	}

	public void CloseWindow(CanvasWindowViewModel window) {
		if (window.IsPinned) PinnedWindows.Remove(window);
		else Windows.Remove(window);
	}
	
	public void Receive(EnterFullscreenMessage message) {
		IsFullscreen = true;
		FullscreenContent = message.ContentToDisplay;
	}
	public void Receive(ExitFullscreenMessage message) {
		IsFullscreen = false;
		FullscreenContent = null;
	}

	public void OnDragDropEnter(DragEventArgs e) {
		MainWindow.Instance.StartShaderDragDropEffect(e);	
	}
	public void OnDragDropHover(DragEventArgs e) {
		MainWindow.Instance.UpdateShaderDragDropEffect(e);
	}
	public void OnDragDropExit(DragEventArgs e) {
		MainWindow.Instance.StopShaderDragDropEffect();
	}
	public void ReceiveDrop(DragEventArgs e) {
		MainWindow.Instance.StopShaderDragDropEffect();
		
		if (!DragDropManager.TryGetFiles(e, out var filePaths)) return;

		var attr = File.GetAttributes(filePaths[0]);
		var type = attr.HasFlag(FileAttributes.Directory) ? CanvasWindowType.FolderTree : CanvasWindowType.CodeEditor;

		OpenNewWindow(e.GetPosition(MainWindow.Instance.WindowsItemsControl), filePaths, type: type);
	}
}