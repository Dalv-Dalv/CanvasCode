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
	private readonly ConcurrentDictionary<string, HashSet<object>> referenceOwners = new();
	
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

	public void AddReference(string path, object owner) {
		if (!referenceOwners.TryGetValue(path, out var ownerSet)) {
			ownerSet = [];
			if (!referenceOwners.TryAdd(path, ownerSet)) {
				// In case another thread got here first
				referenceOwners.TryGetValue(path, out ownerSet);
			}
		}

		if (ownerSet == null) return;
		
		lock (ownerSet) ownerSet.Add(owner);
	}
	
	public void RemoveReference(string path, object owner) {
		if (!referenceOwners.TryGetValue(path, out var owners)) return;
		
		lock(owners) owners.Remove(owner);
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
		var expirationTime = TimeSpan.FromSeconds(10);
		var now = DateTime.Now;
		
		var keysToRemove = lastAccessTimes
		    .Where(pair => folderCache.TryGetValue(pair.Key, out var m) && m.ChildrenLoaded && now - pair.Value > expirationTime)
		    .Select(pair => pair.Key).ToList();

		foreach (var key in keysToRemove) {
			if (referenceOwners.TryGetValue(key, out var owners)) {
				lock(owners) if (owners.Count > 0) continue; // Its being used by something else
			}

			if (!folderCache.TryGetValue(key, out var m)) continue;

			Console.WriteLine($"Pruned Folder {key}");
			ClearAllChildren(m);
		}
	}

	private void ClearAllChildren(FolderModel model) {
		var nodesToClear = new Stack<FolderModel>();
		nodesToClear.Push(model);

		while (nodesToClear.Count > 0) {
			var currentNode = nodesToClear.Pop();

			if (referenceOwners.TryGetValue(currentNode.FullPath, out var owners)) {
				lock(owners) if(owners.Count > 0) continue;
			}

			foreach (var child in currentNode.Children) {
				if (!child.IsDirectory) continue;
				nodesToClear.Push(child);
			}
			
			currentNode.Children.Clear();
			currentNode.ChildrenLoaded = false;

			folderCache.TryRemove(currentNode.FullPath, out _);
			lastAccessTimes.TryRemove(currentNode.FullPath, out _);
			activeWatchers.TryRemove(currentNode.FullPath, out var watcher);
			watcher?.Dispose();
		}
	}

	public void ClearAll() {
		folderCache.Clear();
		lastAccessTimes.Clear();
		
		foreach(var watcher in activeWatchers.Values) {
			watcher.Dispose();
		}
		activeWatchers.Clear();
	}

	public void StartWatching(string path) {
		if (!Directory.Exists(path) || activeWatchers.ContainsKey(path)) return;

		var watcher = new FileSystemWatcher(path) {
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
			IncludeSubdirectories = true
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
		if (referenceOwners.TryGetValue(path, out var owners)) {
			lock(owners) if (owners.Count > 0) return;
		}
		
		if (!activeWatchers.TryRemove(path, out var watcher)) return;
		
		watcher.Dispose();
	}
	
	private void OnFileSystemRenamed(object sender, RenamedEventArgs e) {
		var parentDir = Path.GetDirectoryName(e.FullPath)!;

		// Console.WriteLine("FolderDataService OnFileSystemRenamed:");
		// Console.WriteLine($"{e.OldName} {e.OldFullPath}");
		// Console.WriteLine($"{e.Name} {e.FullPath}");

		if (lastAccessTimes.TryGetValue(e.OldFullPath, out var dateTime)) {
			lastAccessTimes.Remove(e.OldFullPath, out _);
			lastAccessTimes.TryAdd(e.FullPath, dateTime);
		}

		if (referenceOwners.TryGetValue(e.OldFullPath, out var owners)) {
			referenceOwners.Remove(e.OldFullPath, out _);
			referenceOwners.TryAdd(e.FullPath, owners);
		}

		if (activeWatchers.TryGetValue(e.OldFullPath, out var watcher)) {
			activeWatchers.Remove(e.OldFullPath, out _);
			watcher.Path = e.FullPath;
			if (!activeWatchers.TryAdd(e.FullPath, watcher)) {
				watcher.Dispose();
			}
		}
		
		if (folderCache.TryGetValue(e.OldFullPath, out var model)) {
			model.Name = Path.GetFileName(e.Name!);
			model.FullPath = e.FullPath;

			folderCache.Remove(e.OldFullPath, out _);
			folderCache.TryAdd(e.FullPath, model);
		}
	}

	private void OnFileSystemChanged(object sender, FileSystemEventArgs e) {
		var parentDir = Path.GetDirectoryName(e.FullPath);
		if (parentDir == null) return;

		// Console.WriteLine($"FolderDataService OnFileSystemChanged: {e.FullPath}");

		lastAccessTimes.TryRemove(e.FullPath, out _);
		referenceOwners.TryRemove(e.FullPath, out _);
		if (activeWatchers.TryRemove(e.FullPath, out var watcher)) {
			watcher.Dispose();
		}
		
		if (folderCache.TryRemove(parentDir, out var model)) {
			model.ChildrenLoaded = false;
		}

		messenger.Send(new FolderContentsChangedMessage(parentDir));
	}
}

public sealed record FolderContentsChangedMessage(string path);