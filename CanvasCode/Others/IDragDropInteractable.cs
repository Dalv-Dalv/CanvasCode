using Avalonia.Input;

namespace CanvasCode.Others;

public interface IDragDropInteractable {
	public void OnDragDropEnter(DragEventArgs e);
	public void OnDragDropHover(DragEventArgs e);
	public void OnDragDropExit(DragEventArgs e);
	public void ReceiveDrop(DragEventArgs e);
}