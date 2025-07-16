using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Views.CanvasWindows;

public partial class CanvasWindowView : UserControl {
	private enum ThumbEvent {
		SizeTL, SizeT, SizeTR,
		SizeL , Drag , SizeR ,
		SizeBL, SizeB, SizeBR,
		None
	}

	ThumbEvent currentThumbEvent = ThumbEvent.None;
	private Point initialPos;
	private Size initialSize;
	
	public CanvasWindowView() {
		InitializeComponent();
	}

	private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		e.Handled = true; // Stop propagation to the pan and zoom canvas
	}

	private void Thumb_OnDragStarted(object? sender, VectorEventArgs e) {
		if (DataContext is not CanvasWindowViewModel vm) return;
		if (sender is not Thumb thumb) return;

		currentThumbEvent = thumb.Name switch {
			"TL" => ThumbEvent.SizeTL,
			"TR" => ThumbEvent.SizeTR,
			"BR" => ThumbEvent.SizeBR,
			"BL" => ThumbEvent.SizeBL,
			"L"  => ThumbEvent.SizeL,
			"T"  => ThumbEvent.SizeT,
			"R"  => ThumbEvent.SizeR,
			"B"  => ThumbEvent.SizeB,
			"D"  => ThumbEvent.Drag,
			_    => ThumbEvent.None
		};

		if (currentThumbEvent == ThumbEvent.None) return;

		initialPos = vm.Position;
		initialSize = vm.Size;
	}
	
	private void Thumb_OnDrag(object? sender, VectorEventArgs e) {
		if (currentThumbEvent == ThumbEvent.None) return;
		if (DataContext is not CanvasWindowViewModel vm) return;
		if (sender is not Thumb thumb) return;

		var delta = e.Vector;
		
	    var currentSize = vm.Size;
	    var currentPosition = vm.Position;
	    
	    var newWidth = currentSize.Width;
	    var newHeight = currentSize.Height;
	    var newLeft = currentPosition.X;
	    var newTop = currentPosition.Y;

	    
	    // Left/right
	    switch (currentThumbEvent) {
		    case ThumbEvent.SizeL or ThumbEvent.SizeTL or ThumbEvent.SizeBL: // Left sided requires adjusting both width and position
			    newLeft += delta.X;
			    newWidth -= delta.X;
			    break;
			
		    case ThumbEvent.SizeR or ThumbEvent.SizeTR or ThumbEvent.SizeBR:
			    newWidth += delta.X;
			    break;
		    
		    case ThumbEvent.Drag:
			    newLeft += delta.X;
			    break;
	    }
		
	    // Up/down resizing
	    switch (currentThumbEvent) {
		    case ThumbEvent.SizeT or ThumbEvent.SizeTL or ThumbEvent.SizeTR: // Top sided requires adjusting both height and position
			    newTop += delta.Y;
			    newHeight -= delta.Y;
			    break;
			
		    case ThumbEvent.SizeB or ThumbEvent.SizeBL or ThumbEvent.SizeBR:
			    newHeight += delta.Y;
			    break;
		    
		    case ThumbEvent.Drag:
			    newTop += delta.Y;
			    break;
	    }

	    newWidth = Math.Clamp(newWidth, MinWidth, MaxWidth);
	    newHeight = Math.Clamp(newHeight, MinHeight, MaxHeight);
	    
	    if (currentThumbEvent is ThumbEvent.SizeL or ThumbEvent.SizeTL or ThumbEvent.SizeBL) {
	        var actualWidthChange = currentSize.Width - newWidth;
	        newLeft = currentPosition.X + actualWidthChange;
	    }
	    if (currentThumbEvent is ThumbEvent.SizeT or ThumbEvent.SizeTL or ThumbEvent.SizeTR) {
	        var actualHeightChange = currentSize.Height - newHeight;
	        newTop = currentPosition.Y + actualHeightChange;
	    }
	    
	    vm.Size = new Size(newWidth, newHeight);
	    vm.Position = new Point(newLeft, newTop);
	}

	private void Thumb_OnDragCompleted(object? sender, VectorEventArgs e) {
		currentThumbEvent = ThumbEvent.None;
	}
}