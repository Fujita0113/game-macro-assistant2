using System;
using System.Threading.Tasks;

namespace GameMacroAssistant.Core.Services;

public interface ICrashDumpService
{
    Task GenerateCrashDumpAsync(Exception exception);
    Task<bool> HasPendingCrashDumpsAsync();
    Task<bool> RequestUploadConsentAsync();
    Task UploadPendingDumpsAsync();
    Task CleanupOldDumpsAsync();
}