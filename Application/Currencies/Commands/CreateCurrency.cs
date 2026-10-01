using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Domain.Entities;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase) : IRequest<IResult>;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("El código es requerido.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.RateToBase).GreaterThan(0).WithMessage("La tasa debe ser mayor a 0.");
    }
}

public class CreateCurrencyHandler : IRequestHandler<CreateCurrencyCommand, IResult>
{
    private readonly AppDbContext _db;

    public CreateCurrencyHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _db.Currencies
            .AnyAsync(c => c.Code == request.Code.ToUpper(), cancellationToken);

        if (codeExists)
        {
            return Results.BadRequest(new { message = "El código de divisa ya existe." });
        }

        var currency = new Currency
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            RateToBase = request.RateToBase
        };

        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/currencies/{currency.Id}", currency);
    }
}