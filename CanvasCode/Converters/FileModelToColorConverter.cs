using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CanvasCode.Models;

namespace CanvasCode.Converters;

public class FileModelToColorConverter : IValueConverter {
	private static readonly Dictionary<string, IBrush> FileToIconColor = new Dictionary<string, IBrush>() {
		{""      , new SolidColorBrush(new Color(255, 221, 221, 221))},
		{".cs"   , new SolidColorBrush(new Color(255, 95 , 173, 101))},
		{".txt"  , new SolidColorBrush(new Color(255, 221, 221, 221))},
		{".md"   , new SolidColorBrush(new Color(255, 84 , 138, 247))},
		{".cpp"  , new SolidColorBrush(new Color(255, 149, 90 , 224))},
		{".h"    , new SolidColorBrush(new Color(255, 149, 90 , 224))},
		{".css"  , new SolidColorBrush(new Color(255, 84 , 138, 247))},
		{".html" , new SolidColorBrush(new Color(255, 87 , 150, 92 ))},
		{".xml"  , new SolidColorBrush(new Color(255, 197, 124, 84 ))},
		{".xaml" , new SolidColorBrush(new Color(255, 50 , 99 , 194))},
		{".axaml", new SolidColorBrush(new Color(255, 50 , 99 , 194))}
	};
	
	private static readonly IBrush DefaultIconColor = new SolidColorBrush(new Color(255, 221, 221, 221));
	private static readonly IBrush InaccessibleIconColor = new SolidColorBrush(new Color(255, 124, 130, 133));
	
	
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not FolderModel model) {
			return DefaultIconColor;
		}

		if (model.IsDirectory) return model.IsAccessible ? DefaultIconColor : InaccessibleIconColor;

		var extension = Path.GetExtension(model.Name);
		return FileToIconColor.GetValueOrDefault(extension, DefaultIconColor);
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}