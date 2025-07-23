using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace CanvasCode.Behaviours;

public class FocusOnVisibleBehaviour : Behavior<Control> {
	private IDisposable? disposable;

	protected override void OnAttached() {
		base.OnAttached();
		
		if (AssociatedObject == null) return;
		disposable = AssociatedObject.GetObservable(Visual.IsVisibleProperty).Subscribe(OnIsVisibleChanged);
	}

	protected override void OnDetaching() {
		base.OnDetaching();
		disposable.Dispose();
	}

	private void OnIsVisibleChanged(bool isVisible) {
		if (AssociatedObject == null) return;
		if (!isVisible) return;

		Dispatcher.UIThread.Post(() => {
			if(AssociatedObject == null) return;
			AssociatedObject.Focus(NavigationMethod.Pointer);
			
			if(AssociatedObject is TextBox textBox) textBox.SelectAll();
		}, DispatcherPriority.Input);
	}
}