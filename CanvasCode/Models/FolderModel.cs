using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.Models;

public partial class FolderModel(string name, string fullPath, bool isDirectory, bool isAccessible = true) : ObservableObject {
	[ObservableProperty] private string name = name;
	[ObservableProperty] private string fullPath = fullPath;
	[ObservableProperty] private bool isDirectory = isDirectory;
	[ObservableProperty] private bool isAccessible = isAccessible;
	[ObservableProperty] private bool childrenLoaded = false;
	
	public FolderModel(bool isDirectory, bool isAccessible = true) : this("Loading...", "", isDirectory, isAccessible) {}
	public FolderModel(string fullPath, bool isAccessible = true) : this(
		Path.GetPathRoot(fullPath) == Path.GetFullPath(fullPath) ? fullPath : Path.GetFileName(fullPath), 
		fullPath, 
		Directory.Exists(fullPath), 
		isAccessible) { }
	
	public ObservableCollection<FolderModel> Children { get; set; } = [];
}