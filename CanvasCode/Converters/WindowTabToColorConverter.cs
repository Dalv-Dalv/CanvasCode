using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Converters;

public class WindowTabToColorConverter : IMultiValueConverter {
	private static readonly IBrush DefaultColor = new SolidColorBrush(new Color(255, 223, 225, 229));
	
	
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		var color = DefaultColor;
		
		if (values.Count < 2 || values[0] is not CanvasWindowType type || values[1] is not ICanvasContentState state) 
			return color;

		
		switch (type) {
			case CanvasWindowType.CodeEditor:
				if (state is CanvasCodeEditorState codeEditorState) {
					var extension = Path.GetExtension(codeEditorState.FilePath);
					
					if(FileConverterHelper.TryGetColorFromFileExtension(extension, out var newColor))
						color = newColor;
				} 
				break;
			
			case CanvasWindowType.FolderTree:
				break;
			
			case CanvasWindowType.ShaderPreview:
				break;
			
			default:
				throw new ArgumentOutOfRangeException();
		}

		return color;
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}