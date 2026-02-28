using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.Contracts;
using DecorRental.Application.UseCases.GenerateContractDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize(Policy = AuthorizationPolicies.ReadKits)]
public sealed class ContractsController : ControllerBase
{
    private readonly GenerateContractDocumentHandler _generateContractDocumentHandler;

    public ContractsController(GenerateContractDocumentHandler generateContractDocumentHandler)
    {
        _generateContractDocumentHandler = generateContractDocumentHandler;
    }

    [HttpPost("generate")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Generate(
        [FromQuery] string format,
        [FromBody] ContractDataRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseFormat(format, out var documentFormat))
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Falha de validacao.",
                Detail = "Formato de contrato invalido. Use 'docx' ou 'pdf'.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        var contractData = new ContractData(
            request.KitThemeId,
            request.ReservationId,
            request.KitThemeName,
            request.KitCategoryName,
            request.ReservationStartDate,
            request.ReservationEndDate,
            request.CustomerName,
            request.CustomerDocumentNumber,
            request.CustomerPhoneNumber,
            request.CustomerAddress,
            request.Notes,
            request.HasBalloonArch,
            request.IsEntryPaid,
            request.ContractDate);

        var command = new GenerateContractDocumentCommand(contractData, documentFormat);
        var file = await _generateContractDocumentHandler.HandleAsync(command, cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    private static bool TryParseFormat(string format, out ContractDocumentFormat documentFormat)
    {
        documentFormat = default;
        if (string.IsNullOrWhiteSpace(format))
        {
            return false;
        }

        if (string.Equals(format, "docx", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(format, "word", StringComparison.OrdinalIgnoreCase))
        {
            documentFormat = ContractDocumentFormat.Docx;
            return true;
        }

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            documentFormat = ContractDocumentFormat.Pdf;
            return true;
        }

        return false;
    }
}
