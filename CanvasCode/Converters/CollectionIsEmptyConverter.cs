using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CanvasCode.Converters;

public class CollectionIsEmptyConverter : IValueConverter {
	public bool Invert { get; set; } = false;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not ICollection collection) return Invert ? true : false;
		
		var isEmpty = collection.Count == 0;
		return Invert ? !isEmpty : isEmpty;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}