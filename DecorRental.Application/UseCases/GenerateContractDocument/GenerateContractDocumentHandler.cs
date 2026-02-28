using DecorRental.Application.Contracts;

namespace DecorRental.Application.UseCases.GenerateContractDocument;

public sealed class GenerateContractDocumentHandler
{
    private readonly IContractDocumentGenerator _contractDocumentGenerator;

    public GenerateContractDocumentHandler(IContractDocumentGenerator contractDocumentGenerator)
    {
        _contractDocumentGenerator = contractDocumentGenerator;
    }

    public Task<GeneratedContractFile> HandleAsync(
        GenerateContractDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        return _contractDocumentGenerator.GenerateAsync(command.ContractData, command.Format, cancellationToken);
    }
}
