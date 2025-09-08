using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace CanvasCode.Behaviours;

public class SuppressBringIntoViewBehaviour : Behavior<Control> {
	protected override void OnAttached() {
		base.OnAttached();
		AssociatedObject?.AddHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView, RoutingStrategies.Bubble);
	}

	protected override void OnDetaching() {
		base.OnDetaching();
		AssociatedObject?.RemoveHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView);
	}

	private void OnRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e) {
		e.Handled = true;
	}
}