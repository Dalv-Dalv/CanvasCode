using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace CanvasCode.Behaviours;

public class FocusOnAttachedToVisualTreeBehaviour : Behavior<Control> {
	protected override void OnAttached() {
		base.OnAttached();
		
		if (AssociatedObject == null) return;
		AssociatedObject.AttachedToVisualTree += Focus;
	}

	private void Focus(object? sender, VisualTreeAttachmentEventArgs e) {
		if (AssociatedObject == null) return;
		
		var timer = new DispatcherTimer();
		timer.Tick += delegate {
			Dispatcher.UIThread.Post(() => {
				if (AssociatedObject == null) return;
				Console.WriteLine("                   BEHAVIOUR FIRED!");
				AssociatedObject.Focus(NavigationMethod.Pointer);
			}, DispatcherPriority.Input);
			timer.Stop();
		};

		timer.Interval = TimeSpan.FromMilliseconds(1000);
		timer.Start();
	}
}