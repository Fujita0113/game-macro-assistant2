using FluentAssertions;
using GameMacroAssistant.Core.Models;
using System.Text.Json;
using Xunit;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// Macro モデルの単体テスト
/// JSON シリアライゼーション・デシリアライゼーションを含む
/// </summary>
public class MacroTests
{
    [Fact]
    public void Macro_Should_Serialize_To_Json_Correctly()
    {
        // Arrange
        var macro = new Macro
        {
            Version = "1.0",
            Metadata = new MacroMetadata
            {
                Name = "テストマクロ",
                Description = "単体テスト用",
                Created = new DateTime(2025, 7, 29, 10, 30, 45, DateTimeKind.Utc),
                Modified = new DateTime(2025, 7, 29, 11, 15, 22, DateTimeKind.Utc),
                Encrypted = false
            },
            Steps = new List<MacroStep>
            {
                new MacroStep
                {
                    Id = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
                    Type = MacroStepType.Mouse,
                    TimestampMs = 1000,
                    Data = new MouseStepData
                    {
                        X = 100,
                        Y = 200,
                        Button = "left",
                        Action = "click"
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(macro, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        // Assert
        json.Should().Contain("\"version\": \"1.0\"");
        json.Should().Contain("\"name\": \"テストマクロ\"");
        json.Should().Contain("\"type\": \"Mouse\"");
        json.Should().Contain("\"x\": 100");
        json.Should().Contain("\"y\": 200");
    }

    [Fact]
    public void MacroMetadata_Should_Initialize_With_Required_Properties()
    {
        // Arrange
        var created = DateTime.UtcNow;
        var modified = DateTime.UtcNow.AddMinutes(5);

        // Act
        var metadata = new MacroMetadata
        {
            Name = "サンプルマクロ",
            Description = "説明文",
            Created = created,
            Modified = modified,
            Encrypted = true
        };

        // Assert
        metadata.Name.Should().Be("サンプルマクロ");
        metadata.Description.Should().Be("説明文");
        metadata.Created.Should().Be(created);
        metadata.Modified.Should().Be(modified);
        metadata.Encrypted.Should().BeTrue();
    }

    [Fact]
    public void MacroStep_Should_Support_Different_Step_Types()
    {
        // Arrange & Act
        var mouseStep = new MacroStep
        {
            Id = Guid.NewGuid(),
            Type = MacroStepType.Mouse,
            TimestampMs = 500,
            Data = new MouseStepData { X = 10, Y = 20, Button = "left", Action = "click" }
        };

        var keyboardStep = new MacroStep
        {
            Id = Guid.NewGuid(),
            Type = MacroStepType.Keyboard,
            TimestampMs = 1000,
            Data = new KeyboardStepData { VirtualKey = 65, Action = "press", Modifiers = new[] { "ctrl" } }
        };

        var delayStep = new MacroStep
        {
            Id = Guid.NewGuid(),
            Type = MacroStepType.Delay,
            TimestampMs = 1500,
            Data = new DelayStepData { Duration = 2000 }
        };

        // Assert
        mouseStep.Type.Should().Be(MacroStepType.Mouse);
        keyboardStep.Type.Should().Be(MacroStepType.Keyboard);
        delayStep.Type.Should().Be(MacroStepType.Delay);
    }

    [Theory]
    [InlineData(MacroStepType.Mouse, "Mouse")]
    [InlineData(MacroStepType.Keyboard, "Keyboard")]
    [InlineData(MacroStepType.Image, "Image")]
    [InlineData(MacroStepType.Delay, "Delay")]
    [InlineData(MacroStepType.Comment, "Comment")]
    public void MacroStepType_Enum_Should_Serialize_As_String(MacroStepType stepType, string expectedString)
    {
        // Act
        var json = JsonSerializer.Serialize(stepType);

        // Assert
        json.Should().Be($"\"{expectedString}\"");
    }

    [Fact]
    public void ImageStepData_Should_Handle_Base64_Template()
    {
        // Arrange
        var base64Image = Convert.ToBase64String(new byte[] { 137, 80, 78, 71 }); // PNG header
        
        // Act
        var imageData = new ImageStepData
        {
            Template = base64Image,
            Region = new ImageRegion { X = 0, Y = 0, Width = 100, Height = 50 },
            Threshold = new ImageThreshold { Ssim = 0.95, PixelDiff = 0.03 }
        };

        // Assert
        imageData.Template.Should().Be(base64Image);
        imageData.Region.Width.Should().Be(100);
        imageData.Region.Height.Should().Be(50);
        imageData.Threshold.Ssim.Should().Be(0.95);
        imageData.Threshold.PixelDiff.Should().Be(0.03);
    }
}