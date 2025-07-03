using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace CanvasCode.Controls;

public enum ResizeBehaviour {
	All, Horizontal, Vertical, None
}

public partial class CanvasWindow : Border {
	private enum ResizeDirection { None, Left, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft }

	private bool isResizing;
	private ResizeDirection resizeDirection;
	private Point initialPos;
	private double initialTop, initialLeft, initialWidth, initialHeight;
	
	
	public ResizeBehaviour ResizeMode { get => GetValue(ResizeModeProperty); set => SetValue(ResizeModeProperty, value); }
	public static readonly StyledProperty<ResizeBehaviour> ResizeModeProperty = AvaloniaProperty.Register<CanvasWindow, ResizeBehaviour>(nameof(ResizeMode), defaultValue: ResizeBehaviour.All);

	public double HandlesSize { get => GetValue(HandlesSizeProperty); set => SetValue(HandlesSizeProperty, value); }
	public static readonly StyledProperty<double> HandlesSizeProperty = AvaloniaProperty.Register<CanvasWindow, double>(nameof(HandlesSize), defaultValue:4);


	
	protected override void OnPointerPressed(PointerPressedEventArgs e) {
		if (!e.Properties.IsLeftButtonPressed) return;
		
		Console.WriteLine($"{Bounds.Left}  {e.GetPosition(Parent as Visual)}");
		
		var pos = e.GetPosition(Parent as Visual);
		isResizing = true;
		resizeDirection = GetResizeDirection(pos);

		initialPos = pos;
		initialWidth = Width;
		initialHeight = Height;
		initialTop = Canvas.GetTop(this);
		initialLeft = Canvas.GetLeft(this);

		e.Pointer.Capture(this);
		e.Handled = true;
	}
	
	protected override void OnPointerMoved(PointerEventArgs e) {
		var pos = e.GetPosition(Parent as Visual);
		UpdateCursorVisual(pos);
		
		if (!isResizing) return;

		var delta = pos - initialPos;
		
		double newLeft = initialLeft, newTop = initialTop, newWidth = initialWidth, newHeight = initialHeight;
		
		switch (resizeDirection) {
			case ResizeDirection.Left or ResizeDirection.TopLeft or ResizeDirection.BottomLeft:
				newLeft = initialLeft + delta.X;
				newWidth = initialWidth - delta.X;
				
				var correction = newWidth - double.Max(MinWidth, double.Min(newWidth, MaxWidth));
				newWidth = double.Max(MinWidth, double.Min(newWidth, MaxWidth));
				newLeft += correction;
				break;
			
			case ResizeDirection.Right or ResizeDirection.TopRight or ResizeDirection.BottomRight:
				newWidth = double.Max(MinWidth, double.Min(initialWidth + delta.X, MaxWidth));;
				break;
			
			case ResizeDirection.None:
				newLeft = initialLeft + delta.X;
				break;
		}

		switch (resizeDirection) {
			case ResizeDirection.Top or ResizeDirection.TopLeft or ResizeDirection.TopRight:
				newTop = initialTop + delta.Y;
				newHeight = initialHeight - delta.Y;
				
				var correction = newHeight - double.Max(MinHeight, double.Min(newHeight, MaxHeight));
				newHeight = double.Max(MinHeight, double.Min(newHeight, MaxHeight));
				newTop += correction;
				break;
			
			case ResizeDirection.Bottom or ResizeDirection.BottomLeft or ResizeDirection.BottomRight:
				newHeight = double.Max(MinHeight, double.Min(initialHeight + delta.Y, MaxHeight));
				break;
			
			case ResizeDirection.None:
				newTop = initialTop + delta.Y;
				break;
		}
		
		Width = newWidth;
		Height = newHeight;
		Canvas.SetLeft(this, newLeft);
		Canvas.SetTop(this, newTop);
	}

	private void UpdateCursorVisual(Point pos) {
		bool left = pos.X - Bounds.Left <= HandlesSize;
		bool right = Bounds.Right - pos.X <= HandlesSize;
		bool top = pos.Y - Bounds.Top <= HandlesSize;
		bool bottom = Bounds.Bottom - pos.Y <=  HandlesSize;

		StandardCursorType cursor = StandardCursorType.Arrow;
		if ((left || right) && !(top || bottom)) cursor = StandardCursorType.SizeWestEast;
		else if ((top || bottom) && !(left || right)) cursor = StandardCursorType.SizeNorthSouth;
		else if ((top && left) || (bottom && right)) cursor = StandardCursorType.TopLeftCorner;
		else if((top && right) ||  (bottom && left)) cursor = StandardCursorType.BottomLeftCorner;

		Cursor = new Cursor(cursor);
	}

	private ResizeDirection GetResizeDirection(Point pos) {
		bool left = pos.X - Bounds.Left <= HandlesSize;
		bool right = Bounds.Right - pos.X <= HandlesSize;
		bool top = pos.Y - Bounds.Top <= HandlesSize;
		bool bottom = Bounds.Bottom - pos.Y <=  HandlesSize;

		if (left && top) return ResizeDirection.TopLeft;
		if (left && bottom) return  ResizeDirection.BottomLeft;
		if (right && top) return ResizeDirection.TopRight;
		if (right && bottom) return ResizeDirection.BottomRight;

		if (left) return ResizeDirection.Left;
		if (right) return ResizeDirection.Right;
		if (top) return ResizeDirection.Top;
		if (bottom) return ResizeDirection.Bottom;

		return ResizeDirection.None;
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e) {
		isResizing = false;
		resizeDirection = ResizeDirection.None;
		
		e.Pointer.Capture(null);
		e.Handled = true;
	}
}