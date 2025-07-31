using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCode.ViewModels.Dialogs;

public abstract partial class DialogViewModel : ViewModelBase {
	[ObservableProperty] private bool isOpen;
	[ObservableProperty] private Point position;

	protected TaskCompletionSource closeTask = new();

	public event Action OnShow, OnClose;

	public async Task WaitAsync() {
		await closeTask.Task;
	}
	
	public void Show() {
		if(closeTask.Task.IsCompleted || closeTask.Task.IsCanceled)
			closeTask = new TaskCompletionSource();
		IsOpen = true;

		OnShow?.Invoke();
	}

	public void Close() {
		if (!IsOpen) return;
		
		IsOpen = false;
		closeTask.TrySetResult();
		
		OnClose?.Invoke();
	}
}