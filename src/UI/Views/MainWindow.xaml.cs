using System.Windows;
using System.Windows.Input;
using GameMacroAssistant.UI.ViewModels;
using ReactiveUI;

namespace GameMacroAssistant.UI.Views;

/// <summary>
/// MainWindow.xaml の相互作用ロジック
/// Code-behind は最小限に抑制（ReactiveUI MVVM パターン）
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        Loaded += (_, _) =>
        {
            if (DataContext is IActivatableViewModel avm)
            {
                avm.Activator.Activate();
            }
        };

        Unloaded += (_, _) =>
        {
            if (DataContext is IActivatableViewModel avm)
            {
                avm.Activator.Deactivate();
            }
        };
    }

    private void StopButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // ReactiveCommandが実行される前に停止準備のみ実行（記録状態は変更しない）
        if (DataContext is MainWindowViewModel viewModel && viewModel.IsRecording)
        {
            System.Console.WriteLine("[DEBUG UI] 停止ボタンPreviewMouseDown - 停止準備のみ実行");
            
            // 停止準備を実行（停止関連イベントの抑制開始）
            viewModel.PrepareStopRecording();
            
            // NOTE: IsRecordingは変更しない（ReactiveCommandが実行されなくなるため）
        }
    }
}