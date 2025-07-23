using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CanvasCode.Models;
using CanvasCode.Others;
using CanvasCode.ViewModels.CanvasWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<EnterFullscreenMessage>, IRecipient<ExitFullscreenMessage> {
	
	[ObservableProperty] private string? currentFolder = null;
	[ObservableProperty] private bool isFullscreen;
    
	[ObservableProperty]
	private ICanvasWindowContentViewModel? fullscreenContent;
	
	public ObservableCollection<CanvasWindowViewModel> Windows { get; } = [];

	private Point lastRightClickPos;
	
	public MainWindowViewModel() {
		App.Messenger.RegisterAll(this);
	}


	[RelayCommand]
	private void OpenNewWindow(object? parameter) {
		OpenNewWindow(lastRightClickPos, CanvasWindowType.FolderTree);
	}
	public void SetLastRightClickPos(Point pos) => lastRightClickPos = pos;
	public void OpenNewWindow(Point pos, CanvasWindowType type = CanvasWindowType.CodeEditor) {
		var size = new Size(500, 500);
		Windows.Add(new CanvasWindowViewModel {
			Position = pos - new Point(size.Width / 2, size.Height / 2),
			Size = size,
			SelectedType = type
		});
	}
	public void OpenNewWindow(Point pos, object data, CanvasWindowType type = CanvasWindowType.CodeEditor) {
		var size = new Size(500, 500);
		var window = new CanvasWindowViewModel {
			Position = pos - new Point(size.Width / 2, size.Height / 2),
			Size = size,
			SelectedType = type
		};
		window.SetData(data);
		Windows.Add(window);
	}

	public void BringWindowToFront(CanvasWindowViewModel? window) {
		if (window == null) return;
        
		var index = Windows.IndexOf(window);

		if (index < 0 || index >= Windows.Count - 1) return;
		
		Windows.Move(index, Windows.Count - 1);
	}

	public void CloseWindow(CanvasWindowViewModel window) {
		Windows.Remove(window);
	}
	
	public void Receive(EnterFullscreenMessage message) {
		IsFullscreen = true;
		FullscreenContent = message.ContentToDisplay;
	}
	public void Receive(ExitFullscreenMessage message) {
		IsFullscreen = false;
		FullscreenContent = null;
	}
}