using DecorRental.Application.Contracts;

namespace DecorRental.Application.UseCases.GenerateContractDocument;

public sealed record GenerateContractDocumentCommand(
    ContractData ContractData,
    ContractDocumentFormat Format);
