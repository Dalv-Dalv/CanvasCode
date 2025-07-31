using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Converters;

public class PathStatusToErrorMessageConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not CanvasFolderTreeViewModel.PathStatus pathStatus) return "Press CTRL+~ or use the button to select a folder";

		if (!pathStatus.IsInvalidated) return "Press CTRL+Q or use the button to select a folder";
		
		return $"Previous path {pathStatus.Path} has been either moved or deleted. Please open the folder again";
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}