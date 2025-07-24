using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CanvasCode.Models.CommandPalettes;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.CommandPalettes;

public partial class CommandPaletteViewModel : ViewModelBase {
	[ObservableProperty] private string currentTitle = "Quick Actions";
	[ObservableProperty] private CommandPalette rootMenu;
	[ObservableProperty] private CommandPalette currentMenu;

	public ObservableCollection<CommandPaletteItem> DisplayedCommands { get; } = [];

	public CommandPaletteViewModel(CommandPalette rootMenu) {
		this.rootMenu = rootMenu;
		currentMenu = rootMenu;
		UpdateDisplayedCommands();
	}

	private void UpdateDisplayedCommands() {
		DisplayedCommands.Clear();
		foreach (var cmd in CurrentMenu.Commands) {
			DisplayedCommands.Add(cmd);
		}

		CurrentTitle = CurrentMenu.Title;
	}


	[RelayCommand]
	private void SelectCommandByButton(CommandPaletteItem? command) {
		if (command == null) return;
		SelectCommand(command);
	}
	
	public bool SelectCommand(CommandPaletteItem command) {
		if (command.SubMenu != null) {
			CurrentMenu = command.SubMenu;
			UpdateDisplayedCommands();
			return false;
		}

		command.Execute();
		return true;
	}

	public bool SelectCommand(int index) {
		if (index >= DisplayedCommands.Count) return true;
		
		return SelectCommand(DisplayedCommands[index]);
	}
}