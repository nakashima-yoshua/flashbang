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

namespace handleRender
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
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Program.exe <BatchListFilePath>");
                return;
            }

            string batchListFilePath = args[0];

            if (!File.Exists(batchListFilePath))
            {
                Console.WriteLine($"Error: File not found - {batchListFilePath}");
                return;
            }

            // リストファイルを読み込んで処理
            foreach (var line in File.ReadLines(batchListFilePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 2)
                {
                    Console.WriteLine($"Skipping invalid line: {line}");
                    continue;
                }

                string templateFilePath = parts[0].Trim();
                string xmlFilePath = parts[1].Trim();

                if (!File.Exists(templateFilePath) || !File.Exists(xmlFilePath))
                {
                    Console.WriteLine($"Skipping: File not found - {templateFilePath} or {xmlFilePath}");
                    continue;
                }

                try
                {
                    Console.WriteLine("--- Compile ---");
                    string result = CompileTemplate(templateFilePath, xmlFilePath);

                    Console.WriteLine("--- Export  ---");
                    string outputFilePath = ExportFile(xmlFilePath, result);

                    Console.WriteLine("--- End     ---");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {templateFilePath} and {xmlFilePath}: {ex.Message}");
                }
            }
        }

        private static string ExportFile(string templateFilePath, string result)
        {
            // 生成されたコードがC#かを判定
            bool isCSharpCode = IsCSharpCode(result);
            Console.WriteLine($"Is the generated code C#? {isCSharpCode}");

            string extension = isCSharpCode ? ".cs" : ".txt";

            // 結果を txt ファイルに出力
            string outputFilePath = Path.ChangeExtension(templateFilePath, extension);
            File.WriteAllText(outputFilePath, result);
            Console.WriteLine($"Output written to {outputFilePath}");

            return outputFilePath;
        }

        /// <summary>
        /// テンプレートをコンパイルして実行する
        /// </summary>
        /// <param name="args"></param>
        /// <param name="templateFilePath"></param>
        /// <returns></returns>
        private static string CompileTemplate(string templateFilePath, string xmlFilePath)
        {
            // テンプレートファイルを読み込む
            string templateContent = File.ReadAllText(templateFilePath);

            // XMLファイルを読み込む
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var xmlData = ParseElement(xmlDoc.Root);

            // Handlebars テンプレートをコンパイル
            var template = Handlebars.Compile(templateContent);

            // テンプレートにデータを適用
            string result = template(xmlData);
            Console.Write(result);

            return result;
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


        /// <summary>
        /// XML要素を再帰的に解析し、辞書に格納する
        /// </summary>
        /// <param name="element">XML要素</param>
        /// <returns>辞書形式のデータ</returns>
        static Dictionary<string, object> ParseElement(XElement element)
        {
            var result = new Dictionary<string, object>();

            // 子要素をグループ化（同じ名前の要素が複数あるかチェック）
            var groupedElements = element.Elements().GroupBy(e => e.Name.LocalName);

            foreach (var group in groupedElements)
            {
                if (group.Count() > 1)
                {
                    // 同じタグ名の要素が複数ある場合（リストとして扱う）
                    var list = new List<object>();
                    foreach (var item in group)
                    {
                        if (item.HasElements)
                        {
                            // 子要素を持つ場合は辞書として格納
                            list.Add(ParseElement(item));
                        }
                        else
                        {
                            // 単純な値の場合は文字列として格納
                            list.Add(item.Value);
                        }
                    }
                    result[group.Key] = list;
                }
                else
                {
                    // 単一の要素
                    var child = group.First();
                    if (child.HasElements)
                    {
                        result[child.Name.LocalName] = ParseElement(child);
                    }
                    else
                    {
                        result[child.Name.LocalName] = child.Value;
                    }
                }
            }

            return result;
        }

        class ObjectCollection : List<object>
        {
            public override string ToString()
            {

                return base.ToString();
            }
        }

        /// <summary>
        /// 辞書を再帰的に出力する（デバッグ用）
        /// </summary>
        /// <param name="dict">出力する辞書</param>
        /// <param name="indent">インデントの深さ</param>
        static void PrintDictionary(Dictionary<string, object> dict, int indent)
        {
            foreach (var kvp in dict)
            {
                Console.Write(new string(' ', indent * 2));
                Console.Write($"{kvp.Key}: ");

                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    Console.WriteLine();
                    PrintDictionary(nestedDict, indent + 1);
                }
                else if (kvp.Value is List<object> list)
                {
                    Console.WriteLine();
                    foreach (var item in list)
                    {
                        Console.Write(new string(' ', (indent + 1) * 2));
                        if (item is Dictionary<string, object> itemDict)
                        {
                            Console.WriteLine();
                            PrintDictionary(itemDict, indent + 2);
                        }
                        else
                        {
                            Console.WriteLine(item);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(kvp.Value);
                }
            }
        }
    }
}
