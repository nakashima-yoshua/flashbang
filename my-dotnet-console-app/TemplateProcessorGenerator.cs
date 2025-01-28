using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;

[Generator]
public class TemplateProcessorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // 初期化コード（必要に応じて）
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // AdditionalText からテンプレートファイルとXMLファイルのパスを取得
        var templateFile = context.AdditionalFiles[0];
        var xmlFile = context.AdditionalFiles[1];

        var templateFilePath = templateFile.Path;
        var xmlFilePath = xmlFile.Path;

        // テンプレートとXMLの内容を読み込む
        var templateContent = File.ReadAllText(templateFilePath);
        var xmlContent = File.ReadAllText(xmlFilePath);

        // テンプレート処理（簡単な置換処理の例）
        var outputContent = templateContent.Replace("{{XmlContent}}", xmlContent);

        // 出力ファイルのパスを決定
        var outputFilePath = Path.ChangeExtension(templateFilePath, ".txt");

        // 出力ファイルに書き込む
        File.WriteAllText(outputFilePath, outputContent);

        // 出力ファイルをソースとして追加
        context.AddSource("GeneratedOutput", SourceText.From(outputContent, Encoding.UTF8));
    }
}
