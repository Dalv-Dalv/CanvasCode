using System.Collections.Generic;
using CanvasCode.Models.CommandPalettes;

namespace CanvasCode.ViewModels.CanvasWindows;

public interface ICanvasWindowContentViewModel {
	public string GetTitle();
	
	public void SetData(object data);

	public List<CommandPaletteItem> GetQuickActions();
}