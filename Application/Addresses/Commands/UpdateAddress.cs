using FluentValidation;
using MediatR;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Addresses.Commands;

public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;

public class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().WithMessage("La calle es requerida.");
        RuleFor(x => x.City).NotEmpty().WithMessage("La ciudad es requerida.");
        RuleFor(x => x.Country).NotEmpty().WithMessage("El país es requerido.");
    }
}

public class UpdateAddressHandler : IRequestHandler<UpdateAddressCommand, IResult>
{
    private readonly AppDbContext _db;

    public UpdateAddressHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _db.Addresses.FindAsync(new object[] { request.Id }, cancellationToken);
        if (address is null) 
            return Results.NotFound(new { message = "Dirección no encontrada." });

        address.Street = request.Street;
        address.City = request.City;
        address.Country = request.Country;
        address.ZipCode = request.ZipCode;

        await _db.SaveChangesAsync(cancellationToken);
        return Results.Ok(address);
    }
}