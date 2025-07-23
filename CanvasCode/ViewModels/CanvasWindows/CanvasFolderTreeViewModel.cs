using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CanvasCode.Models;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasFolderTreeViewModel : ViewModelBase, ICanvasWindowContentViewModel {
	public ObservableCollection<FileNodeViewModel> OpenFolderRoots { get; } = [];
	[ObservableProperty] private bool isEmpty = true;

	
	
	public CanvasFolderTreeViewModel() {
		OpenFolderRoots.CollectionChanged += (_, _) => IsEmpty = OpenFolderRoots.Count == 0;
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
	
	

	public string GetTitle() => "Folder Tree";

	public List<CommandPaletteItem> GetQuickActions() => [
		new("Open Folder", command: OpenFolderCommand)
	];
	

	
	[RelayCommand]
	private async Task OpenFolder() {
		var folders = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
			Title = "Select folder", AllowMultiple = false
		});

		if (folders.Count <= 0) return;
		
		var path = folders[0].TryGetLocalPath();

		if (path == null) return;

		PopulateTree(path);
	}
	
	private void PopulateTree(string path) {
		if(!Directory.Exists(path)) return;

		//TODO: Handle multiple roots
		if (OpenFolderRoots.Count > 0) OpenFolderRoots.Clear();

		var vm = new FileNodeViewModel(new FolderModel(path), App.FolderService, App.Messenger);
		OpenFolderRoots.Add(vm);
		App.FolderService.StartWatching(path);
		
		OpenFolderRoots[0].IsExpanded = true;
	}
}