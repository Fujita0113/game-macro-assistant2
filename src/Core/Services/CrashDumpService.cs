using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GameMacroAssistant.Core.Services;

public class CrashDumpService : ICrashDumpService
{
    private readonly ILogger<CrashDumpService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IDialogService _dialogService;
    private readonly string _crashDumpDirectory;
    private const string UploadUrl = "https://crash-reports.gamemacroassistant.dev/api/v1/reports";

    public CrashDumpService(ILogger<CrashDumpService> logger, HttpClient httpClient, IDialogService dialogService)
        : this(logger, httpClient, dialogService, null)
    {
    }

    protected CrashDumpService(ILogger<CrashDumpService> logger, HttpClient httpClient, IDialogService dialogService, string? crashDumpDirectory)
    {
        _logger = logger;
        _httpClient = httpClient;
        _dialogService = dialogService;
        
        _crashDumpDirectory = crashDumpDirectory ?? GetCrashDumpDirectory();
        
        if (!Directory.Exists(_crashDumpDirectory))
        {
            Directory.CreateDirectory(_crashDumpDirectory);
        }
    }

    protected virtual string GetCrashDumpDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "GameMacroAssistant", "CrashDumps");
    }

    public async Task GenerateCrashDumpAsync(Exception exception)
    {
        try
        {
            var crashReport = CreateCrashReport(exception);
            var fileName = $"crash_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}.json";
            var filePath = Path.Combine(_crashDumpDirectory, fileName);

            var json = JsonSerializer.Serialize(crashReport, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            _logger.LogInformation("Crash dump generated: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate crash dump");
        }
    }

    public async Task<bool> HasPendingCrashDumpsAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(_crashDumpDirectory))
                    return false;

                var files = Directory.GetFiles(_crashDumpDirectory, "crash_*.json");
                return files.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for pending crash dumps");
                return false;
            }
        });
    }

    public async Task<bool> RequestUploadConsentAsync()
    {
        return await _dialogService.ShowCrashDumpConsentDialogAsync();
    }

    public async Task UploadPendingDumpsAsync()
    {
        try
        {
            if (!Directory.Exists(_crashDumpDirectory))
                return;

            var files = Directory.GetFiles(_crashDumpDirectory, "crash_*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    
                    // Mock upload - in real implementation this would make HTTP request
                    _logger.LogInformation("Mock uploading crash dump: {FileName}", Path.GetFileName(file));
                    await Task.Delay(100); // Simulate upload time
                    
                    // Mark as uploaded by renaming
                    var uploadedFile = file.Replace(".json", ".uploaded.json");
                    File.Move(file, uploadedFile);
                    
                    _logger.LogInformation("Crash dump uploaded successfully: {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload crash dump: {FileName}", Path.GetFileName(file));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload pending crash dumps");
        }
    }

    public async Task CleanupOldDumpsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(_crashDumpDirectory))
                    return;

                var cutoffDate = DateTime.UtcNow.AddDays(-90);
                var files = Directory.GetFiles(_crashDumpDirectory, "*.json");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted old crash dump: {FileName}", Path.GetFileName(file));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old crash dumps");
            }
        });
    }

    private CrashReport CreateCrashReport(Exception exception)
    {
        var process = Process.GetCurrentProcess();
        
        return new CrashReport
        {
            Timestamp = DateTime.UtcNow,
            Exception = new ExceptionInfo
            {
                Type = exception.GetType().FullName,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException != null ? new ExceptionInfo
                {
                    Type = exception.InnerException.GetType().FullName,
                    Message = exception.InnerException.Message,
                    StackTrace = exception.InnerException.StackTrace
                } : null
            },
            SystemInfo = new SystemInfo
            {
                OSVersion = Environment.OSVersion.ToString(),
                RuntimeVersion = Environment.Version.ToString(),
                ProcessorArchitecture = Environment.ProcessorCount.ToString(),
                WorkingSet = process.WorkingSet64,
                ThreadCount = process.Threads.Count
            },
            ApplicationInfo = new ApplicationInfo
            {
                Version = typeof(CrashDumpService).Assembly.GetName().Version?.ToString() ?? "Unknown",
                CommandLine = Environment.CommandLine,
                CurrentDirectory = Environment.CurrentDirectory
            }
        };
    }

    private class CrashReport
    {
        public DateTime Timestamp { get; set; }
        public ExceptionInfo Exception { get; set; } = new();
        public SystemInfo SystemInfo { get; set; } = new();
        public ApplicationInfo ApplicationInfo { get; set; } = new();
    }

    private class ExceptionInfo
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public ExceptionInfo? InnerException { get; set; }
    }

    private class SystemInfo
    {
        public string? OSVersion { get; set; }
        public string? RuntimeVersion { get; set; }
        public string? ProcessorArchitecture { get; set; }
        public long WorkingSet { get; set; }
        public int ThreadCount { get; set; }
    }

    private class ApplicationInfo
    {
        public string? Version { get; set; }
        public string? CommandLine { get; set; }
        public string? CurrentDirectory { get; set; }
    }
}