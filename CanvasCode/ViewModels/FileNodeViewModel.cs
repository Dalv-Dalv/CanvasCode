using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CanvasCode.Models;
using CanvasCode.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace CanvasCode.ViewModels;

public partial class FileNodeViewModel : ViewModelBase, IRecipient<FolderContentsChangedMessage>, IDisposable {
	private readonly FolderDataService? folderService = null;
	private readonly IMessenger? messenger = null;
	
	[ObservableProperty] private FolderModel model;
	[ObservableProperty] private bool isExpanded;

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

		var sortedChildren = GetSortedChildren(Model);

		foreach (var childModel in sortedChildren) {
			Children.Add(new FileNodeViewModel(childModel, folderService, messenger));
		}

		childrenViewModelsLoaded = true;
	}

	private void SchedulePrune() {
		pruneCts = new CancellationTokenSource();
		var token = pruneCts.Token;
		
		Task.Delay(TimeSpan.FromSeconds(5), token).ContinueWith(t => {
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

	public void Receive(FolderContentsChangedMessage message) {
		if (Model.FullPath != message.path) return;

		folderService.RemoveReference(Model.FullPath, this);
		Model = folderService.GetModel(message.path);
		folderService.AddReference(Model.FullPath, this);
		
		Dispatcher.UIThread.Post(RefreshChildren);
	}

	private IOrderedEnumerable<FolderModel> GetSortedChildren(FolderModel model) {
		var sortedChildModels = model.Children
			 .OrderByDescending(m => m.IsDirectory)
			 .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase);
		return sortedChildModels;
	}

	private void RefreshChildren() {
		folderService.EnsureChildrenAreLoaded(Model);

		var currentChildren = Children.ToDictionary(vm => vm.Model.FullPath);
		var modelChildren = Model.Children.ToDictionary(m => m.FullPath);

		var vmsToRemove = Children.Where(vm => !modelChildren.ContainsKey(vm.Model.FullPath)).ToList();
		foreach (var vmToRemove in vmsToRemove) {
			vmToRemove.Dispose();
			Children.Remove(vmToRemove);
		}

		var sortedChildren = GetSortedChildren(Model);

		foreach (var m in sortedChildren) {
			if(currentChildren.ContainsKey(m.FullPath)) continue;
			
			Children.Add(new FileNodeViewModel(m, folderService, messenger));
		}
	}

	public override string ToString() {
		return Model.Name;
	}

	public void Dispose() {
		messenger?.UnregisterAll(this);
		folderService?.RemoveReference(model.FullPath, this);
		
		foreach (var child in Children) {
			child.Dispose();
		}
	}
}