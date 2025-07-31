using System.Threading.Tasks;
using Xunit;
using GameMacroAssistant.UI.Services;

namespace GameMacroAssistant.Tests.UI;

public class DialogServiceTests
{
    [Fact]
    public void DialogService_CanBeInstantiated()
    {
        // Arrange & Act
        var dialogService = new DialogService();

        // Assert
        Assert.NotNull(dialogService);
    }

    // Note: Testing actual dialog display requires UI thread and is better suited for integration tests
    // This test just verifies the service can be instantiated correctly
}