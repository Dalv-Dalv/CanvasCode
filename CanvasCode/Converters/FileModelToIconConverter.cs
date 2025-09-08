using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using CanvasCode.Models;

namespace CanvasCode.Converters;

public class FileModelToIconConverter : IMultiValueConverter {
	private const string DefaultIcon = "\uE230"; 
	private const string DirectoryIcon = "\uE25A"; 
	private const string InaccessibleDirectoryIcon = "\uEB5E"; 
	private const string HiddenDirectoryIcon = "\uE8F8";
	
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		if (values.Count < 3) return DefaultIcon;
		if (values[0] is not string name) return DefaultIcon;
		if (values[1] is not bool isDirectory) return DefaultIcon;
		if (values[2] is not bool isAccessible) return DefaultIcon;
		
		switch (isDirectory) {
			case true when !isAccessible:
				return InaccessibleDirectoryIcon;
			case true:
				return (name.Length > 0 && name[0] == '.') ? HiddenDirectoryIcon : DirectoryIcon;
			default: 
				var extension = Path.GetExtension(name);
				return FileConverterHelper.TryGetIconFromFileExtension(extension, out var icon) ? icon : DefaultIcon;
		}
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}