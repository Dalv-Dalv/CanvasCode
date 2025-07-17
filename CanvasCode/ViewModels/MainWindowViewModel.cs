using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CanvasCode.Models;
using CanvasCode.ViewModels.CanvasWindows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
	
	[ObservableProperty] private string? currentFolder = null;
	
	public ObservableCollection<FileNodeViewModel> OpenFolderRoots { get; } = [];
	public readonly Action<string> OnFolderChanged;

	public ObservableCollection<CanvasWindowViewModel> Windows { get; } = [];
	
	public MainWindowViewModel() {
		OnFolderChanged += PopulateTree;
		
		OpenNewWindow(new Point(500, 500));
	}

	public void OpenNewWindow(Point pos, CanvasWindowType type = CanvasWindowType.CodeEditor) {
		Windows.Add(new CanvasWindowViewModel {
			Position = pos,
			Size = new Size(300, 300),
			SelectedType = type
		});
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
		
		//TODO: Load multiple folders
		if (OpenFolderRoots.Count > 0) {
			if (OpenFolderRoots[0].ToString() == Path.GetDirectoryName(folderPath)) return;
			OpenFolderRoots.Clear();
		}
		
		//TODO: Use FileSystemWatcher to watch for changes in the tree and update the UI
		
		OpenFolderRoots.Add(new FileNodeViewModel(new FolderModel(folderPath)));
		OpenFolderRoots[0].IsExpanded = true;
	}
}