using System.Windows;
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
}