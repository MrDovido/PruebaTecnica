using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Currencies.Commands;

public record ConvertCurrencyCommand(string FromCurrencyCode, string ToCurrencyCode, decimal Amount) 
    : IRequest<IResult>;

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty();
        RuleFor(x => x.ToCurrencyCode).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
    }
}

public class ConvertCurrencyHandler : IRequestHandler<ConvertCurrencyCommand, IResult>
{
    private readonly AppDbContext _db;

    public ConvertCurrencyHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(ConvertCurrencyCommand request, CancellationToken cancellationToken)
    {
        var fromCurrency = await _db.Currencies
            .FirstOrDefaultAsync(c => c.Code == request.FromCurrencyCode.ToUpper(), cancellationToken);
            
        var toCurrency = await _db.Currencies
            .FirstOrDefaultAsync(c => c.Code == request.ToCurrencyCode.ToUpper(), cancellationToken);

        if (fromCurrency is null || toCurrency is null)
        {
            return Results.BadRequest(new { message = "Una o ambas monedas no existen." });
        }

        // Fórmula: (Amount * From.RateToBase) / To.RateToBase
        var amountInBase = request.Amount * fromCurrency.RateToBase;
        var convertedAmount = amountInBase / toCurrency.RateToBase;

        return Results.Ok(new
        {
            fromCurrency = fromCurrency.Code,
            toCurrency = toCurrency.Code,
            originalAmount = request.Amount,
            convertedAmount = Math.Round(convertedAmount, 2)
        });
    }
}