using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Others;
using CanvasCode.Services;
using CanvasCode.ViewModels.CommandPalettes;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels.CanvasWindows;

public partial class CanvasWindowViewModel : ViewModelBase, IDisposable, IAsyncDisposable {
	[ObservableProperty] private Point position;
	[ObservableProperty] private Size size;
	[ObservableProperty] private int zIndex = 0;
	[ObservableProperty] private bool isPinned;
	private Point beforePinPosition;

	private readonly TabContentCacheService tabContentCacheService;

	public ObservableCollection<CanvasWindowTabViewModel> OpenTabs { get; } = [];
	
	[ObservableProperty] private ICanvasWindowContentViewModel currentContent = null!;

	[ObservableProperty] private bool isHeaderVisible = true;
	
	[ObservableProperty] private int? activeTabIndex = null;
	
	// Quick Actions
	[ObservableProperty] private bool isQuickActionsOpen = false;
	[ObservableProperty] private CommandPaletteViewModel? quickActions;


	public CanvasWindowViewModel() { // FOR DESIGN VIEW ONLY
		tabContentCacheService = new TabContentCacheService(this);
		OpenNewTab(CanvasWindowType.FolderTree);
		OpenNewTab(CanvasWindowType.CodeEditor);
		OpenNewTab(CanvasWindowType.ShaderPreview);
		
	}

	public CanvasWindowViewModel(CanvasWindowType type = CanvasWindowType.CodeEditor) {
		tabContentCacheService = new TabContentCacheService(this);

		OpenNewTab(type);
	}

	public CanvasWindowViewModel(CanvasWindowType type, object data) {
		tabContentCacheService = new TabContentCacheService(this);
		
		OpenNewTab(type, data: data);
	}

	public void SetData(object data) {
		CurrentContent?.SetData(data);
	}

	public void OpenNewTab(CanvasWindowType type = CanvasWindowType.FolderTree, ICanvasContentState? state = null, object? data = null) {
		var tab = new CanvasWindowTabViewModel(TabCallback, type, state);
		OpenTabs.Add(tab);
		ActiveTabIndex = OpenTabs.Count - 1; // TODO: Test that this updates CurrentContent before the next line
		
		if(data != null) CurrentContent.SetData(data);
	}

	private void TabCallback(CanvasWindowTabViewModel tabViewModel, TabCallbackAction action) {
		var index = OpenTabs.IndexOf(tabViewModel);
		if (index < 0) return;
		
		switch (action) {
			case TabCallbackAction.ClickedOn:
				ActiveTabIndex = index;
				
				tabContentCacheService.MoveTabToMostRecent(tabViewModel);
				break;
			
			case TabCallbackAction.Close:
				// TODO: Check for unsaved changes
				tabContentCacheService.RemoveTab(tabViewModel);
				
				if (OpenTabs.Count == 1) {
					CloseWindow();
					break;
				}
				
				OpenTabs.Remove(tabViewModel);
				if (ActiveTabIndex == index) {
					ActiveTabIndex = Math.Max(index - 1, 0); // QOL: Make it select the previously active tab 
				}else if (ActiveTabIndex > index) {
					ActiveTabIndex--; // BUG: This might cause problems, testing needed
				}
				
				break;
		}
		
		
	}
	
	partial void OnActiveTabIndexChanged(int? oldValue, int? newValue) {
		if (oldValue == newValue) return;
		if (newValue == null || newValue >= OpenTabs.Count) return;

		if (oldValue != null && oldValue < OpenTabs.Count) {
			OpenTabs[oldValue.Value].State = CurrentContent.GetState();
			OpenTabs[oldValue.Value].IsOpen = false;
		}

		CurrentContent = tabContentCacheService.GetContent(OpenTabs[newValue.Value]);
		
		var state = OpenTabs[newValue.Value].State;
		if (state != null) CurrentContent.SetState(state);

		OpenTabs[newValue.Value].IsOpen = true;
		
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
			new("Code Editor", "1", new RelayCommand(() => OpenNewTab(CanvasWindowType.CodeEditor))),	
			new("Folder Tree", "2", new RelayCommand(() => OpenNewTab(CanvasWindowType.FolderTree))),	
			new("Shader Preview", "3", new RelayCommand(() => OpenNewTab(CanvasWindowType.ShaderPreview)))	
		};
		var windowTypesMenu = new CommandPalette("Choose Type", windowTypeActions);
		
		var generalActions = new List<CommandPaletteItem> {
			new("Toggle header", command: new RelayCommand(ToggleHeader)),
			new("Toggle fullscreen", command: new RelayCommand(ToggleFullscreen)),
			new("Open New Tab", subMenu: windowTypesMenu),
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

	
	
	
	public void Dispose() {
		tabContentCacheService.Dispose();
	}

	public async ValueTask DisposeAsync() {
		await tabContentCacheService.DisposeAsync();
	}
}