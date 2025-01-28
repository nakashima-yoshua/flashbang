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
        // �������R�[�h�i�K�v�ɉ����āj
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // �e���v���[�g�t�@�C����XML�t�@�C���̃p�X���擾
        var templateFilePath = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TemplateFilePath", out var templatePath) ? templatePath : null;
        var xmlFilePath = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.XmlFilePath", out var xmlPath) ? xmlPath : null;

        if (templateFilePath == null || xmlFilePath == null)
        {
            return;
        }

        // �e���v���[�g��XML�̓��e��ǂݍ���
        var templateContent = File.ReadAllText(templateFilePath);
        var xmlContent = File.ReadAllText(xmlFilePath);

        // �e���v���[�g�����i�ȒP�Ȓu�������̗�j
        var outputContent = templateContent.Replace("{{XmlContent}}", xmlContent);

        // �o�̓t�@�C���̃p�X������
        var outputFilePath = Path.ChangeExtension(templateFilePath, ".txt");

        // �o�̓t�@�C���ɏ�������
        File.WriteAllText(outputFilePath, outputContent);

        // �o�̓t�@�C�����\�[�X�Ƃ��Ēǉ�
        context.AddSource("GeneratedOutput", SourceText.From(outputContent, Encoding.UTF8));
    }
}
