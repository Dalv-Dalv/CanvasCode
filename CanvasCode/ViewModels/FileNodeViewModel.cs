using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels;

public partial class FileNodeViewModel :  ViewModelBase {
	[ObservableProperty] private string name;
	[ObservableProperty] private string filePath;
	[ObservableProperty] private bool isDirectory;
	[ObservableProperty] private string phosphorIcon;
	[ObservableProperty] private IBrush iconColor;
	
	[ObservableProperty] private bool isExpanded;
	[ObservableProperty] private bool isInteractable = true;


	private static readonly Dictionary<string, string> FileToIcon = new Dictionary<string, string>() {
		{""      , "\uE230"}, // Loading
		{".cs"   , "\uEB30"},
		{".txt"  , "\uE23A"},
		{".md"   , "\uED50"},
		{".cpp"  , "\uEB2E"},
		{".h"  , "\uEB2E"},
		{".css"  , "\uEB34"},
		{".html" , "\uEB38"},
		{".xml"  , "\uE914"},
		{".xaml" , "\uE914"},
		{".axaml", "\uE914"}
	};
	private const string DefaultIcon = "\uE230"; 
	private const string DirectoryIcon = "\uE25A"; 
	private const string InaccessibleDirectoryIcon = "\uEB5E"; 
	private const string HiddenDirectoryIcon = "\uE8F8";
	
	private static readonly Dictionary<string, IBrush> FileToIconColor = new Dictionary<string, IBrush>() {
		{""      , new SolidColorBrush(new Color(255, 255, 255, 255))},
		{".cs"   , new SolidColorBrush(new Color(255, 95 , 173, 101))},
		{".txt"  , new SolidColorBrush(new Color(255, 255, 255, 255))},
		{".md"   , new SolidColorBrush(new Color(255, 84 , 138, 247))},
		{".cpp"  , new SolidColorBrush(new Color(255, 149, 90 , 224))},
		{".h"    , new SolidColorBrush(new Color(255, 149, 90 , 224))},
		{".css"  , new SolidColorBrush(new Color(255, 84 , 138, 247))},
		{".html" , new SolidColorBrush(new Color(255, 87 , 150, 92 ))},
		{".xml"  , new SolidColorBrush(new Color(255, 197, 124, 84 ))},
		{".xaml" , new SolidColorBrush(new Color(255, 50 , 99 , 194))},
		{".axaml", new SolidColorBrush(new Color(255, 50 , 99 , 194))}
	};

	private readonly IBrush DefaultIconColor = new SolidColorBrush(new Color(255, 255, 255, 255));
	private readonly IBrush DefaultFolderIconColor = new SolidColorBrush(new Color(255, 255, 255, 255));
	
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

		try {
			var files = Directory.GetFiles(path).Length;
			if (files > 0) {
				Children.Add(new FileNodeViewModel(false));
				return;
			}
		} catch (Exception _) {
			IsInteractable = false;
			Children.Clear();

			PhosphorIcon = InaccessibleDirectoryIcon;
			
			return;
		}
		
		var directories = Directory.GetDirectories(path).Length;
		if (directories > 0) {
			Children.Add(new FileNodeViewModel(true));
		}
	}

	partial void OnIsExpandedChanged(bool value) {
		if (!value || childrenLoaded) return;

		string[] files;
		string[] directories;
		try {
			files = Directory.GetFiles(FilePath);
			directories = Directory.GetDirectories(FilePath);
		} catch (Exception _) {
			Children.Clear();
			return;
		}
		
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

	partial void OnPhosphorIconChanged(string value) {
		if (IsDirectory) {
			IconColor = DefaultFolderIconColor;
			return;
		}
		
		var extension = Path.GetExtension(filePath);
		IconColor = FileToIconColor.GetValueOrDefault(extension, DefaultIconColor);
	}

	public override string ToString() {
		return Name;
	}
}