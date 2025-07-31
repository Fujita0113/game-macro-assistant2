using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using GameMacroAssistant.Core.Services;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.UI.Services;

namespace GameMacroAssistant.UI;

/// <summary>
/// App.xaml の相互作用ロジック
/// ReactiveUI + DI コンテナ統合
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Contains("--headless"))
        {
            string? macroPath = e.Args.FirstOrDefault(a => a != "--headless");
            if (macroPath == null)
            {
                Console.Error.WriteLine("Macro file path required in headless mode.");
                Shutdown(1);
                return;
            }

            try
            {
                var json = File.ReadAllText(macroPath);
                var macro = JsonSerializer.Deserialize<Macro>(json);
                if (macro == null)
                {
                    Console.Error.WriteLine("Failed to parse macro file.");
                    Shutdown(1);
                    return;
                }

                Console.WriteLine($"Running macro '{macro.Metadata.Name}' silently...");
                var executor = new MacroExecutor();
                await executor.RunAsync(macro);
                Console.WriteLine("Macro execution completed.");
                Shutdown();
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Shutdown(1);
                return;
            }
        }

        // DI コンテナとログ設定
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            })
            .Build();

        // ReactiveUI の DI 統合
        var serviceProvider = _host.Services;
        Locator.CurrentMutable.RegisterConstant(serviceProvider, typeof(IServiceProvider));
        
        // ReactiveUI 初期化
        Locator.CurrentMutable.InitializeReactiveUI();

        // Global exception handlers
        SetupExceptionHandlers();

        // Check for pending crash dumps on startup
        await CheckPendingCrashDumpsAsync();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core layer services
        services.AddHttpClient<ICrashDumpService, CrashDumpService>();
        services.AddSingleton<ICrashDumpService, CrashDumpService>();
        
        // UI layer services
        services.AddSingleton<IDialogService, DialogService>();
        
        // Other services (まだ実装クラスがないため、後で追加)
        // services.AddSingleton<IInputRecorder, InputRecorder>();
        // services.AddSingleton<IScreenshotProvider, ScreenshotProvider>();
        // services.AddTransient<MainWindowViewModel>();
        // services.AddTransient<EditorViewModel>();
    }

    private void SetupExceptionHandlers()
    {
        // Handle WPF unhandled exceptions
        DispatcherUnhandledException += async (sender, e) =>
        {
            await HandleUnhandledException(e.Exception);
            e.Handled = true;
        };

        // Handle application domain unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                await HandleUnhandledException(exception);
            }
        };

        // Handle task scheduler unobserved exceptions
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += async (sender, e) =>
        {
            await HandleUnhandledException(e.Exception);
            e.SetObserved();
        };
    }

    private async System.Threading.Tasks.Task HandleUnhandledException(Exception exception)
    {
        try
        {
            var crashDumpService = _host?.Services.GetService<ICrashDumpService>();
            if (crashDumpService != null)
            {
                await crashDumpService.GenerateCrashDumpAsync(exception);
            }

            var logger = _host?.Services.GetService<ILogger<App>>();
            logger?.LogCritical(exception, "Unhandled exception occurred");

            // Show error dialog to user
            MessageBox.Show($"予期しないエラーが発生しました。\n\nエラー詳細: {exception.Message}\n\nアプリケーションを終了します。",
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            // Last resort - at least try to show something to the user
            MessageBox.Show($"重大なエラーが発生しました: {ex.Message}", "エラー", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async System.Threading.Tasks.Task CheckPendingCrashDumpsAsync()
    {
        try
        {
            var crashDumpService = _host?.Services.GetService<ICrashDumpService>();
            if (crashDumpService == null) return;

            var hasPendingDumps = await crashDumpService.HasPendingCrashDumpsAsync();
            if (hasPendingDumps)
            {
                var consent = await crashDumpService.RequestUploadConsentAsync();
                if (consent)
                {
                    await crashDumpService.UploadPendingDumpsAsync();
                }
            }

            // Cleanup old dumps regardless
            await crashDumpService.CleanupOldDumpsAsync();
        }
        catch (Exception ex)
        {
            var logger = _host?.Services.GetService<ILogger<App>>();
            logger?.LogError(ex, "Failed to check pending crash dumps");
        }
    }
}