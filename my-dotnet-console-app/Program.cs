using System;
using System.IO;
using Microsoft.VisualStudio.TextTemplating;

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

            string templateContent = File.ReadAllText(templateFilePath);
            string outputContent = ProcessTemplate(templateContent, xmlFilePath);

            string outputFilePath = Path.ChangeExtension(templateFilePath, ".txt");
            File.WriteAllText(outputFilePath, outputContent);

            Console.WriteLine($"Output written to {outputFilePath}");
        }

        static string ProcessTemplate(string templateContent, string xmlFilePath)
        {
            var host = new CustomTextTemplatingHost();
            host.TemplateFile = templateContent;
            host.Session["XmlFilePath"] = xmlFilePath;

            var engine = new Engine();
            string output = engine.ProcessTemplate(templateContent, host);

            if (host.Errors.HasErrors)
            {
                foreach (var error in host.Errors)
                {
                    Console.WriteLine(error);
                }
                throw new Exception("Template processing failed.");
            }

            return output;
        }
    }

    public class CustomTextTemplatingHost : ITextTemplatingEngineHost
    {
        public string TemplateFile { get; set; }
        public IDictionary<string, object> Session { get; } = new Dictionary<string, object>();

        public string ResolvePath(string path)
        {
            return Path.GetFullPath(path);
        }

        // Implement other ITextTemplatingEngineHost members as needed
        // ...
    }
}