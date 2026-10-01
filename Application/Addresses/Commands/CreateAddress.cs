using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Domain.Entities;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Addresses.Commands;

public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;

public class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().WithMessage("La calle es requerida.");
        RuleFor(x => x.City).NotEmpty().WithMessage("La ciudad es requerida.");
        RuleFor(x => x.Country).NotEmpty().WithMessage("El país es requerido.");
    }
}

public class CreateAddressHandler : IRequestHandler<CreateAddressCommand, IResult>
{
    private readonly AppDbContext _db;

    public CreateAddressHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists) return Results.NotFound(new { message = "El usuario especificado no existe." });

        var address = new Address
        {
            UserId = request.UserId,
            Street = request.Street,
            City = request.City,
            Country = request.Country,
            ZipCode = request.ZipCode
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/addresses/{address.Id}", address);
    }
}