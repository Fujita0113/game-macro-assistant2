using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using GameMacroAssistant.Core.Services;
using GameMacroAssistant.Core.Models;

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

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core layer services (まだ実装クラスがないため、後で追加)
        // services.AddSingleton<IInputRecorder, InputRecorder>();
        // services.AddSingleton<IScreenshotProvider, ScreenshotProvider>();

        // UI layer services
        // services.AddTransient<MainWindowViewModel>();
        // services.AddTransient<EditorViewModel>();
    }
}