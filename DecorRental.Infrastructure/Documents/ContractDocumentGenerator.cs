using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DecorRental.Application.Contracts;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace DecorRental.Infrastructure.Documents;

public sealed class ContractDocumentGenerator : IContractDocumentGenerator
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private const string PdfContentType = "application/pdf";

    private static readonly CultureInfo PtBrCulture = new("pt-BR");

    private readonly ContractTemplateOptions _templateOptions;

    public ContractDocumentGenerator(IOptions<ContractTemplateOptions> templateOptions)
    {
        _templateOptions = templateOptions.Value;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<GeneratedContractFile> GenerateAsync(
        ContractData contractData,
        ContractDocumentFormat format,
        CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ContractDocumentFormat.Docx => new GeneratedContractFile(
                await GenerateDocxAsync(contractData, cancellationToken),
                DocxContentType,
                $"{BuildFileNamePrefix(contractData)}.docx"),
            ContractDocumentFormat.Pdf => new GeneratedContractFile(
                GeneratePdf(contractData),
                PdfContentType,
                $"{BuildFileNamePrefix(contractData)}.pdf"),
            _ => throw new InvalidOperationException("Formato de contrato nao suportado.")
        };
    }

    private async Task<byte[]> GenerateDocxAsync(ContractData contractData, CancellationToken cancellationToken)
    {
        var templateFilePath = _templateOptions.FilePath;
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return await GenerateDocxFromTemplateAsync(contractData, templateFilePath, cancellationToken);
        }

        return GenerateDefaultDocx(contractData);
    }

    private static async Task<byte[]> GenerateDocxFromTemplateAsync(
        ContractData contractData,
        string templateFilePath,
        CancellationToken cancellationToken)
    {
        var templateBytes = await File.ReadAllBytesAsync(templateFilePath, cancellationToken);
        await using var stream = new MemoryStream();
        await stream.WriteAsync(templateBytes, cancellationToken);
        stream.Position = 0;

        using var wordDocument = WordprocessingDocument.Open(stream, true);
        var mainDocumentPart = wordDocument.MainDocumentPart
            ?? throw new InvalidOperationException("Template de contrato invalido.");

        var replacements = BuildTemplateReplacements(contractData);
        ReplaceTemplateTokens(mainDocumentPart, replacements);
        mainDocumentPart.Document.Save();

        return stream.ToArray();
    }

    private static byte[] GenerateDefaultDocx(ContractData contractData)
    {
        using var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainDocumentPart = wordDocument.AddMainDocumentPart();
            mainDocumentPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();

            var body = new Body(
                CreateParagraph("CONTRATO DE LOCACAO DE DECORACAO", isBold: true, fontSizeHalfPoints: "32"),
                CreateParagraph(string.Empty),
                CreateParagraph($"Cliente: {contractData.CustomerName}"),
                CreateParagraph($"Documento: {contractData.CustomerDocumentNumber}"),
                CreateParagraph($"Telefone: {contractData.CustomerPhoneNumber}"),
                CreateParagraph($"Endereco: {contractData.CustomerAddress}"),
                CreateParagraph($"Tema: {contractData.KitThemeName}"),
                CreateParagraph($"Categoria: {contractData.KitCategoryName}"),
                CreateParagraph($"Periodo: {FormatDate(contractData.ReservationStartDate)} ate {FormatDate(contractData.ReservationEndDate)}"),
                CreateParagraph($"Arco de baloes: {ToYesNo(contractData.HasBalloonArch)}"),
                CreateParagraph($"Entrada paga: {ToYesNo(contractData.IsEntryPaid)}"),
                CreateParagraph($"Observacoes: {contractData.Notes ?? "Nao informado."}"),
                CreateParagraph(string.Empty),
                CreateParagraph("Assinaturas", isBold: true),
                CreateParagraph("Cliente: ___________________________"),
                CreateParagraph("Empresa: ___________________________"),
                CreateParagraph($"Data: {FormatDate(contractData.ContractDate)}"));

            mainDocumentPart.Document.AppendChild(body);
            mainDocumentPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static byte[] GeneratePdf(ContractData contractData)
    {
        return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(28);

                    page.Header()
                        .Text("Contrato de Locacao de Decoracao")
                        .SemiBold()
                        .FontSize(20);

                    page.Content().Column(column =>
                    {
                        column.Spacing(6);
                        column.Item().Text($"Cliente: {contractData.CustomerName}");
                        column.Item().Text($"Documento: {contractData.CustomerDocumentNumber}");
                        column.Item().Text($"Telefone: {contractData.CustomerPhoneNumber}");
                        column.Item().Text($"Endereco: {contractData.CustomerAddress}");
                        column.Item().Text($"Tema: {contractData.KitThemeName}");
                        column.Item().Text($"Categoria: {contractData.KitCategoryName}");
                        column.Item().Text($"Periodo: {FormatDate(contractData.ReservationStartDate)} ate {FormatDate(contractData.ReservationEndDate)}");
                        column.Item().Text($"Arco de baloes: {ToYesNo(contractData.HasBalloonArch)}");
                        column.Item().Text($"Entrada paga: {ToYesNo(contractData.IsEntryPaid)}");
                        column.Item().Text($"Observacoes: {contractData.Notes ?? "Nao informado."}");

                        column.Item().PaddingTop(16).Text("Assinaturas").SemiBold();
                        column.Item().Text("Cliente: ___________________________");
                        column.Item().Text("Empresa: ___________________________");
                        column.Item().Text($"Data: {FormatDate(contractData.ContractDate)}");
                    });

                    page.Footer()
                        .AlignRight()
                        .Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10);
                });
            })
            .GeneratePdf();
    }

    private static string BuildFileNamePrefix(ContractData contractData)
    {
        var normalizedCustomerName = RemoveDiacritics(contractData.CustomerName);
        var slug = Regex.Replace(normalizedCustomerName, "[^a-zA-Z0-9]+", "-")
            .Trim('-')
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "cliente";
        }

        return $"contrato-{slug}-{contractData.ReservationStartDate:yyyyMMdd}";
    }

    private static Dictionary<string, string> BuildTemplateReplacements(ContractData contractData)
    {
        var yesNoBalloonArch = ToYesNo(contractData.HasBalloonArch);
        var yesNoEntryPaid = ToYesNo(contractData.IsEntryPaid);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["{{KIT_THEME_NAME}}"] = contractData.KitThemeName,
            ["{{KIT_CATEGORY_NAME}}"] = contractData.KitCategoryName,
            ["{{RESERVATION_START_DATE}}"] = FormatDate(contractData.ReservationStartDate),
            ["{{RESERVATION_END_DATE}}"] = FormatDate(contractData.ReservationEndDate),
            ["{{CUSTOMER_NAME}}"] = contractData.CustomerName,
            ["{{CUSTOMER_DOCUMENT_NUMBER}}"] = contractData.CustomerDocumentNumber,
            ["{{CUSTOMER_PHONE_NUMBER}}"] = contractData.CustomerPhoneNumber,
            ["{{CUSTOMER_ADDRESS}}"] = contractData.CustomerAddress,
            ["{{NOTES}}"] = contractData.Notes ?? string.Empty,
            ["{{HAS_BALLOON_ARCH}}"] = yesNoBalloonArch,
            ["{{IS_ENTRY_PAID}}"] = yesNoEntryPaid,
            ["{{CONTRACT_DATE}}"] = FormatDate(contractData.ContractDate),
            ["{{NOME_CLIENTE}}"] = contractData.CustomerName,
            ["{{DOCUMENTO_CLIENTE}}"] = contractData.CustomerDocumentNumber,
            ["{{TELEFONE_CLIENTE}}"] = contractData.CustomerPhoneNumber,
            ["{{ENDERECO_CLIENTE}}"] = contractData.CustomerAddress,
            ["{{DATA_RETIRADA}}"] = FormatDate(contractData.ReservationStartDate),
            ["{{DATA_DEVOLUCAO}}"] = FormatDate(contractData.ReservationEndDate),
            ["{{TEMA_KIT}}"] = contractData.KitThemeName,
            ["{{CATEGORIA_KIT}}"] = contractData.KitCategoryName,
            ["{{ARCO_BALOES}}"] = yesNoBalloonArch,
            ["{{ENTRADA_PAGA}}"] = yesNoEntryPaid
        };
    }

    private static void ReplaceTemplateTokens(MainDocumentPart mainDocumentPart, IReadOnlyDictionary<string, string> replacements)
    {
        var textNodes = mainDocumentPart.Document.Descendants<Text>().ToList();

        foreach (var textNode in textNodes)
        {
            var currentText = textNode.Text;
            if (string.IsNullOrEmpty(currentText))
            {
                continue;
            }

            foreach (var replacement in replacements)
            {
                currentText = currentText.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
            }

            textNode.Text = currentText;
        }
    }

    private static Paragraph CreateParagraph(string text, bool isBold = false, string fontSizeHalfPoints = "24")
    {
        var runProperties = new RunProperties(new FontSize { Val = fontSizeHalfPoints });
        if (isBold)
        {
            runProperties.Append(new Bold());
        }

        var run = new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(run);
    }

    private static string FormatDate(DateOnly date)
        => date.ToString("dd/MM/yyyy", PtBrCulture);

    private static string ToYesNo(bool value)
        => value ? "Sim" : "Nao";

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var filtered = normalized
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(filtered).Normalize(NormalizationForm.FormC);
    }
}
