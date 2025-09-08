using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;

namespace CanvasCode.Services;

public class FocusService {
	private IFocusManager focusManager;
	
	public FocusService(TopLevel topLevel) {
		topLevel.AddHandler(InputElement.GotFocusEvent, OnAnyGotFocus, RoutingStrategies.Bubble);
		topLevel.AddHandler(InputElement.LostFocusEvent, OnAnyLostFocus, RoutingStrategies.Bubble);

		focusManager = topLevel.FocusManager;
	}

	private void OnAnyLostFocus(object? sender, RoutedEventArgs e) {
		// Console.WriteLine($"Object lost focus: {sender} {e.Source} {focusManager.GetFocusedElement()}");
	}

	private void OnAnyGotFocus(object? sender, GotFocusEventArgs e) {
		// Console.WriteLine($"New object got focus: {sender} {e.Source} {focusManager.GetFocusedElement()}");
	}
}