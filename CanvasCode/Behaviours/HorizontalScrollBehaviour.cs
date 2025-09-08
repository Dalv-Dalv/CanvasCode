using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace CanvasCode.Behaviours;

public class HorizontalScrollBehavior : Behavior<ScrollViewer> {
	protected override void OnAttached() {
		base.OnAttached();
		
		AssociatedObject?.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, handledEventsToo: true);
	}

	protected override void OnDetaching() {
		base.OnDetaching();
		AssociatedObject?.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
	}

	private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (AssociatedObject is not ScrollViewer scrollViewer) return;
		if (e.Delta.Y == 0 || e.Delta.X != 0) return;
		if (scrollViewer?.Presenter is not { } presenter) return;

		e.Handled = true;
		e = new PointerWheelEventArgs(
			e.Source,
			e.Pointer,
			AssociatedObject,
			e.GetPosition(AssociatedObject),
			e.Timestamp,
			new PointerPointProperties((RawInputModifiers)e.KeyModifiers, PointerUpdateKind.Other),
			e.KeyModifiers,
			new Vector(-e.Delta.Y, e.Delta.X));
		presenter.RaiseEvent(e);

		e.Handled = true;
	}
}