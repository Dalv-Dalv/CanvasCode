using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Others;
using CanvasCode.ViewModels.CommandPalettes;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasWindowViewModel : ViewModelBase {
	[ObservableProperty] private Point position;
	[ObservableProperty] private Size size;
	[ObservableProperty] private int zIndex = 0;
	[ObservableProperty] private string title = null!;
	[ObservableProperty] private bool isPinned;
	private Point beforePinPosition;

	[ObservableProperty] private CanvasWindowType? selectedType;
	public IEnumerable<CanvasWindowType> AvailableTypes { get; } = Enum.GetValues<CanvasWindowType>();
	
	[ObservableProperty] private ICanvasWindowContentViewModel currentContent = null!;
	private Dictionary<CanvasWindowType, ICanvasContentState?> windowTypesStates = new() {
		{CanvasWindowType.CodeEditor, null},
		{CanvasWindowType.ShaderPreview, null},
		{CanvasWindowType.FolderTree, null}
	};

	[ObservableProperty] private bool isHeaderVisible = true;
	
	// Quick Actions
	[ObservableProperty] private bool isQuickActionsOpen = false;
	[ObservableProperty] private CommandPaletteViewModel? quickActions;


	public CanvasWindowViewModel() { // FOR DESIGN VIEW ONLY
		SelectedType = CanvasWindowType.FolderTree;
	}

	public CanvasWindowViewModel(CanvasWindowType type = CanvasWindowType.CodeEditor) {
		SelectedType = type;
	}

	public CanvasWindowViewModel(object data, CanvasWindowType type) {
		SelectedType = type; // TODO: DEBUG IF THIS DOES WHAT I THINK IT DOES
		CurrentContent.SetData(data);
	}

	public void SetData(object data) {
		CurrentContent.SetData(data);
	}

	partial void OnSelectedTypeChanged(CanvasWindowType? oldValue, CanvasWindowType? newValue) {
		if (oldValue == newValue) return;
		if (newValue == null) return;

		if(oldValue != null) windowTypesStates[oldValue.Value] = CurrentContent.GetState();

		CurrentContent = newValue switch {
			CanvasWindowType.CodeEditor => new CanvasCodeEditorViewModel(this),
			CanvasWindowType.FolderTree => new CanvasFolderTreeViewModel(this),
			CanvasWindowType.ShaderPreview => new CanvasShaderPreviewViewModel(this),
			_ => CurrentContent
		};

		if(windowTypesStates[newValue.Value] != null) CurrentContent.SetState(windowTypesStates[newValue.Value]!);
		
		Title = CurrentContent.GetTitle();
		
		QuickActions = null;
	}
	
	// Quick Actions
	public void ToggleQuickActions() {
		IsQuickActionsOpen = !IsQuickActionsOpen;
		if (IsQuickActionsOpen) BuildQuickActionMenu();
	}

	public void ToggleQuickActions(bool visibility) {
		IsQuickActionsOpen = visibility;
		if (IsQuickActionsOpen) BuildQuickActionMenu();
	}

	private void BuildQuickActionMenu() {
		if(QuickActions != null && QuickActions.CurrentMenu == QuickActions.RootMenu) return;

		var windowTypeActions = new List<CommandPaletteItem> {
			new("Code Editor", "1", new RelayCommand(() => SelectedType = CanvasWindowType.CodeEditor)),	
			new("Folder Tree", "2", new RelayCommand(() => SelectedType = CanvasWindowType.FolderTree)),	
			new("Shader Preview", "3", new RelayCommand(() => SelectedType = CanvasWindowType.ShaderPreview))	
		};
		var windowTypesMenu = new CommandPalette("Change Window Type", windowTypeActions);
		
		var generalActions = new List<CommandPaletteItem> {
			new("Toggle header", command: new RelayCommand(ToggleHeader)),
			new("Toggle fullscreen", command: new RelayCommand(ToggleFullscreen)),
			new("Change Window Type", subMenu: windowTypesMenu),
			new("Close window", command: new RelayCommand(CloseWindow))
		};

		var windowTypeSpecificActions = CurrentContent.GetQuickActions();

		var allActions = generalActions.Concat(windowTypeSpecificActions).ToList();

		for (int i = 0; i < allActions.Count && i < 10; i++) {
			allActions[i].Index = $"{i + 1}";
		}
		
		var rootMenu = new CommandPalette("Quick Actions", allActions);
		
		QuickActions = new  CommandPaletteViewModel(rootMenu);
	}

	
	
	// Actions
	public void ToggleHeader() => IsHeaderVisible = !IsHeaderVisible;

	public void ToggleFullscreen() {
		if(App.WindowingService.IsFullscreen) {
			App.WindowingService.ExitFullscreen();
			App.Messenger.Send(new ExitFullscreenMessage());
		} else {
			App.WindowingService.EnterFullscreen();
			App.Messenger.Send(new EnterFullscreenMessage(CurrentContent));
		}
	}

	public void CloseWindow() {
		MainWindow.Instance.CloseWindow(this);
	}

	[RelayCommand]
	private void CloseWindow(object? parameter) {
		CloseWindow();
	}


	[RelayCommand]
	private void PinWindow(object? parameter) {
		if (MainWindow.Instance.DataContext is not MainWindowViewModel vm) return;
		
		IsPinned = true;
		beforePinPosition = Position;
		Position = new Point(0, 0);
		
		vm.PinWindow(this);
	}

	[RelayCommand]
	private void UnpinWindow(object? parameter) {
		if (MainWindow.Instance.DataContext is not MainWindowViewModel vm) return;
		
		IsPinned = false;
		Position = beforePinPosition;
		
		vm.UnpinWindow(this);
	}
}