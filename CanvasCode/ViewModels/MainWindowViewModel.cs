using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
	
	[ObservableProperty] private string? currentFolder = null;
	
	public ObservableCollection<FileNodeViewModel> OpenFolderRoot { get; } = [];

	public readonly Action<string> OnFolderChanged;
	
	public MainWindowViewModel() {
		OnFolderChanged += PopulateTree;
	}
	
	public async void SelectFolderCommand(Window window) {
		var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
			Title = "Select folder", AllowMultiple = false
		});

		if (folders.Count <= 0) return;
		
		var path = folders[0].TryGetLocalPath();
		if (path == null || path == CurrentFolder) return;
		
		CurrentFolder = path;
		OnFolderChanged.Invoke(path);
	}

	private void PopulateTree(string folderPath) {
		if (!Directory.Exists(folderPath)) return;
		if (OpenFolderRoot.Count > 0) {
			if (OpenFolderRoot[0].ToString() == Path.GetDirectoryName(folderPath)) return;
			OpenFolderRoot.Clear();
		}
		
		OpenFolderRoot.Add(new FileNodeViewModel(folderPath));
		OpenFolderRoot[0].IsExpanded = true;
	}
}