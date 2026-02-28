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
        var reservationId = Guid.NewGuid();
        var reservationItems = BuildReservationItems(reservationId, category);

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

        var normalizedCustomerName = NormalizeAndValidateCustomerName(customerName);
        var normalizedDocumentNumber = NormalizeAndValidateDocumentNumber(customerDocumentNumber);
        var normalizedPhoneNumber = NormalizeAndValidatePhoneNumber(customerPhoneNumber);
        var normalizedAddress = NormalizeAndValidateAddress(customerAddress);
        var normalizedNotes = NormalizeAndValidateNotes(notes);
        var normalizedStockOverrideReason = NormalizeAndValidateStockOverrideReason(
            isStockOverride,
            stockOverrideReason);

        Id = reservationId;
        KitThemeId = kitThemeId;
        KitCategoryId = kitCategoryId;
        Period = period;
        Status = ReservationStatus.Active;
        IsStockOverride = isStockOverride;
        StockOverrideReason = normalizedStockOverrideReason;
        CustomerName = normalizedCustomerName;
        CustomerDocumentNumber = normalizedDocumentNumber;
        CustomerPhoneNumber = normalizedPhoneNumber;
        CustomerAddress = normalizedAddress;
        Notes = normalizedNotes;
        HasBalloonArch = hasBalloonArch;
        IsEntryPaid = isEntryPaid;

        _items.AddRange(items);
    }

    public void Update(
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
        if (Status == ReservationStatus.Cancelled)
        {
            throw new DomainException("Nao e possivel editar uma reserva cancelada.");
        }

        var updatedItems = BuildReservationItems(Id, category);
        var normalizedCustomerName = NormalizeAndValidateCustomerName(customerName);
        var normalizedDocumentNumber = NormalizeAndValidateDocumentNumber(customerDocumentNumber);
        var normalizedPhoneNumber = NormalizeAndValidatePhoneNumber(customerPhoneNumber);
        var normalizedAddress = NormalizeAndValidateAddress(customerAddress);
        var normalizedNotes = NormalizeAndValidateNotes(notes);
        var normalizedStockOverrideReason = NormalizeAndValidateStockOverrideReason(
            isStockOverride,
            stockOverrideReason);

        KitCategoryId = category.Id;
        Period = period;
        IsStockOverride = isStockOverride;
        StockOverrideReason = normalizedStockOverrideReason;
        CustomerName = normalizedCustomerName;
        CustomerDocumentNumber = normalizedDocumentNumber;
        CustomerPhoneNumber = normalizedPhoneNumber;
        CustomerAddress = normalizedAddress;
        Notes = normalizedNotes;
        HasBalloonArch = hasBalloonArch;
        IsEntryPaid = isEntryPaid;

        _items.Clear();
        _items.AddRange(updatedItems);
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return;
        }

        Status = ReservationStatus.Cancelled;
    }

    private static List<ReservationItem> BuildReservationItems(Guid reservationId, KitCategory category)
    {
        if (category is null)
        {
            throw new DomainException("Categoria e obrigatoria.");
        }

        if (category.Items.Count == 0)
        {
            throw new DomainException("Category must have at least one item.");
        }

        return category.Items
            .Select(categoryItem => new ReservationItem(reservationId, categoryItem.ItemTypeId, categoryItem.Quantity))
            .ToList();
    }

    private static string NormalizeAndValidateCustomerName(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("O nome do cliente e obrigatorio.");
        }

        var normalizedCustomerName = customerName.Trim();
        if (normalizedCustomerName.Length > CustomerNameMaxLength)
        {
            throw new DomainException($"O nome do cliente deve ter no maximo {CustomerNameMaxLength} caracteres.");
        }

        return normalizedCustomerName;
    }

    private static string NormalizeAndValidateDocumentNumber(string customerDocumentNumber)
    {
        if (string.IsNullOrWhiteSpace(customerDocumentNumber))
        {
            throw new DomainException("O documento do cliente e obrigatorio.");
        }

        var normalizedDocumentNumber = customerDocumentNumber.Trim();
        if (normalizedDocumentNumber.Length > CustomerDocumentNumberMaxLength)
        {
            throw new DomainException($"O documento do cliente deve ter no maximo {CustomerDocumentNumberMaxLength} caracteres.");
        }

        return normalizedDocumentNumber;
    }

    private static string NormalizeAndValidatePhoneNumber(string customerPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(customerPhoneNumber))
        {
            throw new DomainException("O telefone do cliente e obrigatorio.");
        }

        var normalizedPhoneNumber = customerPhoneNumber.Trim();
        if (normalizedPhoneNumber.Length > CustomerPhoneNumberMaxLength)
        {
            throw new DomainException($"O telefone do cliente deve ter no maximo {CustomerPhoneNumberMaxLength} caracteres.");
        }

        return normalizedPhoneNumber;
    }

    private static string NormalizeAndValidateAddress(string customerAddress)
    {
        if (string.IsNullOrWhiteSpace(customerAddress))
        {
            throw new DomainException("O endereco do cliente e obrigatorio.");
        }

        var normalizedAddress = customerAddress.Trim();
        if (normalizedAddress.Length > CustomerAddressMaxLength)
        {
            throw new DomainException($"O endereco do cliente deve ter no maximo {CustomerAddressMaxLength} caracteres.");
        }

        return normalizedAddress;
    }

    private static string? NormalizeAndValidateNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalizedNotes = notes.Trim();
        if (normalizedNotes.Length > NotesMaxLength)
        {
            throw new DomainException($"As observacoes devem ter no maximo {NotesMaxLength} caracteres.");
        }

        return normalizedNotes;
    }

    private static string? NormalizeAndValidateStockOverrideReason(bool isStockOverride, string? stockOverrideReason)
    {
        if (!isStockOverride)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(stockOverrideReason))
        {
            throw new DomainException("O motivo do override de estoque e obrigatorio.");
        }

        return stockOverrideReason.Trim();
    }

    private Reservation() { }
}
