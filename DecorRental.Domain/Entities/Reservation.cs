using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities;

public class Reservation
{
    private const int CustomerNameMaxLength = 120;
    private const int CustomerDocumentNumberMaxLength = 40;
    private const int CustomerPhoneNumberMaxLength = 30;
    private const int CustomerAddressMaxLength = 250;
    private const int NotesMaxLength = 500;

    private readonly List<ReservationItem> _items = new();

    public Guid Id { get; private set; }
    public Guid KitThemeId { get; private set; }
    public Guid KitCategoryId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public bool IsStockOverride { get; private set; }
    public string? StockOverrideReason { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public string CustomerDocumentNumber { get; private set; } = null!;
    public string CustomerPhoneNumber { get; private set; } = null!;
    public string CustomerAddress { get; private set; } = null!;
    public string? Notes { get; private set; }
    public bool HasBalloonArch { get; private set; }
    public bool IsEntryPaid { get; private set; }
    public IReadOnlyCollection<ReservationItem> Items => _items;

    public static Reservation Create(
        Guid kitThemeId,
        KitCategory category,
        DateRange period,
        bool isStockOverride,
        string? stockOverrideReason,
        string customerName,
        string customerDocumentNumber,
        string customerPhoneNumber,
        string customerAddress,
        string? notes,
        bool hasBalloonArch,
        bool isEntryPaid)
    {
        if (category is null)
        {
            throw new DomainException("Categoria e obrigatoria.");
        }

        if (category.Items.Count == 0)
        {
            throw new DomainException("Category must have at least one item.");
        }

        var reservationId = Guid.NewGuid();
        var reservationItems = category.Items
            .Select(categoryItem => new ReservationItem(reservationId, categoryItem.ItemTypeId, categoryItem.Quantity))
            .ToList();

        return new Reservation(
            reservationId,
            kitThemeId,
            category.Id,
            period,
            reservationItems,
            isStockOverride,
            stockOverrideReason,
            customerName,
            customerDocumentNumber,
            customerPhoneNumber,
            customerAddress,
            notes,
            hasBalloonArch,
            isEntryPaid);
    }

    private Reservation(
        Guid reservationId,
        Guid kitThemeId,
        Guid kitCategoryId,
        DateRange period,
        IReadOnlyCollection<ReservationItem> items,
        bool isStockOverride,
        string? stockOverrideReason,
        string customerName,
        string customerDocumentNumber,
        string customerPhoneNumber,
        string customerAddress,
        string? notes,
        bool hasBalloonArch,
        bool isEntryPaid)
    {
        if (items.Count == 0)
        {
            throw new DomainException("Reservation must contain at least one item.");
        }

        if (isStockOverride && string.IsNullOrWhiteSpace(stockOverrideReason))
        {
            throw new DomainException("O motivo do override de estoque e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("O nome do cliente e obrigatorio.");
        }

        if (customerName.Length > CustomerNameMaxLength)
        {
            throw new DomainException($"O nome do cliente deve ter no maximo {CustomerNameMaxLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(customerDocumentNumber))
        {
            throw new DomainException("O documento do cliente e obrigatorio.");
        }

        if (customerDocumentNumber.Length > CustomerDocumentNumberMaxLength)
        {
            throw new DomainException($"O documento do cliente deve ter no maximo {CustomerDocumentNumberMaxLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(customerPhoneNumber))
        {
            throw new DomainException("O telefone do cliente e obrigatorio.");
        }

        if (customerPhoneNumber.Length > CustomerPhoneNumberMaxLength)
        {
            throw new DomainException($"O telefone do cliente deve ter no maximo {CustomerPhoneNumberMaxLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(customerAddress))
        {
            throw new DomainException("O endereco do cliente e obrigatorio.");
        }

        if (customerAddress.Length > CustomerAddressMaxLength)
        {
            throw new DomainException($"O endereco do cliente deve ter no maximo {CustomerAddressMaxLength} caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > NotesMaxLength)
        {
            throw new DomainException($"As observacoes devem ter no maximo {NotesMaxLength} caracteres.");
        }

        Id = reservationId;
        KitThemeId = kitThemeId;
        KitCategoryId = kitCategoryId;
        Period = period;
        Status = ReservationStatus.Active;
        IsStockOverride = isStockOverride;
        StockOverrideReason = isStockOverride ? stockOverrideReason!.Trim() : null;
        CustomerName = customerName.Trim();
        CustomerDocumentNumber = customerDocumentNumber.Trim();
        CustomerPhoneNumber = customerPhoneNumber.Trim();
        CustomerAddress = customerAddress.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        HasBalloonArch = hasBalloonArch;
        IsEntryPaid = isEntryPaid;

        _items.AddRange(items);
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return;
        }

        Status = ReservationStatus.Cancelled;
    }

    private Reservation() { }
}
