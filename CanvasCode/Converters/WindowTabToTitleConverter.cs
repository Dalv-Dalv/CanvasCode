using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Converters;

public class WindowTabToTitleConverter : IMultiValueConverter {
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		var title = "Unknown";
		
		if (values.Count < 2 || values[0] is not CanvasWindowType type) 
			return title;

		if (values[1] is not ICanvasContentState state) {
			return type == CanvasWindowType.ShaderPreview ? "Shader Preview" : title;
		}

		title = type switch {
			CanvasWindowType.CodeEditor => state is not CanvasCodeEditorState codeEditorState ? "Untitled" : Path.GetFileName(codeEditorState.FilePath),
			CanvasWindowType.FolderTree => state is not CanvasFolderTreeState folderTreeState ? "Unknown" : Path.GetFileName(folderTreeState.RootPath),
			CanvasWindowType.ShaderPreview => "Shader Preview",
			_ => throw new ArgumentOutOfRangeException()
		};

		return string.IsNullOrEmpty(title) ? "Unknown" : title;
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}