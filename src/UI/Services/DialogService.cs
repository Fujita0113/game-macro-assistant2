using System.Threading.Tasks;
using System.Windows;
using GameMacroAssistant.Core.Services;
using GameMacroAssistant.UI.Views;

namespace GameMacroAssistant.UI.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ShowCrashDumpConsentDialogAsync()
    {
        return await Task.Run(() =>
        {
            var dialog = new CrashDumpConsentDialog();
            
            // Show dialog on UI thread
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = dialog.ShowDialog();
                return result == true && dialog.ConsentGiven;
            });
        });
    }
}