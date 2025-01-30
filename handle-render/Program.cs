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
    /// �e���v���[�g�������s���N���X
    /// </summary>
    class Program
    {
        /// <summary>
        /// ���C�����\�b�h
        /// </summary>
        /// <param name="args">�R�}���h���C������</param>
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

            // ���X�g�t�@�C����ǂݍ���ŏ���
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
            // �������ꂽ�R�[�h��C#���𔻒�
            bool isCSharpCode = IsCSharpCode(result);
            Console.WriteLine($"Is the generated code C#? {isCSharpCode}");

            string extension = isCSharpCode ? ".cs" : ".txt";

            // ���ʂ� txt �t�@�C���ɏo��
            string outputFilePath = Path.ChangeExtension(templateFilePath, extension);
            File.WriteAllText(outputFilePath, result);
            Console.WriteLine($"Output written to {outputFilePath}");

            return outputFilePath;
        }

        /// <summary>
        /// �e���v���[�g���R���p�C�����Ď��s����
        /// </summary>
        /// <param name="args"></param>
        /// <param name="templateFilePath"></param>
        /// <returns></returns>
        private static string CompileTemplate(string templateFilePath, string xmlFilePath)
        {
            // �e���v���[�g�t�@�C����ǂݍ���
            string templateContent = File.ReadAllText(templateFilePath);

            // XML�t�@�C����ǂݍ���
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var xmlData = ParseElement(xmlDoc.Root);

            // Handlebars �e���v���[�g���R���p�C��
            var template = Handlebars.Compile(templateContent);

            // �e���v���[�g�Ƀf�[�^��K�p
            string result = template(xmlData);
            Console.Write(result);

            return result;
        }

        /// <summary>
        /// �������ꂽ�R�[�h��C#���𔻒肵�܂��B
        /// </summary>
        /// <param name="code">���肷��R�[�h������B</param>
        /// <returns>�R�[�h��C#�̏ꍇ�� true�A����ȊO�̏ꍇ�� false�B</returns>
        static bool IsCSharpCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = syntaxTree.GetDiagnostics();
            return !diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }


        /// <summary>
        /// XML�v�f���ċA�I�ɉ�͂��A�����Ɋi�[����
        /// </summary>
        /// <param name="element">XML�v�f</param>
        /// <returns>�����`���̃f�[�^</returns>
        static Dictionary<string, object> ParseElement(XElement element)
        {
            var result = new Dictionary<string, object>();

            // �q�v�f���O���[�v���i�������O�̗v�f���������邩�`�F�b�N�j
            var groupedElements = element.Elements().GroupBy(e => e.Name.LocalName);

            foreach (var group in groupedElements)
            {
                if (group.Count() > 1)
                {
                    // �����^�O���̗v�f����������ꍇ�i���X�g�Ƃ��Ĉ����j
                    var list = new List<object>();
                    foreach (var item in group)
                    {
                        if (item.HasElements)
                        {
                            // �q�v�f�����ꍇ�͎����Ƃ��Ċi�[
                            list.Add(ParseElement(item));
                        }
                        else
                        {
                            // �P���Ȓl�̏ꍇ�͕�����Ƃ��Ċi�[
                            list.Add(item.Value);
                        }
                    }
                    result[group.Key] = list;
                }
                else
                {
                    // �P��̗v�f
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
        /// �������ċA�I�ɏo�͂���i�f�o�b�O�p�j
        /// </summary>
        /// <param name="dict">�o�͂��鎫��</param>
        /// <param name="indent">�C���f���g�̐[��</param>
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
