using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace GameMacroAssistant.Tests.UI
{
    public class AccessibilityTestsFixture : IDisposable
    {
        public WindowsDriver<WindowsElement> Driver { get; private set; }
        
        public AccessibilityTestsFixture()
        {
            // WinAppDriverが起動していることを前提とする
            var appOptions = new AppiumOptions();
            
            // テスト用のアプリケーションを起動
            var exePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "UI", "bin", "Debug", 
                "net8.0-windows10.0.19041.0", "GameMacroAssistant.UI.exe");
            
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Test application not found at: {exePath}");
            }

            appOptions.AddAdditionalCapability("app", exePath);
            appOptions.AddAdditionalCapability("deviceName", "WindowsPC");
            
            try
            {
                Driver = new WindowsDriver<WindowsElement>(
                    new Uri("http://127.0.0.1:4723"), appOptions);
                
                // アプリケーションの起動を待機
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize WinAppDriver: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            Driver?.Quit();
        }
    }

    public class AccessibilityTests : IClassFixture<AccessibilityTestsFixture>
    {
        private readonly AccessibilityTestsFixture _fixture;

        public AccessibilityTests(AccessibilityTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void RecordButton_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            var recordButton = _fixture.Driver.FindElementByName("記録開始");
            
            // Assert
            Assert.NotNull(recordButton);
            
            // AutomationProperties.Name が設定されているかテスト
            var automationName = recordButton.GetAttribute("Name");
            Assert.False(string.IsNullOrEmpty(automationName));
            
            // AutomationProperties.HelpText が設定されているかテスト
            var helpText = recordButton.GetAttribute("HelpText");
            Assert.False(string.IsNullOrEmpty(helpText));
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void StopButton_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            var stopButton = _fixture.Driver.FindElementByName("停止");
            
            // Assert
            Assert.NotNull(stopButton);
            
            var automationName = stopButton.GetAttribute("Name");
            Assert.False(string.IsNullOrEmpty(automationName));
            
            var helpText = stopButton.GetAttribute("HelpText");
            Assert.False(string.IsNullOrEmpty(helpText));
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void PlayButton_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            var playButton = _fixture.Driver.FindElementByName("再生");
            
            // Assert
            Assert.NotNull(playButton);
            
            var automationName = playButton.GetAttribute("Name");
            Assert.False(string.IsNullOrEmpty(automationName));
            
            var helpText = playButton.GetAttribute("HelpText");
            Assert.False(string.IsNullOrEmpty(helpText));
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void SettingsButton_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            var settingsButton = _fixture.Driver.FindElementByName("設定");
            
            // Assert
            Assert.NotNull(settingsButton);
            
            var automationName = settingsButton.GetAttribute("Name");
            Assert.False(string.IsNullOrEmpty(automationName));
            
            var helpText = settingsButton.GetAttribute("HelpText");
            Assert.False(string.IsNullOrEmpty(helpText));
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void MacrosList_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            // TabControlからマクロ一覧タブを選択
            try
            {
                var macroTab = _fixture.Driver.FindElementByName("マクロ一覧");
                macroTab?.Click();
                
                Thread.Sleep(500); // UI更新を待機
                
                // ListBoxを探す
                var macrosList = _fixture.Driver.FindElementByClassName("ListBox");
                
                // Assert
                Assert.NotNull(macrosList);
                
                var automationName = macrosList.GetAttribute("Name");
                Assert.False(string.IsNullOrEmpty(automationName));
                
                var helpText = macrosList.GetAttribute("HelpText");
                Assert.False(string.IsNullOrEmpty(helpText));
            }
            catch (Exception ex)
            {
                // TabやListBoxが見つからない場合はテスト失敗
                Assert.Fail($"Macros list or tab should be accessible: {ex.Message}");
            }
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void TabControl_ShouldHaveAutomationProperties()
        {
            // Arrange & Act
            var tabControl = _fixture.Driver.FindElementByClassName("TabControl");
            
            // Assert
            Assert.NotNull(tabControl);
            
            var automationName = tabControl.GetAttribute("Name");
            Assert.False(string.IsNullOrEmpty(automationName));
        }

        [Fact(Skip = "Requires WinAppDriver to be running")]
        public void AllInteractiveElements_ShouldBeAccessible()
        {
            // すべての対話可能な要素が適切なUIAプロパティを持っているかの総合テスト
            
            // ボタン要素のテスト
            var buttons = _fixture.Driver.FindElementsByClassName("Button");
            Assert.True(buttons.Count > 0);
            
            foreach (var button in buttons)
            {
                var name = button.GetAttribute("Name");
                Assert.False(string.IsNullOrEmpty(name));
            }
            
            // TabItem要素のテスト
            var tabItems = _fixture.Driver.FindElementsByClassName("TabItem");
            foreach (var tabItem in tabItems)
            {
                var name = tabItem.GetAttribute("Name");
                Assert.False(string.IsNullOrEmpty(name));
            }
        }
    }
}