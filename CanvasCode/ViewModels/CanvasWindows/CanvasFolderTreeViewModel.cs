using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CanvasCode.Models;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Others;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasFolderTreeViewModel : ViewModelBase, IDisposable, ICanvasWindowContentViewModel, IRecipient<FolderContentsChangedMessage>, 
												 IDragDropInteractable {
	public CanvasWindowViewModel ParentWindow { get; }
	public ObservableCollection<FileNodeViewModel> OpenFolderRoots { get; } = [];
	[ObservableProperty] private bool isDraggingOverRoot = false;
	[ObservableProperty] private bool isEmpty = true;
	[ObservableProperty] private PathStatus prevPath;
	public sealed record PathStatus(string Path, bool IsInvalidated);


	private Timer validationTimer;

	public event Action<ICanvasContentState>? OnStateChanged;
	
	
	public CanvasFolderTreeViewModel(CanvasWindowViewModel parentWindow) {
		ParentWindow = parentWindow;
		
		OpenFolderRoots.CollectionChanged += (_, _) => IsEmpty = OpenFolderRoots.Count == 0;
		App.Messenger.RegisterAll(this);

		validationTimer = new Timer(_ => {
			RevalidateRoots();
		}, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

		OpenFolderRoots.CollectionChanged += (_, _) => {
			OnStateChanged?.Invoke(GetState());
		};
	}
	public void SetData(object data) {
		switch (data) {
			case string file:
				PopulateTree(file);
				break;
			case string[] files:
				PopulateTree(files[0]); //TODO: Handle multiple roots
				break;
		}
	}

	public ICanvasContentState? GetState() {
		return new CanvasFolderTreeState { RootPath = OpenFolderRoots.Count <= 0 ? null : OpenFolderRoots[0].Model.FullPath };
	}
	public void SetState(ICanvasContentState state) {
		if (state is not CanvasFolderTreeState folderTreeState) return;
		if (folderTreeState.RootPath == null) return;
		
		PopulateTree(folderTreeState.RootPath);
	}


	public string GetTitle() => "Folder Tree";

	public List<CommandPaletteItem> GetQuickActions() => [
		new("Open Folder", command: OpenFolderCommand)
	];


	[RelayCommand]
	private async Task OpenFolder() {
		var folders = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
			Title = "Select folder", AllowMultiple = false
		});

		App.Messenger.Send(new RequestFocusMessage(ParentWindow));
		
		if (folders.Count <= 0) return;
		
		var path = folders[0].TryGetLocalPath();

		if (path == null) return;

		PopulateTree(path);
	}
	
	private void PopulateTree(string path) {
		if(!Directory.Exists(path)) return;

		//TODO: Handle multiple roots
		if (OpenFolderRoots.Count > 0) {
			PrevPath = new PathStatus(OpenFolderRoots[0].Model.FullPath, false);
			
			foreach (var root in OpenFolderRoots) {
				root.Dispose();
			}
			OpenFolderRoots.Clear();
		}

		var vm = new FileNodeViewModel(null, new FolderModel(path), App.FolderService, App.Messenger);
		OpenFolderRoots.Add(vm);
		App.FolderService.StartWatching(path);
		
		OpenFolderRoots[0].IsExpanded = true;
	}

	private void RevalidateRoots() {
		if (OpenFolderRoots.Count <= 0) return;

		if (Path.Exists(OpenFolderRoots[0].Model.FullPath)) return;

		PrevPath = new PathStatus(OpenFolderRoots[0].Model.FullPath, IsInvalidated: true);
		
		OpenFolderRoots[0].Dispose();
		OpenFolderRoots.Clear();
	}

	public void Receive(FolderContentsChangedMessage message) {
		if (OpenFolderRoots.Count <= 0) return;

		var path = OpenFolderRoots[0].Model.FullPath;
		if (path == message.path || !path.StartsWith(message.path)) return;
		// A parent folder of the open root has changed
		
		
		if (Path.Exists(path)) return; // If our root remains intact we're good

		PrevPath = new PathStatus(OpenFolderRoots[0].Model.FullPath, true);
		
		foreach (var root in OpenFolderRoots) {
			root.Dispose();
		}
		OpenFolderRoots.Clear();
	}

	public void Dispose() {
		App.Messenger.UnregisterAll(this);
		validationTimer.Dispose();
	}

	public void OnDragDropEnter(DragEventArgs e) {
		IsDraggingOverRoot = true;
	}

	public void OnDragDropHover(DragEventArgs e) {}
	public void OnDragDropExit(DragEventArgs e) {
		IsDraggingOverRoot = false;
	}

	public void ReceiveDrop(DragEventArgs e) {
		IsDraggingOverRoot = false;
		if (OpenFolderRoots.Count <= 0) return;

		if (!DragDropManager.TryGetFiles(e, out var filePaths)) return;
		
		App.Messenger.Send(new RequestFocusMessage(ParentWindow));
		App.FolderService.Move(filePaths[0], OpenFolderRoots[0].Model.FullPath);
	}
}