using System.Collections.Generic;

namespace CanvasCode.Models.CommandPalettes;

public class CommandPalette(string title, List<CommandPaletteItem> commands) {
	public string Title { get; } = title;
	public List<CommandPaletteItem> Commands { get; } = commands;
}