using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CanvasCode.Models.CommandPalettes;
using CanvasCode.Others;
using CanvasCode.ViewModels.CanvasWindows;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.Views.CanvasWindows;

public partial class CanvasWindowView : UserControl, IRecipient<RequestFocusMessage> {
	private enum ThumbEvent {
		SizeTL, SizeT, SizeTR,
		SizeL , Drag , SizeR ,
		SizeBL, SizeB, SizeBR,
		None
	}

	ThumbEvent currentThumbEvent = ThumbEvent.None;
	
	
	// For reordering
	private static int biggestZIndex = 0;
	private static CanvasWindowViewModel? frontVM = null; 
	
	public CanvasWindowView() {
		InitializeComponent();
		
		App.Messenger.RegisterAll(this);
		
		Dispatcher.UIThread.Post(BringToFront);
	}

	private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		BringToFront();
	}
	private void OutermostWindow_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		e.Handled = true; // Stop propagation to the pan and zoom canvas
	}

	private void Thumb_OnDragStarted(object? sender, VectorEventArgs e) {
		if (DataContext is not CanvasWindowViewModel) return;
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
		
		BringToFront();
	}
	
	private void Thumb_OnDrag(object? sender, VectorEventArgs e) {
		if (currentThumbEvent == ThumbEvent.None) return;
		if (DataContext is not CanvasWindowViewModel vm) return;
		if (sender is not Thumb) return;

		var delta = e.Vector;
		var windowSize = MainWindow.Instance.ClientSize;
		const double margin = -10; // For pinned windows for now
		
	    var currentSize = vm.Size;
	    var currentPosition = vm.Position;
	    
	    var newWidth = currentSize.Width;
	    var newHeight = currentSize.Height;
	    var newLeft = currentPosition.X;
	    var newTop = currentPosition.Y;

	    
	    // Left/right resizing
	    switch (currentThumbEvent) {
		    case ThumbEvent.SizeL or ThumbEvent.SizeTL or ThumbEvent.SizeBL: // Left sided requires adjusting both width and position
			    if (vm.IsPinned) {
				    var deviation = newLeft + delta.X - Math.Max(newLeft + delta.X, margin);
				    newLeft += delta.X - deviation;
				    newWidth -= delta.X - deviation;
			    } else {
				    newLeft += delta.X;
				    newWidth -= delta.X;   
			    }
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
			    if (vm.IsPinned) {
				    var deviation = newTop + delta.Y - Math.Max(newTop + delta.Y, margin);
				    newTop += delta.Y - deviation;
				    newHeight -= delta.Y - deviation;
			    } else {
				    newTop += delta.Y;
				    newHeight -= delta.Y;   
			    }
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
	    
	    if (vm.IsPinned) {
		    newWidth = Math.Min(newWidth, windowSize.Width - margin * 2);
		    newHeight = Math.Min(newHeight, windowSize.Height - margin * 2);

		    var newRight = newLeft + newWidth;
		    var newBottom = newTop + newHeight;
			
		    if (newRight > windowSize.Width - margin) {
			    if (currentThumbEvent is ThumbEvent.Drag) newLeft = windowSize.Width - margin - newWidth;
			    else newWidth = windowSize.Width - margin - newLeft;
		    }

		    if (newBottom > windowSize.Height - margin) {
			    if (currentThumbEvent is ThumbEvent.Drag) newTop = windowSize.Height - margin - newHeight;
			    else newHeight = windowSize.Height - margin - newTop;
		    }

		    if (newLeft < margin) newLeft = margin;
		    if (newTop < margin) newTop = margin;
	    }
	    
	    vm.Size = new Size(newWidth, newHeight);
	    vm.Position = new Point(newLeft, newTop);
	}

	private void Thumb_OnDragCompleted(object? sender, VectorEventArgs e) {
		currentThumbEvent = ThumbEvent.None;
	}

	private void BringToFront() {
		if (DataContext is not CanvasWindowViewModel vm) return;

		if (vm.ZIndex > biggestZIndex) return;
		if (vm == frontVM) return;
		
		vm.ZIndex = ++biggestZIndex;
		frontVM = vm;
	}

	private void Window_OnKeyDown(object? sender, KeyEventArgs e) {
		if (DataContext is not CanvasWindowViewModel vm) return;
		
		if (e is { Key: Key.OemTilde, KeyModifiers: KeyModifiers.Control }) {
			vm.ToggleQuickActions(true);
			e.Handled = true;
			return;
		}

		if (!vm.IsQuickActionsOpen) return;

		if (e.Key == Key.Escape) {
			vm.ToggleQuickActions(false);
			e.Handled = true;
			return;
		}
		
		int nr = e.Key switch {
			>= Key.D1 and <= Key.D9 => e.Key - Key.D1,
			>= Key.NumPad1 and <= Key.NumPad9 => e.Key - Key.NumPad1,
			_ => -1
		};

		if (nr < 0) return;

		if (vm.QuickActions?.SelectCommand(nr) == true) {
			vm.ToggleQuickActions(false);	
		}
		e.Handled = true;
	}

	private void QuickActionButton_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is not CanvasWindowViewModel vm) return;

		if (sender is not Control c) return;
		if (c.DataContext is not CommandPaletteItem item) return;
		
		if(item.SubMenu == null) vm.ToggleQuickActions(false);
	}

	private void ComboBox_OnDropDownClosed(object? sender, EventArgs e) {
		ActualWindow.Focus();
	}

	private void ActualWindow_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
		ActualWindow.Focus();
	}

	public void Receive(RequestFocusMessage message) {
		if (message.TargetViewModel != this.DataContext) return;

		Dispatcher.UIThread.Post(() => {
			ActualWindow.Focus();
		});
	}
}