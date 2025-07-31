using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using GameMacroAssistant.Core.Services;

namespace GameMacroAssistant.Tests.Core;

public class CrashDumpServiceTests : IDisposable
{
    private readonly Mock<ILogger<CrashDumpService>> _loggerMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly HttpClient _httpClient;
    private readonly string _testDirectory;
    private readonly CrashDumpService _crashDumpService;

    public CrashDumpServiceTests()
    {
        _loggerMock = new Mock<ILogger<CrashDumpService>>();
        _dialogServiceMock = new Mock<IDialogService>();
        _httpClient = new HttpClient();
        
        // Create temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), "CrashDumpTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _crashDumpService = new TestCrashDumpService(_loggerMock.Object, _httpClient, _dialogServiceMock.Object, _testDirectory);
    }

    [Fact]
    public async Task GenerateCrashDumpAsync_ShouldCreateDumpFile()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Debug: Check directory exists and is writable
        Assert.True(Directory.Exists(_testDirectory), $"Test directory should exist: {_testDirectory}");
        
        // Create a test file to ensure directory is writable
        var testFile = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "test");
        Assert.True(File.Exists(testFile), "Should be able to write to test directory");
        File.Delete(testFile);

        // Debug: First test the JSON serialization in isolation
        try
        {
            var testReport = new
            {
                Timestamp = DateTime.UtcNow,
                Exception = new
                {
                    Type = exception.GetType().FullName,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace
                }
            };
            var testJson = JsonSerializer.Serialize(testReport, new JsonSerializerOptions { WriteIndented = true });
            Assert.NotNull(testJson);
            Assert.Contains("Test exception", testJson);
        }
        catch (Exception ex)
        {
            Assert.Fail($"JSON serialization failed: {ex}");
        }

        // Act
        await _crashDumpService.GenerateCrashDumpAsync(exception);

        // Debug: Wait a bit for async operations
        await Task.Delay(100);

        // Debug: List all files in directory
        var allFiles = Directory.GetFiles(_testDirectory);
        var crashFiles = Directory.GetFiles(_testDirectory, "crash_*.json");
        
        if (crashFiles.Length == 0)
        {
            Assert.Fail($"No crash files found. All files in directory: {string.Join(", ", allFiles.Select(Path.GetFileName))}");
        }
        
        // Assert
        Assert.Single(crashFiles);
        
        var content = await File.ReadAllTextAsync(crashFiles[0]);
        Assert.Contains("Test exception", content);
        Assert.Contains("InvalidOperationException", content);
    }

    [Fact]
    public async Task HasPendingCrashDumpsAsync_WithNoDumps_ShouldReturnFalse()
    {
        // Act
        var result = await _crashDumpService.HasPendingCrashDumpsAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPendingCrashDumpsAsync_WithDumps_ShouldReturnTrue()
    {
        // Arrange
        var exception = new Exception("Test");
        await _crashDumpService.GenerateCrashDumpAsync(exception);

        // Act
        var result = await _crashDumpService.HasPendingCrashDumpsAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RequestUploadConsentAsync_ShouldCallDialogService()
    {
        // Arrange
        _dialogServiceMock.Setup(x => x.ShowCrashDumpConsentDialogAsync())
                         .ReturnsAsync(true);

        // Act
        var result = await _crashDumpService.RequestUploadConsentAsync();

        // Assert
        Assert.True(result);
        _dialogServiceMock.Verify(x => x.ShowCrashDumpConsentDialogAsync(), Times.Once);
    }

    [Fact]
    public async Task UploadPendingDumpsAsync_ShouldProcessAllDumps()
    {
        // Arrange
        var exception1 = new Exception("Test 1");
        var exception2 = new Exception("Test 2");
        await _crashDumpService.GenerateCrashDumpAsync(exception1);
        await _crashDumpService.GenerateCrashDumpAsync(exception2);

        // Act
        await _crashDumpService.UploadPendingDumpsAsync();

        // Assert
        var allFiles = Directory.GetFiles(_testDirectory);
        var originalFiles = allFiles.Where(f => f.EndsWith(".json") && !f.EndsWith(".uploaded.json")).ToArray();
        var uploadedFiles = allFiles.Where(f => f.EndsWith(".uploaded.json")).ToArray();
        
        Assert.Empty(originalFiles);
        Assert.Equal(2, uploadedFiles.Length);
    }

    [Fact]
    public async Task CleanupOldDumpsAsync_ShouldDeleteOldFiles()
    {
        // Arrange
        var oldFileName = Path.Combine(_testDirectory, "old_crash.json");
        await File.WriteAllTextAsync(oldFileName, "old dump");
        
        // Set file creation time to 100 days ago
        var oldDate = DateTime.UtcNow.AddDays(-100);
        File.SetCreationTimeUtc(oldFileName, oldDate);

        var newFileName = Path.Combine(_testDirectory, "new_crash.json");
        await File.WriteAllTextAsync(newFileName, "new dump");

        // Act
        await _crashDumpService.CleanupOldDumpsAsync();

        // Assert
        Assert.False(File.Exists(oldFileName));
        Assert.True(File.Exists(newFileName));
    }

    [Fact]
    public async Task GenerateCrashDumpAsync_WithInnerException_ShouldIncludeInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner exception");
        var outerException = new InvalidOperationException("Outer exception", innerException);

        // Act
        await _crashDumpService.GenerateCrashDumpAsync(outerException);

        // Assert
        var files = Directory.GetFiles(_testDirectory, "crash_*.json");
        var content = await File.ReadAllTextAsync(files[0]);
        Assert.Contains("Inner exception", content);
        Assert.Contains("ArgumentException", content);
        Assert.Contains("Outer exception", content);
        Assert.Contains("InvalidOperationException", content);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    // Test implementation that allows injecting custom directory
    private class TestCrashDumpService : CrashDumpService
    {
        public TestCrashDumpService(ILogger<CrashDumpService> logger, HttpClient httpClient, IDialogService dialogService, string testDirectory)
            : base(logger, httpClient, dialogService, testDirectory)
        {
        }
    }
}