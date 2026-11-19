using System.CommandLine;

namespace YS.Knife.Cli.Commands;

/// <summary>
/// The <c>test</c> command, used to verify the tool is installed and working.
/// </summary>
internal static class TestCommand
{
    public static Command Create()
    {
        var nameOption = new Option<string?>(
            aliases: new[] { "--name", "-n" },
            description: "An optional name to greet.");

        var command = new Command("test", "Verify that the YS.Knife CLI tool is working correctly.")
        {
            nameOption
        };

        command.SetHandler(name =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Hello from YS.Knife.CLI! The tool is working correctly.");
            }
            else
            {
                Console.WriteLine($"Hello, {name}! from YS.Knife.CLI!");
            }
        }, nameOption);

        return command;
    }
}
