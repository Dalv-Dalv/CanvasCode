using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CanvasCode.Models;

namespace CanvasCode.Converters;

public class FileModelToColorConverter : IMultiValueConverter {
	private static readonly IBrush DefaultIconColor = new SolidColorBrush(new Color(255, 221, 221, 221));
	private static readonly IBrush InaccessibleIconColor = new SolidColorBrush(new Color(255, 124, 130, 133));
	
	
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not FolderModel model) {
			return DefaultIconColor;
		}

		if (model.IsDirectory) return model.IsAccessible ? DefaultIconColor : InaccessibleIconColor;

		var extension = Path.GetExtension(model.Name);
		return FileConverterHelper.TryGetColorFromFileExtension(extension, out var icon) ? icon : DefaultIconColor;
	}
	
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		if (values.Count < 3) return DefaultIconColor;
		if (values[0] is not string name) return DefaultIconColor;
		if (values[1] is not bool isDirectory) return DefaultIconColor;
		if (values[2] is not bool isAccessible) return DefaultIconColor;
		
		if (isDirectory) return isAccessible ? DefaultIconColor : InaccessibleIconColor;

		var extension = Path.GetExtension(name);
		return FileConverterHelper.TryGetColorFromFileExtension(extension, out var icon) ? icon : DefaultIconColor;
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}