using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using CanvasCode.Models;

namespace CanvasCode.Converters;

public class FileModelToIconConverter : IValueConverter {
	private static readonly Dictionary<string, string> FileToIcon = new Dictionary<string, string>() {
		{""      , "\uE230"},   {".cpp"  , "\uEB2E"}, {".xml"  , "\uE914"},
		{".cs"   , "\uEB30"},   {".h"    , "\uEB2E"}, {".xaml" , "\uE914"},
		{".txt"  , "\uE23A"},   {".css"  , "\uEB34"}, {".axaml", "\uE914"},
		{".md"   , "\uED50"},   {".html" , "\uEB38"}
	};
	private const string DefaultIcon = "\uE230"; 
	private const string DirectoryIcon = "\uE25A"; 
	private const string InaccessibleDirectoryIcon = "\uEB5E"; 
	private const string HiddenDirectoryIcon = "\uE8F8";
	
	
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not FolderModel model) return "";

		switch (model.IsDirectory) {
			case true when !model.IsAccessible:
				return InaccessibleDirectoryIcon;
			case true:
				return (model.Name.Length > 0 && model.Name[0] == '.') ? HiddenDirectoryIcon : DirectoryIcon;
			default: 
				var extension = Path.GetExtension(model.Name);
				return FileToIcon.GetValueOrDefault(extension, DefaultIcon);
		}
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}