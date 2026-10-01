namespace PruebaTecnica.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public User? User { get; set; }
}

public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal RateToBase { get; set; }
}