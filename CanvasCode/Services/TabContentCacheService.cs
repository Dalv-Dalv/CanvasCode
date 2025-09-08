using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanvasCode.Models.CanvasWindows;
using CanvasCode.ViewModels.CanvasWindows;

namespace CanvasCode.Services;

public class TabCacheEntry {
	public ICanvasWindowContentViewModel ViewModel { get; }
	public DateTime LastAccessed { get; set; }
	
	public TabCacheEntry(ICanvasWindowContentViewModel viewModel) {
		ViewModel = viewModel;
		LastAccessed = DateTime.UtcNow;
	}
	
	public TabCacheEntry(ICanvasWindowContentViewModel viewModel, DateTime lastAccessed) {
		ViewModel = viewModel;
		LastAccessed = lastAccessed;
	}
}

public class TabContentCacheService : IDisposable, IAsyncDisposable {
	private readonly Dictionary<CanvasWindowTabViewModel, TabCacheEntry> cache =  new();
	private readonly List<CanvasWindowTabViewModel> mostRecentlyUsedTabs = [];

	private readonly Timer evictionTimer;

	private readonly CanvasWindowViewModel parentWindow;
	
	public TabContentCacheService(CanvasWindowViewModel parentWindow) {
		evictionTimer = new Timer(PruneCache, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
		this.parentWindow = parentWindow;
	}

	public ICanvasWindowContentViewModel GetContent(CanvasWindowTabViewModel tabViewModel) {
		if (cache.TryGetValue(tabViewModel, out var cacheHit)) {
			cacheHit.LastAccessed = DateTime.UtcNow;
			MoveTabToMostRecent(tabViewModel);
			
			return cacheHit.ViewModel;
		}

		var newContent = CreateViewModelFromTab(tabViewModel);
		cache[tabViewModel] = new TabCacheEntry(newContent);
		MoveTabToMostRecent(tabViewModel);
		
		return newContent;
	}

	private ICanvasWindowContentViewModel CreateViewModelFromTab(CanvasWindowTabViewModel tabViewModel) {
		ICanvasWindowContentViewModel viewModel = tabViewModel.Type switch {
			CanvasWindowType.CodeEditor => new CanvasCodeEditorViewModel(parentWindow),
			CanvasWindowType.FolderTree => new CanvasFolderTreeViewModel(parentWindow),
			CanvasWindowType.ShaderPreview => new CanvasShaderPreviewViewModel(parentWindow),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		var state = tabViewModel.State;
		if (state != null) viewModel.SetState(state);
		
		tabViewModel.SubscribeToViewModelEvents(viewModel);

		return viewModel;
	}

	private void PruneCache(object? state) {
		const int keepAliveCount = 3;
		var expirationTime = TimeSpan.FromMinutes(10);
		var now = DateTime.UtcNow;

		var hotTabs = mostRecentlyUsedTabs.TakeLast(keepAliveCount).ToHashSet();

		var tabsToEvict = mostRecentlyUsedTabs
		                  .Where(tab => !hotTabs.Contains(tab) && (now - cache[tab].LastAccessed) >= expirationTime)
		                  .ToList();

		foreach (var tab in tabsToEvict) {
			tab.State = cache[tab].ViewModel.GetState();
			RemoveTab(tab);
		}
	}

	public void RemoveTab(CanvasWindowTabViewModel tabViewModel) {
		Console.WriteLine($"Tab removed from caching service: {tabViewModel.Type}");
		
		cache.Remove(tabViewModel);
		mostRecentlyUsedTabs.Remove(tabViewModel);
	}

	public void MoveTabToMostRecent(CanvasWindowTabViewModel tabViewModel) {
		mostRecentlyUsedTabs.Remove(tabViewModel);
		mostRecentlyUsedTabs.Add(tabViewModel);
	}
	
	

	public void Dispose() {
		evictionTimer.Dispose();
	}

	public async ValueTask DisposeAsync() {
		await evictionTimer.DisposeAsync();
	}
}