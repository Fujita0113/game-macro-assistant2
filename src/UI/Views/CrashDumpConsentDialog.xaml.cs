using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GameMacroAssistant.UI.Views;

public partial class CrashDumpConsentDialog : Window
{
    public bool ConsentGiven { get; private set; }

    public CrashDumpConsentDialog()
    {
        InitializeComponent();
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        ConsentGiven = true;
        DialogResult = true;
        Close();
    }

    private void DeclineButton_Click(object sender, RoutedEventArgs e)
    {
        ConsentGiven = false;
        DialogResult = false;
        Close();
    }

    private void DetailsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var privacyPolicyPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "docs", "privacy_policy.md");

            if (File.Exists(privacyPolicyPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = privacyPolicyPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show(
                    "プライバシーポリシーファイルが見つかりません。",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"プライバシーポリシーを開けませんでした: {ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}