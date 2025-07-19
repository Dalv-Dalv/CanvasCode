using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CanvasCode.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.Services;

public class FolderDataService(IMessenger messenger) : ICacheManager {
	private readonly ConcurrentDictionary<string, FolderModel> folderCache = new();
	private readonly ConcurrentDictionary<string, DateTime> lastAccessTimes = new();
	
	private readonly ConcurrentDictionary<string, FileSystemWatcher> activeWatchers = new();
	
	private readonly IMessenger messenger = messenger;

	public FolderModel GetModel(string fullPath) {
		lastAccessTimes[fullPath] = DateTime.Now;

		if (folderCache.TryGetValue(fullPath, out var cachedModel)) {
			return cachedModel;
		}

		var newModel = new FolderModel(fullPath);
		folderCache.TryAdd(fullPath, newModel);
		
		return newModel;
	}

	public void EnsureChildrenAreLoaded(FolderModel model) {
		if (model.ChildrenLoaded || !model.IsDirectory) return;

		try {
			foreach (var path in Directory.GetDirectories(model.FullPath)) model.Children.Add(GetModel(path));
			foreach (var path in Directory.GetFiles(model.FullPath)) model.Children.Add(GetModel(path));

			model.ChildrenLoaded = true;
		} catch (Exception _) {
			model.IsAccessible = false;
		}
	}
	
	public void Prune() {
		var expirationTime = TimeSpan.FromMinutes(10);
		var now = DateTime.Now;
		
		//TODO CRITICAL: DONT PRUNE IF THEY'RE OPEN SOMEWHERE 
		
		var keysToRemove = lastAccessTimes
		    .Where(pair => now - pair.Value > expirationTime)
		    .Select(pair => pair.Key);

		foreach (var key in keysToRemove) {
			if (folderCache.TryGetValue(key, out var parent)) {
				parent.ChildrenLoaded = false;
			}
			
			Console.WriteLine($"Pruned Folder {key}");
			folderCache.TryRemove(key, out _);
			lastAccessTimes.TryRemove(key, out _);
			activeWatchers.TryRemove(key, out _);
		}
	}

	public void StartWatching(string path) {
		if (!Directory.Exists(path) || activeWatchers.ContainsKey(path)) return;

		var watcher = new FileSystemWatcher(path) {
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
			IncludeSubdirectories = false
		};

		// TODO: Optimize events to fire in batches
		watcher.Created += OnFileSystemChanged;
		watcher.Deleted += OnFileSystemChanged;
		watcher.Renamed += OnFileSystemRenamed;

		if (activeWatchers.TryAdd(path, watcher)) {
			watcher.EnableRaisingEvents = true;
		}
	}

	public void StopWatching(string path) {
		if (!activeWatchers.TryRemove(path, out var watcher)) return;
		
		watcher.EnableRaisingEvents = false;
		watcher.Dispose();
	}
	
	private void OnFileSystemRenamed(object sender, RenamedEventArgs e) {
		var parentDir = Path.GetDirectoryName(e.FullPath)!;

		Console.WriteLine("FolderDataService OnFileSystemRenamed:");
		Console.WriteLine($"{e.OldName} {e.OldFullPath}");
		Console.WriteLine($"{e.Name} {e.FullPath}");
		
		//TODO: I modify the name but I never reupdate the cache dictionary...
		if (folderCache.TryGetValue(e.OldFullPath, out var model)) {
			model.Name = e.Name!;
			model.FullPath = e.FullPath;

			folderCache.Remove(e.OldFullPath, out _);
			folderCache.TryAdd(e.FullPath, model);
		} else {
			Console.WriteLine("WHYYY");
		}

		// messenger.Send(new FolderContentsChangedMessage(parentDir));
	}

	private void OnFileSystemChanged(object sender, FileSystemEventArgs e) {
		var parentDir = Path.GetDirectoryName(e.FullPath);

		Console.WriteLine($"FolderDataService OnFileSystemChanged: {e.FullPath}");
		
		if (folderCache.TryRemove(parentDir, out var model)) {
			model.ChildrenLoaded = false;
			GetModel(parentDir);
		}

		messenger.Send(new FolderContentsChangedMessage(parentDir));
	}
}

public sealed record FolderContentsChangedMessage(string path);