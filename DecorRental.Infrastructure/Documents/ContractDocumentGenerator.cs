using System.Globalization;
using System.IO.Compression;
using System.Security;
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
        var templateFilePath = ResolveTemplateFilePath();
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

        var replacements = BuildEscapedTemplateReplacements(contractData);
        ReplaceTemplateTokens(stream, replacements);

        return stream.ToArray();
    }

    private static byte[] GenerateDefaultDocx(ContractData contractData)
    {
        using var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainDocumentPart = wordDocument.AddMainDocumentPart();
            mainDocumentPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();

            var body = new Body();
            foreach (var paragraphLine in BuildContractParagraphLines(contractData))
            {
                body.AppendChild(CreateParagraph(paragraphLine.Text, paragraphLine.IsBold, paragraphLine.FontSizeHalfPoints));
            }

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

                    page.Content().Column(column =>
                    {
                        column.Spacing(4);

                        foreach (var paragraphLine in BuildContractParagraphLines(contractData))
                        {
                            if (string.IsNullOrWhiteSpace(paragraphLine.Text))
                            {
                                column.Item().PaddingTop(4);
                                continue;
                            }

                            var textDescriptor = column.Item().Text(paragraphLine.Text);
                            if (paragraphLine.IsBold)
                            {
                                textDescriptor.SemiBold();
                            }

                            textDescriptor.FontSize(paragraphLine.FontSizePoints);
                        }
                    });
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
        var formattedStartDate = FormatDate(contractData.ReservationStartDate);
        var formattedEndDate = FormatDate(contractData.ReservationEndDate);
        var formattedContractDate = FormatDate(contractData.ContractDate);
        var formattedTotalAmount = FormatCurrency(contractData.TotalAmount);
        var formattedEntryAmount = FormatCurrency(contractData.EntryAmount);
        var yesNoBalloonArch = ToYesNo(contractData.HasBalloonArch);
        var yesNoEntryPaid = ToYesNo(contractData.IsEntryPaid);

        var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["{{KIT_THEME_NAME}}"] = contractData.KitThemeName,
            ["{{KIT_CATEGORY_NAME}}"] = contractData.KitCategoryName,
            ["{{RESERVATION_START_DATE}}"] = formattedStartDate,
            ["{{RESERVATION_END_DATE}}"] = formattedEndDate,
            ["{{CUSTOMER_NAME}}"] = contractData.CustomerName,
            ["{{CUSTOMER_DOCUMENT_NUMBER}}"] = contractData.CustomerDocumentNumber,
            ["{{CUSTOMER_PHONE_NUMBER}}"] = contractData.CustomerPhoneNumber,
            ["{{CUSTOMER_ADDRESS}}"] = contractData.CustomerAddress,
            ["{{CUSTOMER_ADDRESS_LINE}}"] = contractData.CustomerAddress,
            ["{{CUSTOMER_NEIGHBORHOOD}}"] = contractData.CustomerNeighborhood ?? "Não informado",
            ["{{CUSTOMER_CITY}}"] = contractData.CustomerCity ?? "Não informado",
            ["{{NOTES}}"] = contractData.Notes ?? string.Empty,
            ["{{HAS_BALLOON_ARCH}}"] = yesNoBalloonArch,
            ["{{IS_ENTRY_PAID}}"] = yesNoEntryPaid,
            ["{{CONTRACT_DATE}}"] = formattedContractDate,
            ["{{TOTAL_AMOUNT}}"] = formattedTotalAmount,
            ["{{ENTRY_AMOUNT}}"] = formattedEntryAmount,
            ["{{NOME_CLIENTE}}"] = contractData.CustomerName,
            ["{{DOCUMENTO_CLIENTE}}"] = contractData.CustomerDocumentNumber,
            ["{{TELEFONE_CLIENTE}}"] = contractData.CustomerPhoneNumber,
            ["{{ENDERECO_CLIENTE}}"] = contractData.CustomerAddress,
            ["{{DATA_RETIRADA}}"] = formattedStartDate,
            ["{{DATA_DEVOLUCAO}}"] = formattedEndDate,
            ["{{TEMA_KIT}}"] = contractData.KitThemeName,
            ["{{CATEGORIA_KIT}}"] = contractData.KitCategoryName,
            ["{{ARCO_BALOES}}"] = yesNoBalloonArch,
            ["{{ENTRADA_PAGA}}"] = yesNoEntryPaid,

            // Compatibilidade com template preenchido manualmente
            ["Bruna Nagase Comenali"] = contractData.CustomerName,
            ["40825890829"] = contractData.CustomerDocumentNumber,
            ["Av Pedro Friggi, 3100"] = contractData.CustomerAddress,
            ["Vista Verde"] = contractData.CustomerNeighborhood ?? "Não informado",
            ["Sao Jose dos Campos"] = contractData.CustomerCity ?? "Não informado",
            ["São José dos Campos"] = contractData.CustomerCity ?? "Não informado",
            ["20/03/2026"] = formattedStartDate,
            ["23/03/2026"] = formattedEndDate,
            ["Safari"] = contractData.KitThemeName,
            ["219,90"] = formattedTotalAmount,
            ["109,95"] = formattedEntryAmount,
            ["28/01/2026"] = formattedContractDate
        };

        return replacements;
    }

    private static void ReplaceTemplateTokens(Stream docxStream, IReadOnlyDictionary<string, string> replacements)
    {
        docxStream.Position = 0;
        using var archive = new ZipArchive(docxStream, ZipArchiveMode.Update, leaveOpen: true);

        var xmlEntryNames = archive.Entries
            .Where(entry => entry.FullName.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase)
                            || (entry.FullName.StartsWith("word/header", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            || (entry.FullName.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            .Select(entry => entry.FullName)
            .ToList();

        foreach (var xmlEntryName in xmlEntryNames)
        {
            var xmlEntry = archive.GetEntry(xmlEntryName);
            if (xmlEntry is null)
            {
                continue;
            }

            string xmlContent;
            using (var sourceStream = xmlEntry.Open())
            using (var streamReader = new StreamReader(sourceStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                xmlContent = streamReader.ReadToEnd();
            }

            var replacedXmlContent = ApplyReplacements(xmlContent, replacements);
            if (string.Equals(xmlContent, replacedXmlContent, StringComparison.Ordinal))
            {
                continue;
            }

            xmlEntry.Delete();
            var newXmlEntry = archive.CreateEntry(xmlEntryName, CompressionLevel.Optimal);
            using var targetStream = newXmlEntry.Open();
            using var streamWriter = new StreamWriter(targetStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            streamWriter.Write(replacedXmlContent);
        }

        docxStream.Position = 0;
    }

    private static string ApplyReplacements(string sourceText, IReadOnlyDictionary<string, string> replacements)
    {
        var targetText = sourceText;
        foreach (var replacement in replacements)
        {
            targetText = targetText.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return targetText;
    }

    private static Dictionary<string, string> BuildEscapedTemplateReplacements(ContractData contractData)
    {
        return BuildTemplateReplacements(contractData).ToDictionary(
            pair => pair.Key,
            pair => SecurityElement.Escape(pair.Value) ?? string.Empty,
            StringComparer.Ordinal);
    }

    private static List<ContractParagraphLine> BuildContractParagraphLines(ContractData contractData)
    {
        var lines = new List<ContractParagraphLine>
        {
            new("CONTRATO DE LOCAÇÃO DE DECORAÇÃO", true, "32", 16),
            new(string.Empty),
            new("CLIENTE:", true),
            new($"Nome: {contractData.CustomerName}"),
            new($"CPF: {contractData.CustomerDocumentNumber}"),
            new($"Endereço: {contractData.CustomerAddress}"),
            new($"Bairro: {contractData.CustomerNeighborhood ?? "Não informado"}"),
            new($"Cidade: {contractData.CustomerCity ?? "Não informado"}"),
            new($"Data da retirada: {FormatDate(contractData.ReservationStartDate)}"),
            new("CLÁUSULAS", true)
        };

        lines.AddRange(BuildClauseLines(contractData.ReservationEndDate).Select(clause => new ContractParagraphLine(clause)));

        lines.AddRange(
        [
            new("DECORAÇÃO ENTREGUE:", true),
            new(contractData.KitThemeName),
            new($"VALOR TOTAL: R$ {FormatCurrency(contractData.TotalAmount)}"),
            new($"VALOR DE ENTRADA: R$ {FormatCurrency(contractData.EntryAmount)}"),
            new("*É proibido o uso de vela faísca no bolo.*"),
            new("Em caso de não devolução ou dano, será cobrado o valor vigente das peças."),
            new("ASSINATURAS", true),
            new("Cliente : ___________________________"),
            new("Empresa : ___________________________"),
            new($"Data: {FormatDate(contractData.ContractDate)}")
        ]);

        return lines;
    }

    private static IReadOnlyList<string> BuildClauseLines(DateOnly reservationEndDate)
    {
        return
        [
            "1. Não colocar cola quente, fita adesiva, grampo ou pregos na decoração.",
            "2. Não furar o bolo fake para utilização de velas ou usar velas faísca no bolo.",
            "3. Taxa de limpeza será cobrada se o material for entregue sujo (R$ 20,00 por item).",
            $"4. A devolução da decoração deverá ocorrer até {FormatDate(reservationEndDate)} sob multa de R$ 100,00 por dia de atraso.",
            "5. O transporte é de responsabilidade do cliente.",
            "6. A reserva será confirmada mediante pagamento de 50% do valor.",
            "7. Cancelamentos não geram reembolso, podendo remarcar em até 6 meses.",
            "8. Não há troca de data com menos de 30 dias de antecedência."
        ];
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
        => value ? "Sim" : "Não";

    private static string FormatCurrency(decimal? value)
        => value.HasValue ? value.Value.ToString("N2", PtBrCulture) : "0,00";

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var filtered = normalized
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(filtered).Normalize(NormalizationForm.FormC);
    }

    private string ResolveTemplateFilePath()
    {
        if (string.IsNullOrWhiteSpace(_templateOptions.FilePath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(_templateOptions.FilePath))
        {
            return _templateOptions.FilePath;
        }

        var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, _templateOptions.FilePath);
        if (File.Exists(baseDirectoryPath))
        {
            return baseDirectoryPath;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), _templateOptions.FilePath);
    }

    private sealed record ContractParagraphLine(
        string Text,
        bool IsBold = false,
        string FontSizeHalfPoints = "24",
        int FontSizePoints = 12);
}
