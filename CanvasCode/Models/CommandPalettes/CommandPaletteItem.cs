using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.Models.CommandPalettes;

public class CommandPaletteItem {
	public string Name { get; }
	public string Index { get; set; }
	public IRelayCommand Command { get; }
	public CommandPalette? SubMenu { get; }

	public CommandPaletteItem(string name, string index = "", IRelayCommand? command = null, CommandPalette? subMenu = null) {
		Name = name;
		Index = index;
		Command = command ?? new RelayCommand(() => { });
		SubMenu = subMenu;
	}
	
	public void Execute(object? parameter = null) {
		Command?.Execute(parameter);
	}
}