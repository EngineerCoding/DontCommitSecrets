using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DontCommitSecrets.Client.Utils;

public static class DialogUtils
{
    public static readonly DialogOptions DialogOptions = new DialogOptions()
    {
        MaxWidth = MaxWidth.Small,
        Position = DialogPosition.TopCenter,
    };

    public static async Task OpenForResult<TDialog, TResult>(
        this IDialogService dialogService,
        string title,
        Action<DialogParameters<TDialog>>? dialogParametersAction = null,
        Action<TResult>? resultAction = null,
        Action? cancelledAction = null) where TDialog : ComponentBase
    {
        var dialogParameters = new DialogParameters<TDialog>();
        dialogParametersAction?.Invoke(dialogParameters);

        var reference = dialogService.Show<TDialog>(title, dialogParameters, DialogOptions);
        var dialogReference = await reference.Result;
        if (dialogReference.Canceled)
        {
            cancelledAction?.Invoke();
        }
        else
        {
            resultAction?.Invoke((TResult)dialogReference.Data);
        }
    }
}
