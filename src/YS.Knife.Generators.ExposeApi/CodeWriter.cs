using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FlyTiger
{
    class CodeWriter(Compilation compilation, SourceProductionContext context)
    {
        public string CodeFileSuffix { get; set; } = "g.cs";

        public Compilation Compilation { get; private set; } = compilation;
        public SourceProductionContext Context { get; private set; } = context;

        private readonly Dictionary<string, int> fileNames = new(StringComparer.InvariantCultureIgnoreCase);

        public void WriteCodeFile(CodeFile codeFile)
        {
            if (codeFile == null) return;

            fileNames.TryGetValue(codeFile.BasicName, out var i);
            var name = i == 0 ? codeFile.BasicName : $"{codeFile.BasicName}.{i + 1}";
            fileNames[codeFile.BasicName] = i + 1;

            var sourceName = $"{name}.{CodeFileSuffix}";
            var sourceText = SourceText.From(codeFile.Content, Encoding.UTF8);

            Context.AddSource(sourceName, sourceText);
        }
        private readonly Dictionary<string, int> codeNames = new();

        public string GetUniqueCodeName(string group, string basicName)
        {
            var fullName = $"{group}::{basicName}";
            codeNames.TryGetValue(fullName, out var i);
            var name = i == 0 ? basicName : $"{basicName}{i + 1}";
            codeNames[fullName] = i + 1;
            return name;

        }
    }

}
