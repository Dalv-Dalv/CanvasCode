using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CanvasCode.ViewModels.Dialogs;

public partial class ConfirmDialogViewModel : DialogViewModel {

	[ObservableProperty] private string title = "Confirm";
	[ObservableProperty] private string message = "Are you sure?";
	[ObservableProperty] private string confirmText = "Ok";
	[ObservableProperty] private string cancelText = "Cancel";
	
	[ObservableProperty] private string icon = "";
	
	[ObservableProperty] private string statusText = "";
	[ObservableProperty] private string progressText = "";
	
	[ObservableProperty] private double dialogWidth = double.NaN;
	[ObservableProperty] private double dialogHeight = double.NaN;
	
	[ObservableProperty] private bool confirmed = false;
	
	[NotifyCanExecuteChangedFor(nameof(CancelCommand))]
	[ObservableProperty] private bool isBusy = false;

	public Func<ConfirmDialogViewModel, Task<bool>> OnConfirm { get; set; } = _ => Task.FromResult(true);
	
	[RelayCommand]
	public async Task ConfirmAsync() {
		if (IsBusy) return;
		
		IsBusy = true;

		StatusText = "";
		ProgressText = "Processing...";

		var result = await OnConfirm(this);

		IsBusy = false;

		if (!result) return;
		
		Confirmed = true;
		Close();
	}


	private bool IsNotBusy() => !IsBusy;
	[RelayCommand(CanExecute = nameof(IsNotBusy))]
	public void Cancel() {
		Confirmed = false;
		Close();
	}
}