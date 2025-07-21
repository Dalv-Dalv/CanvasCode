using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CanvasCode.Services;
using CanvasCode.ViewModels;
using CanvasCode.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode;

public class App : Application {
	
	//TODO: Convert this to DI
	public static FolderDataService FolderService { get; private set; } = null!;
	private static CacheEvictionService cacheEvictionService = null!;
	public static IMessenger Messenger { get; set; } = null!;
	
	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		Messenger = new WeakReferenceMessenger();
		FolderService = new FolderDataService(Messenger);
		cacheEvictionService = new CacheEvictionService(FolderService, TimeSpan.FromMinutes(20));

		cacheEvictionService.StartAsync(CancellationToken.None);
					
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
			// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
			DisableAvaloniaDataAnnotationValidation();
			desktop.MainWindow = new MainWindow {
				DataContext = new MainWindowViewModel()
			};
			
			desktop.ShutdownRequested += OnShutdown;
		}

		base.OnFrameworkInitializationCompleted();
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