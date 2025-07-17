using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CanvasCode.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels;

public partial class FileNodeViewModel :  ViewModelBase {
	[ObservableProperty] private FolderModel model;

	[ObservableProperty] private bool isExpanded = false;
	[ObservableProperty] private bool isInteractable = true;
	[ObservableProperty] private bool childrenLoaded = false;

	public ObservableCollection<FileNodeViewModel> Children { get; } = [];

	public FileNodeViewModel() {
		Model = new FolderModel("Loading...", "", false);
	}
	
	public FileNodeViewModel(FolderModel model) {
		Model = model;
		if (!Model.IsDirectory) return;

		try {
			if (Model.Children.Count == 0 && Directory.EnumerateFileSystemEntries(Model.FullPath).Any()) {
				Children.Add(new FileNodeViewModel());
			}
		} catch (Exception _) {
			IsInteractable = false;
			Model.Children.Clear();
			Model.IsAccessible = false;
		}
	}

	async partial void OnIsExpandedChanged(bool value) {
		if (!value || ChildrenLoaded) return;

		var childModels = await Task.Run(() => {
			var models = new List<FolderModel>();
			try {
				foreach (var dir in Directory.GetDirectories(Model.FullPath)) models.Add(new FolderModel(dir));
				foreach (var file in Directory.GetFiles(Model.FullPath)) models.Add(new FolderModel(file));
			} catch (Exception _) {
				IsInteractable = false;
				Model.Children.Clear();
				Model.IsAccessible = false;
			}

			return models;
		});
		
		Children.Clear(); // Remove 'loading' node
		foreach (var m in childModels) {
			Children.Add(new  FileNodeViewModel(m));
			Model.Children.Add(m);
		}

		ChildrenLoaded = true;
	}

	public override string ToString() {
		return Model.Name;
	}
}