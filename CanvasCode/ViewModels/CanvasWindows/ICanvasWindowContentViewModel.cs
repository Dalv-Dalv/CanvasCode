using System;
using System.Collections.Generic;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.Models.CommandPalettes;

namespace CanvasCode.ViewModels.CanvasWindows;

public interface ICanvasWindowContentViewModel {
	public CanvasWindowViewModel ParentWindow { get; }
	
	public string GetTitle();
	
	public void SetData(object data);

	public ICanvasContentState? GetState();
	public void SetState(ICanvasContentState state);

	public List<CommandPaletteItem> GetQuickActions();
	
	public event Action<ICanvasContentState> OnStateChanged;
}