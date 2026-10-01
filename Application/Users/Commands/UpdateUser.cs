using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Users.Commands;

public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive) : IRequest<IResult>;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido.");
    }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, IResult>
{
    private readonly AppDbContext _db;

    public UpdateUserHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user is null) 
            return Results.NotFound(new { message = "Usuario no encontrado." });

        var emailInUse = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != request.Id, cancellationToken);
        if (emailInUse) 
            return Results.BadRequest(new { message = "El email ya pertenece a otro usuario." });

        user.Name = request.Name;
        user.Email = request.Email;
        user.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        return Results.Ok(user);
    }
}