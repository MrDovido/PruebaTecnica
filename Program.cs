using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Application.Addresses.Commands;
using PruebaTecnica.Application.Common.Behaviors;
using PruebaTecnica.Application.Common.Security;
using PruebaTecnica.Application.Currencies.Commands;
using PruebaTecnica.Application.Users.Commands;
using PruebaTecnica.Domain.Entities;
using PruebaTecnica.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de datos SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// 2. Registro de MediatR, FluentValidation y Behaviors
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(CreateUserCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// 3. Security Middleware (X-API-KEY)
app.UseMiddleware<ApiKeyMiddleware>();

// ============================================================================
// 1. ENDPOINTS: USUARIOS (USERS)
// ============================================================================

// GET - Listar usuarios
app.MapGet("/users", async (bool? isActive, AppDbContext db) =>
{
    var query = db.Users.AsQueryable();
    if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
    return Results.Ok(await query.ToListAsync());
});

// GET - Obtener usuario por ID
app.MapGet("/users/{id:int}", async (int id, AppDbContext db) =>
    await db.Users.FindAsync(id) is User u ? Results.Ok(u) : Results.NotFound());

// POST - Crear usuario
app.MapPost("/users", async (CreateUserCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd));

// PUT - Modificar usuario
app.MapPut("/users/{id:int}", async (int id, UpdateUserCommand cmd, IMediator mediator) =>
{
    var commandWithId = cmd with { Id = id };
    return await mediator.Send(commandWithId);
});

// DELETE - Eliminar usuario
app.MapDelete("/users/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ============================================================================
// 2. ENDPOINTS: DIRECCIONES (ADDRESSES)
// ============================================================================

// POST - Crear dirección para usuario
app.MapPost("/users/{userId:int}/addresses", async (int userId, CreateAddressCommand cmd, IMediator mediator) =>
{
    var commandWithUser = cmd with { UserId = userId };
    return await mediator.Send(commandWithUser);
});

// GET - Listar direcciones de un usuario
app.MapGet("/users/{userId:int}/addresses", async (int userId, AppDbContext db) =>
{
    var userExists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists) return Results.NotFound("Usuario no encontrado.");
    return Results.Ok(await db.Addresses.Where(a => a.UserId == userId).ToListAsync());
});

// PUT - Modificar dirección por ID
app.MapPut("/addresses/{id:int}", async (int id, UpdateAddressCommand cmd, IMediator mediator) =>
{
    var commandWithId = cmd with { Id = id };
    return await mediator.Send(commandWithId);
});

// DELETE - Eliminar dirección por ID
app.MapDelete("/addresses/{id:int}", async (int id, AppDbContext db) =>
{
    var addr = await db.Addresses.FindAsync(id);
    if (addr is null) return Results.NotFound();
    db.Addresses.Remove(addr);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ============================================================================
// 3. ENDPOINTS: MONEDAS (CURRENCIES)
// ============================================================================

// GET - Listar monedas
app.MapGet("/currencies", async (AppDbContext db) => 
    Results.Ok(await db.Currencies.ToListAsync()));

// POST - Crear moneda
app.MapPost("/currencies", async (CreateCurrencyCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd));

// PUT - Modificar moneda
app.MapPut("/currencies/{id:int}", async (int id, UpdateCurrencyCommand cmd, IMediator mediator) =>
{
    var commandWithId = cmd with { Id = id };
    return await mediator.Send(commandWithId);
});

// POST - Convertir moneda
app.MapPost("/currency/convert", async (ConvertCurrencyCommand cmd, IMediator mediator) => 
    await mediator.Send(cmd));

// ============================================================================
// INICIALIZACIÓN DE BASE DE DATOS Y EJECUCIÓN
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// DTOs auxiliares de Commands requeridos para los endpoints
//public record CreateUserCommand(string Name, string Email, string Password) : IRequest<IResult>;
//public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive) : IRequest<IResult>;
//public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;
//public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode) : IRequest<IResult>;
//public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase) : IRequest<IResult>;