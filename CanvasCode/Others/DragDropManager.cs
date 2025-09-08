using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CanvasCode.Views;

namespace CanvasCode.Others;

public static class DragDropManager {
	public const double SqrDragStartDistance = 5 * 5; 
	
	
	public static void Initialize() {
		MainWindow.Instance.AddHandler(DragDrop.DragOverEvent, DragOver);
		MainWindow.Instance.AddHandler(DragDrop.DropEvent, Drop);
		MainWindow.Instance.AddHandler(DragDrop.DragEnterEvent, DragEnter);
		MainWindow.Instance.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
	}


	static Control? prevControl = null;
	static IDragDropInteractable? prevInteractable = null;
	private static void DragEnter(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;
		if (c.DataContext is not IDragDropInteractable dragDropInteractable) return;
		Console.WriteLine($"DragDropManager: Drag entered {c.DataContext}");
		
		dragDropInteractable.OnDragDropEnter(e);

		c = c.FindAncestorOfType<Border>(includeSelf: true) ?? c;
		c.Classes.Add("dragOver");
		prevControl = c;
		prevInteractable = dragDropInteractable;
	}
	private static void DragOver(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;
		if (c.DataContext is not IDragDropInteractable dragDropInteractable) return;
		Console.WriteLine($"DragDropManager: Drag over {c.DataContext}");
		
		dragDropInteractable.OnDragDropHover(e);
	}
	private static void DragLeave(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;
		if (c.DataContext is not IDragDropInteractable dragDropInteractable) return;
		Console.WriteLine($"DragDropManager: Drag left {c.DataContext}");
		
		dragDropInteractable.OnDragDropExit(e);
		c = c.FindAncestorOfType<Border>(includeSelf: true) ?? c;
		c.Classes.Remove("dragOver");
	}


	
	private static void Drop(object? sender, DragEventArgs e) {
		if (e.Source is not Control c) return;
		if (c.DataContext is not IDragDropInteractable dragDropInteractable) return;
		Console.WriteLine($"DragDropManager: DROPPED over {c.DataContext}");

		// TODO: Debug why this would be needed
		// prevInteractable?.OnDragDropExit(e);
		prevControl?.Classes.Remove("dragOver");
		
		dragDropInteractable.ReceiveDrop(e);

		prevControl = null;
		prevInteractable = null;
	}

	public static bool TryGetFiles(DragEventArgs e, out string[]? filePaths) {
		filePaths = e.Data.Get(DataFormats.FileNames) as string[] 
		         ?? e.Data.Get("Files") as string[] 
		         ?? e.Data.GetFiles().Select(item => item.TryGetLocalPath()).Where(path => path is not null).ToArray();

		if (filePaths == null || filePaths.Length <= 0) return false;

		filePaths = filePaths.Where(path => !string.IsNullOrEmpty(path)).ToArray();
		
		return filePaths.Length > 0;
	}
}