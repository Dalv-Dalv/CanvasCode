using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CanvasCode.Models;
using CanvasCode.Others;
using CanvasCode.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels;

public partial class FileNodeViewModel : ViewModelBase, 
                                         IRecipient<FolderContentsChangedMessage>,
                                         IRecipient<FileRenamedMessage>, IDisposable {
	private readonly FolderDataService? folderService = null;
	private readonly IMessenger? messenger = null;
	
	[ObservableProperty] private FolderModel model;
	[ObservableProperty] private bool isExpanded = false;
	[ObservableProperty] private bool isRenaming = false;

	private bool childrenViewModelsLoaded;
	private CancellationTokenSource? pruneCts;

	public ObservableCollection<FileNodeViewModel> Children { get; } = [];

	
	public FileNodeViewModel(FolderModel model, FolderDataService service, IMessenger messenger) {
		Model = model;
		folderService = service;
		this.messenger = messenger;

		folderService.AddReference(model.FullPath, this);
		
		messenger.RegisterAll(this);
		
		if (!Model.IsDirectory) return;
		
		if (ModelHasChildren()) 
			Children.Add(new FileNodeViewModel()); // Dummy node
	}

	private FileNodeViewModel() {
		Model = new FolderModel("Loading...", "", false);
	}
	
	async partial void OnIsExpandedChanged(bool value) {
		if (!value) { // Collapsing 
			if (!childrenViewModelsLoaded) return;
			SchedulePrune();
			return;
		}
		
		// Cancel any pending prune operation
		pruneCts?.Cancel();
		pruneCts = null;

		if (childrenViewModelsLoaded) return;

		await Task.Run(() => folderService.EnsureChildrenAreLoaded(Model));
		
		Children.Clear();

		foreach (var childModel in Model.Children) {
			Children.Add(new FileNodeViewModel(childModel, folderService, messenger));
		}
		
		SortChildren();

		childrenViewModelsLoaded = true;
	}

	[RelayCommand]
	private void Delete() {
		Console.WriteLine($"Deleting file {Model.Name}");
		folderService?.Delete(Model.FullPath);
	}

	[RelayCommand]
	private void StartRename() {
		Console.WriteLine($"Starting rename on file {Model.Name}");
		IsRenaming = true;
	}

	[RelayCommand]
	private void CommitRename(string newName) {
		bool ValidateName(string name) {
			return !string.IsNullOrWhiteSpace(name) && !name.Any(ch => "\\/:*?\"<>|".Contains(ch));
		}
		
		if (!IsRenaming) return;
		
		Console.WriteLine($"Committing rename on file {Model.Name}, new name is {newName}");
		CancelRename();

		if (!ValidateName(newName)) return;
		
		folderService?.Rename(Model.FullPath, newName);
	}

	[RelayCommand]
	private void CancelRename() {
		Console.WriteLine($"Canceled rename on file {Model.Name}");
		IsRenaming = false;
	}

	[RelayCommand]
	private void Refresh() {
		Console.WriteLine($"Refresh file {Model.Name}");
		
		RefreshChildren();
	}

	private void SchedulePrune() {
		pruneCts = new CancellationTokenSource();
		var token = pruneCts.Token;
		
		Task.Delay(TimeSpan.FromMinutes(10), token).ContinueWith(t => {
			if (t.IsCanceled) return;

			Dispatcher.UIThread.Post(() => {
				foreach (var child in Children) {
					child.Dispose();
				}
				Children.Clear();
				childrenViewModelsLoaded = false;
				
				if (ModelHasChildren()) Children.Add(new FileNodeViewModel());
			});
		}, TaskScheduler.Default);
	}

	public void Receive(FileRenamedMessage message) {
		if (Model.FullPath != message.parentPath) return;
		
		Dispatcher.UIThread.Post(() => SortItem(message.parentPath, message.newFullPath));
	}
	
	public void Receive(FolderContentsChangedMessage message) {
		if (Model.FullPath != message.path) return;

		folderService.RemoveReference(Model.FullPath, this);
		Model = folderService.GetModel(message.path);
		folderService.AddReference(Model.FullPath, this);
		
		Dispatcher.UIThread.Post(RefreshChildren);
	}

	private void SortItem(string parentPath, string path) {
		int insertIndex = -1, oldIndex = -1;

		var target = Children.FirstOrDefault(vm => vm.Model.FullPath == path);
		if (target == null) return;
		
		for (int i = 0; i < Children.Count; i++) {
			var crnt = Children[i].Model;
			
			if (crnt.FullPath == path) oldIndex = i;

			if (insertIndex >= 0) continue;

			if(!target.Model.IsDirectory && crnt.IsDirectory) continue;

			bool isUnorderedAlphabetically = string.Compare(target.Model.Name, crnt.Name, StringComparison.OrdinalIgnoreCase) <= 0; 
			if (target.Model.IsDirectory && !crnt.IsDirectory || isUnorderedAlphabetically) {
				insertIndex = i;
				if (oldIndex >= 0) break;
			}
		}

		if (insertIndex < 0 || oldIndex < 0) return;
		
		Children.Move(oldIndex, insertIndex);
	}
	private void SortChildren() {
		var sortedChildren = Children
		                     .OrderByDescending(vm => vm.Model.IsDirectory)
		                     .ThenBy(vm => vm.Model.Name, StringComparer.OrdinalIgnoreCase)
		                     .ToList();
		for (int i = 0; i < sortedChildren.Count; i++) {
			var itemToMove = sortedChildren[i];
			int currentIndex = Children.IndexOf(itemToMove);
			
			if (currentIndex == i) continue;
			
			Children.Move(currentIndex, i);
		}
	}

	private void RefreshChildren() {
		folderService?.EnsureChildrenAreLoaded(Model);
	
		var currentChildren = Children.ToDictionary(vm => vm.Model.FullPath);
		var modelChildren = Model.Children.ToDictionary(m => m.FullPath);

		var vmsToRemove = Children.Where(vm => !modelChildren.ContainsKey(vm.Model.FullPath)).ToList();
		foreach (var vmToRemove in vmsToRemove) {
			vmToRemove.Dispose();
			Children.Remove(vmToRemove);
		}

		foreach (var m in Model.Children) {
			if(currentChildren.ContainsKey(m.FullPath)) continue;
			
			Children.Add(new FileNodeViewModel(m, folderService, messenger));
		}
		
		SortChildren();
	}
	
	private bool ModelHasChildren() {
		try {
			if (!childrenViewModelsLoaded && Directory.EnumerateFileSystemEntries(Model.FullPath).Any()) {
				return true;
			}
		} catch (Exception _) {
			return false;
		}
		return false;
	}

	public void Dispose() {
		messenger?.UnregisterAll(this);
		folderService?.RemoveReference(model.FullPath, this);
		
		foreach (var child in Children) {
			child.Dispose();
		}
	}
	
	public override string ToString() {
		return Model.Name;
	}
}