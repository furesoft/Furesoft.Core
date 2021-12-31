using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Furesoft.CodeDom.Generator
{
    [Generator]
    public class NodeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Start building C# code
            var sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
  public static class HelloWorld
  {
    public static void SayHello()
    {
      Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
");

            // Get a list of syntax trees (.cs files)
            var syntaxTrees = context.Compilation.SyntaxTrees;

            // Add output of each file path for each syntax tree
            foreach (SyntaxTree tree in syntaxTrees)
            {
                sourceBuilder.AppendLine($@"Console.WriteLine(@"" - {tree.FilePath}"");");
            }

            // Close generated code block
            sourceBuilder.Append(@"
    }
  }
}");

            foreach (var afile in context.AdditionalFiles)
            {
                var extension = Path.GetExtension(afile.Path);

                if (extension == ".grammar")
                {
                    GenerateNodes(afile.Path, context);
                }
            }

            // Inject the created source
            context.AddSource("helloWorldGenerated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private void GenerateNodes(string path, GeneratorExecutionContext context)
        {
        }
    }
}