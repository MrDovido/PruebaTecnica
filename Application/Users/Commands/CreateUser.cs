using FluentValidation;
using MediatR;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Domain.Entities;
using PruebaTecnica.Infrastructure.Persistence;

namespace PruebaTecnica.Application.Users.Commands;

public record CreateUserCommand(string Name, string Email, string Password) : IRequest<IResult>;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Formato de email inválido.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, IResult>
{
    private readonly AppDbContext _db;

    public CreateUserHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return Results.BadRequest(new { message = "El email ya se encuentra registrado." });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password), // O tu método de hashing
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/users/{user.Id}", user);
    }
    private static string HashPassword(string password)
{
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}
}
