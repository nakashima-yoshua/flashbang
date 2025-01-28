using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MyDotNetConsoleApp
{
    /// <summary>
    /// テンプレート処理を行うクラス
    /// </summary>
    class Program
    {
        /// <summary>
        /// メインメソッド
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Program.exe <TemplateFilePath> <XmlFilePath>");
                return;
            }

            string templateFilePath = args[0];
            string xmlFilePath = args[1];

            // テンプレートファイルを読み込む
            string templateContent = File.ReadAllText(templateFilePath);

            // XMLファイルを読み込む
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var xmlData = new Dictionary<string, object>();
            foreach (var element in xmlDoc.Root.Elements())
            {
                if (element.HasElements)
                {
                    var subElements = new Dictionary<string, string>();
                    foreach (var subElement in element.Elements())
                    {
                        subElements[subElement.Name.LocalName] = subElement.Value;
                    }
                    xmlData[element.Name.LocalName] = subElements;
                }
                else
                {
                    xmlData[element.Name.LocalName] = element.Value;
                }
            }

            // Handlebars テンプレートをコンパイル
            var template = Handlebars.Compile(templateContent);

            // テンプレートにデータを適用
            string result = template(xmlData);

            // 結果を txt ファイルに出力
            string outputFilePath = Path.ChangeExtension(templateFilePath, ".txt");
            File.WriteAllText(outputFilePath, result);

            Console.WriteLine($"Output written to {outputFilePath}");

            // 生成されたコードがC#かを判定
            bool isCSharpCode = IsCSharpCode(result);
            Console.WriteLine($"Is the generated code C#? {isCSharpCode}");
        }

        /// <summary>
        /// 生成されたコードがC#かを判定します。
        /// </summary>
        /// <param name="code">判定するコード文字列。</param>
        /// <returns>コードがC#の場合は true、それ以外の場合は false。</returns>
        static bool IsCSharpCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = syntaxTree.GetDiagnostics();
            return !diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }
    }
}
