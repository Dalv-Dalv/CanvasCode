using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;

namespace CanvasCode.Controls;

public class FloatingWindow : Border {
	private bool isPressed = false;
	private Point lastPos;

	public FloatingWindow() {
		PointerPressed += HandlePressed;
		PointerReleased += HandleReleased;
		PointerMoved += HandleMoved;
	}

	private void HandlePressed(object? sender, PointerPressedEventArgs e) {
		if (!e.Properties.IsLeftButtonPressed) return;
		isPressed = true;
		lastPos = e.GetPosition(null);
	}
	private void HandleReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.InitialPressMouseButton != MouseButton.Left) return;
		isPressed = false;
	}
	private void HandleMoved(object? sender, PointerEventArgs e) {
		if (!isPressed) return;

		var pos = e.GetPosition(null);
		var dx = pos.X - lastPos.X;
		var dy = pos.Y - lastPos.Y;
	}
}