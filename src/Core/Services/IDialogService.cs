using System.Threading.Tasks;

namespace GameMacroAssistant.Core.Services;

public interface IDialogService
{
    Task<bool> ShowCrashDumpConsentDialogAsync();
}