﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CanvasCode.Converters;

public partial class EnumToValuesConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value is not Type enumType || !enumType.IsEnum) return null;

		return Enum.GetValues(enumType);
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}