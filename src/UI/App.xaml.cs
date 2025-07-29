using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using GameMacroAssistant.Core.Services;

namespace GameMacroAssistant.UI;

/// <summary>
/// App.xaml の相互作用ロジック
/// ReactiveUI + DI コンテナ統合
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
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