using System.CommandLine;
using YS.Knife.Cli.Commands;

var rootCommand = new RootCommand("YS.Knife CLI Tool");

// Register commands here. Each command lives in its own file under Commands/.
rootCommand.AddCommand(TestCommand.Create());

return await rootCommand.InvokeAsync(args);
