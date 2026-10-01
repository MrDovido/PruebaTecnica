using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Application.Common.Behaviors;
using PruebaTecnica.Application.Common.Security;
using PruebaTecnica.Application.Currencies.Commands;
using PruebaTecnica.Domain.Entities;
using PruebaTecnica.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// 2. Registrar MediatR y FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// 3. Aplicar Middleware de API Key
app.UseMiddleware<ApiKeyMiddleware>();

// --- ENDPOINTS USUARIOS ---
app.MapPost("/users", async (CreateUserCommand cmd, IMediator mediator) => await mediator.Send(cmd));
app.MapGet("/users", async (bool? isActive, AppDbContext db) =>
{
    var query = db.Users.AsQueryable();
    if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
    return Results.Ok(await query.ToListAsync());
});
app.MapGet("/users/{id:int}", async (int id, AppDbContext db) =>
    await db.Users.FindAsync(id) is User u ? Results.Ok(u) : Results.NotFound());

app.MapPut("/users/{id:int}", async (int id, UpdateUserCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd with { Id = id }));

app.MapDelete("/users/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// --- ENDPOINTS DIRECCIONES ---
app.MapPost("/users/{userId:int}/addresses", async (int userId, CreateAddressCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd with { UserId = userId }));

app.MapGet("/users/{userId:int}/addresses", async (int userId, AppDbContext db) =>
{
    var userExists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists) return Results.NotFound("Usuario no encontrado.");
    return Results.Ok(await db.Addresses.Where(a => a.UserId == userId).ToListAsync());
});

app.MapPut("/addresses/{id:int}", async (int id, UpdateAddressCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd with { Id = id }));

app.MapDelete("/addresses/{id:int}", async (int id, AppDbContext db) =>
{
    var addr = await db.Addresses.FindAsync(id);
    if (addr is null) return Results.NotFound();
    db.Addresses.Remove(addr);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// --- ENDPOINTS CURRENCY ---
app.MapGet("/currencies", async (AppDbContext db) => Results.Ok(await db.Currencies.ToListAsync()));
app.MapPost("/currencies", async (CreateCurrencyCommand cmd, IMediator mediator) => await mediator.Send(cmd));
app.MapPost("/currency/convert", async (ConvertCurrencyCommand cmd, IMediator mediator) => await mediator.Send(cmd));

// Migración automática al iniciar (útil para pruebas inmediatas)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// DTOs auxiliares de Commands requeridos para los endpoints
public record CreateUserCommand(string Name, string Email, string Password) : IRequest<IResult>;
public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive) : IRequest<IResult>;
public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;
public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;
public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase) : IRequest<IResult>;