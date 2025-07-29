using System.Text.Json;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;

namespace GameMacroAssistant.CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        bool headless = false;
        string? macroPath = null;

        foreach (var arg in args)
        {
            if (arg == "--headless") headless = true;
            else if (macroPath == null) macroPath = arg;
        }

        if (!headless)
        {
            Console.WriteLine("Headless flag not specified. Launching WPF UI...");
            // In a full implementation we would launch the WPF app here.
            return 0;
        }

        if (macroPath == null)
        {
            Console.Error.WriteLine("Macro file path required in headless mode.");
            return 1;
        }

        try
        {
            var json = File.ReadAllText(macroPath);
            var macro = JsonSerializer.Deserialize<Macro>(json);
            if (macro == null)
            {
                Console.Error.WriteLine("Failed to parse macro file.");
                return 1;
            }

            Console.WriteLine($"Running macro '{macro.Metadata.Name}' silently...");
            var executor = new MacroExecutor();
            await executor.RunAsync(macro);
            Console.WriteLine("Macro execution completed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
