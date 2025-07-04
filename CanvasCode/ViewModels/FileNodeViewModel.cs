using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels;

public partial class FileNodeViewModel :  ViewModelBase {
	[ObservableProperty] private string name;
	[ObservableProperty] private string filePath;
	[ObservableProperty] private bool isDirectory;
	[ObservableProperty] private string phosphorIcon;
	
	[ObservableProperty] private bool isExpanded;


	private static readonly Dictionary<string, string> FileToIcon = new Dictionary<string, string>() {
		{"", "\uE230"},
		{".cs", "\uEB30"},
		{".txt", "\uE23A"},
		{".md", "\uED50"},
		{".cpp", "\uEB2E"},
		{".css", "\uEB34"},
		{".html", "\uEB38"},
		{".xml", "\uE914"},
		{".xaml", "\uE914"},
		{".axaml", "\uE914"}
	};

	private const string DefaultIcon = "\uE230"; 
	private const string DirectoryIcon = "\uE25A"; 
	private const string HiddenDirectoryIcon = "\uE8F8"; 
	
	public ObservableCollection<FileNodeViewModel> Children { get; } = [];
	private bool childrenLoaded = false;

	private FileNodeViewModel(bool isDirectory) {
		IsDirectory = isDirectory;
		
		name = "Loading...";
		FilePath = "";
	}
	
	public FileNodeViewModel(string path) {
		if(!Directory.Exists(path) && !File.Exists(path)) throw new DirectoryNotFoundException($"Directory/file at {path} doesn't exist");
		
		IsDirectory = Directory.Exists(path);

		FilePath = path;
		Name = Path.GetFileName(path);

		if (!IsDirectory) return;
		
		var files = Directory.GetFiles(path).Length;
		if (files > 0) {
			Children.Add(new FileNodeViewModel(false));
			return;
		}
		
		var directories = Directory.GetDirectories(path).Length;
		if (directories > 0) {
			Children.Add(new FileNodeViewModel(true));
		}
	}

	partial void OnIsExpandedChanged(bool value) {
		if (!value || childrenLoaded) return;

		var files = Directory.GetFiles(FilePath);
		var directories = Directory.GetDirectories(FilePath);
		
		Children.Clear();
		
		foreach (var directory in directories) {
			Children.Add(new FileNodeViewModel(directory));
		}

		foreach (var file in files) {
			Children.Add(new FileNodeViewModel(file));
		}
		
		childrenLoaded = true;
	}

	partial void OnNameChanged(string value) {
		var extension = Path.GetExtension(value);

		if (IsDirectory) {
			PhosphorIcon = extension.Length > 0 ? HiddenDirectoryIcon : DirectoryIcon;
			return;
		}
		
		PhosphorIcon = FileToIcon.GetValueOrDefault(extension, DefaultIcon);
	}

	public override string ToString() {
		return Name;
	}
}