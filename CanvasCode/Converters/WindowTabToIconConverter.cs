using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Converters;

public class WindowTabToIconConverter : IMultiValueConverter {
	private const string ShaderPreviewIcon = "\uE00A"; 
	private const string FolderTreeIcon = "\uE256"; 
	private const string DefaultCodeEditorIcon = "\uE1BC";
	
	private const string DefaultUnknownIcon = "\uE5F4"; 
	
	
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		var icon = DefaultUnknownIcon;
		
		if (values.Count < 2 || values[0] is not CanvasWindowType type) 
			return icon;

		if (values[1] == null) {
			return type switch {
				CanvasWindowType.CodeEditor => DefaultCodeEditorIcon,
				CanvasWindowType.FolderTree => FolderTreeIcon,
				CanvasWindowType.ShaderPreview => ShaderPreviewIcon
			};
		}

		if (values[1] is not ICanvasContentState state) return DefaultUnknownIcon;
		
		switch (type) {
			case CanvasWindowType.CodeEditor:
				icon = DefaultCodeEditorIcon;
				if (state is CanvasCodeEditorState codeEditorState && !string.IsNullOrEmpty(codeEditorState.FilePath)) {
					var extension = Path.GetExtension(codeEditorState.FilePath);
					
					if(FileConverterHelper.TryGetIconFromFileExtension(extension, out var newIcon))
						icon = newIcon;
				} 
				break;
			
			case CanvasWindowType.FolderTree:
				icon = FolderTreeIcon;
				break;
			
			case CanvasWindowType.ShaderPreview:
				icon = ShaderPreviewIcon;
				break;
			
			default:
				throw new ArgumentOutOfRangeException();
		}

		return icon;
	}
	
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}