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
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Program.exe <TemplateFilePath> <XmlFilePath>");
                return;
            }

            string templateFilePath = args[0];
            string xmlFilePath = args[1];

            // �e���v���[�g�t�@�C����ǂݍ���
            string templateContent = File.ReadAllText(templateFilePath);

            // XML�t�@�C����ǂݍ���
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

            // Handlebars �e���v���[�g���R���p�C��
            var template = Handlebars.Compile(templateContent);

            // �e���v���[�g�Ƀf�[�^��K�p
            string result = template(xmlData);

            // ���ʂ� txt �t�@�C���ɏo��
            string outputFilePath = Path.ChangeExtension(templateFilePath, ".txt");
            File.WriteAllText(outputFilePath, result);

            Console.WriteLine($"Output written to {outputFilePath}");

            // �������ꂽ�R�[�h��C#���𔻒�
            bool isCSharpCode = IsCSharpCode(result);
            Console.WriteLine($"Is the generated code C#? {isCSharpCode}");
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
    }
}
