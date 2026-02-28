namespace DecorRental.Application.Contracts;

public interface IContractDocumentGenerator
{
    Task<GeneratedContractFile> GenerateAsync(
        ContractData contractData,
        ContractDocumentFormat format,
        CancellationToken cancellationToken = default);
}
