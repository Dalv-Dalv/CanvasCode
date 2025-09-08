using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CanvasCode.Others;
using CanvasCode.Services;
using CanvasCode.ViewModels;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode;

public class App : Application {
	
	//TODO: Convert this to DI
	public static DialogService DialogService = new();

	public static FocusService FocusService;
	
	public static FolderDataService FolderService { get; private set; } = null!;
	private static CacheEvictionService cacheEvictionService = null!;
	public static IMessenger Messenger { get; set; } = null!;

	private static DateTime startTime;
	
	public static IWindowingService WindowingService { get; private set; } = null!;
	
	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		Messenger = new WeakReferenceMessenger();
		FolderService = new FolderDataService(Messenger);
		cacheEvictionService = new CacheEvictionService(FolderService, TimeSpan.FromMinutes(20));

		startTime = DateTime.Now;
		
		cacheEvictionService.StartAsync(CancellationToken.None);
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
			// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
			DisableAvaloniaDataAnnotationValidation();
			desktop.MainWindow = new MainWindow {
				DataContext = new MainWindowViewModel()
			};

			// desktop.MainWindow.WindowState = WindowState.Maximized;

			FocusService = new FocusService(desktop.MainWindow);
			WindowingService = new WindowsWindowingService(desktop.MainWindow);
			
			desktop.ShutdownRequested += OnShutdown;
		}

		if(!Design.IsDesignMode)
			DragDropManager.Initialize();
		
		base.OnFrameworkInitializationCompleted();
	}

	public static float GetCurrentTime() {
		return (float)(DateTime.Now - startTime).TotalSeconds;
	}
	
	private void OnShutdown(object? sender, ShutdownRequestedEventArgs e) {
		cacheEvictionService.StopAsync(CancellationToken.None);
		cacheEvictionService.Dispose();
	}

	private void DisableAvaloniaDataAnnotationValidation() {
		// Get an array of plugins to remove
		var dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

		// remove each entry found
		foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
	}
}