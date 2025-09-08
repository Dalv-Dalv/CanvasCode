using System.Collections.Generic;
using Avalonia.Media;

namespace CanvasCode.Converters;

public static class FileConverterHelper {
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
	
	private static readonly Dictionary<string, string> FileToIcon = new() {
		{""      , "\uE230"},   {".cpp"  , "\uEB2E"}, {".xml"  , "\uE914"},
		{".cs"   , "\uEB30"},   {".h"    , "\uEB2E"}, {".xaml" , "\uE914"},
		{".txt"  , "\uE23A"},   {".css"  , "\uEB34"}, {".axaml", "\uE914"},
		{".md"   , "\uED50"},   {".html" , "\uEB38"}
	};
	
	public static bool TryGetColorFromFileExtension(string extension, out IBrush? brush) {
		if (FileToIconColor.TryGetValue(extension, out var color)) {
			brush = color;
			return true;
		}

		brush = null;
		return false;
	}

	public static bool TryGetIconFromFileExtension(string extension, out string? icon) {
		if (FileToIcon.TryGetValue(extension, out var fileIcon)) {
			icon = fileIcon;
			return true;
		}

		icon = null;
		return false;
	}
}