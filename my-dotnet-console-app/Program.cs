using System;
using System.IO;

namespace MyDotNetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Program.exe <TemplateFilePath> <XmlFilePath>");
                return;
            }

            string templateFilePath = args[0];
            string xmlFilePath = args[1];

            // Source Generator を使用してテンプレートを処理
            var generator = new TemplateProcessorGenerator();
            var context = new GeneratorExecutionContext();
            context.AnalyzerConfigOptions.GlobalOptions["build_property.TemplateFilePath"] = templateFilePath;
            context.AnalyzerConfigOptions.GlobalOptions["build_property.XmlFilePath"] = xmlFilePath;

            generator.Execute(context);

            Console.WriteLine($"Output written to {Path.ChangeExtension(templateFilePath, ".txt")}");
        }
    }
}
