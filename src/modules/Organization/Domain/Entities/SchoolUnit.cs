namespace Giglio.EduCore.Organization.Domain.Entities;

public class SchoolUnit
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string Number { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string? Phone { get; private set; }
    public string? ResponsibleName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private SchoolUnit() { }

    public SchoolUnit(
        string name,
        string address,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? phone = null,
        string? responsibleName = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetAddress(address);
        SetNumber(number);
        SetNeighborhood(neighborhood);
        SetCity(city);
        SetState(state);
        SetZipCode(zipCode);
        Phone = phone;
        ResponsibleName = responsibleName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required", nameof(address));
        Address = address.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNumber(string number)
    {
        Number = number.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNeighborhood(string neighborhood)
    {
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Neighborhood is required", nameof(neighborhood));
        Neighborhood = neighborhood.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        City = city.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required", nameof(state));
        State = state.Trim().ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetZipCode(string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode is required", nameof(zipCode));
        ZipCode = zipCode.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPhone(string? phone)
    {
        Phone = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResponsibleName(string? responsibleName)
    {
        ResponsibleName = responsibleName?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}