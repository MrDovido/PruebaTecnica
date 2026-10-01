using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Currencies.Commands;

public record UpdateCurrencyCommand(int Id, string Name, decimal RateToBase) : IRequest<IResult>;

public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.RateToBase).GreaterThan(0).WithMessage("La tasa debe ser mayor a 0.");
    }
}

public class UpdateCurrencyHandler : IRequestHandler<UpdateCurrencyCommand, IResult>
{
    private readonly AppDbContext _db;

    public UpdateCurrencyHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _db.Currencies.FindAsync(new object[] { request.Id }, cancellationToken);
        if (currency is null) 
            return Results.NotFound(new { message = "Moneda no encontrada." });

        currency.Name = request.Name;
        currency.RateToBase = request.RateToBase;

        await _db.SaveChangesAsync(cancellationToken);
        return Results.Ok(currency);
    }
}