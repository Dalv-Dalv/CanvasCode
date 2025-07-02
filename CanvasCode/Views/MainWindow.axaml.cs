using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CanvasCode.Views;

public partial class MainWindow : Window {
	public MainWindow() {
		InitializeComponent();
	}

	protected override void OnResized(WindowResizedEventArgs e) {
		base.OnResized(e);
		ShaderHost.ShaderWidth = e.ClientSize.Width;
		ShaderHost.ShaderHeight = e.ClientSize.Height;
	}
	
	// protected override void OnLoaded(RoutedEventArgs e) {
	// 	base.OnLoaded(e);
	// 	ShaderHost.Start();
	// }
	//
	// protected override void OnUnloaded(RoutedEventArgs e) {
	// 	base.OnUnloaded(e);
	// 	ShaderHost.Stop();
	// }
}