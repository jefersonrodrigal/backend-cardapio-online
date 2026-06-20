using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Features.Clients.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Clients.Commands;

public record CreateClientCommand(
    string Name,
    string Email,
    string Phone,
    string ZipCode,
    string Street,
    string Number,
    string Neighborhood,
    string City,
    string State,
    string Complement,
    string Password
) : IRequest<ClientDto>;

public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(9);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().Length(2);
        RuleFor(x => x.Complement).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(64);
    }
}

public class CreateClientHandler(IApplicationDbContext db)
    : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand cmd, CancellationToken ct)
    {
        var existingPhone = await db.Clients.AnyAsync(c => c.Phone == cmd.Phone, ct);
        if (existingPhone)
        {
            throw new InvalidOperationException("Ja existe um cliente cadastrado com este telefone.");
        }

        var existingEmail = await db.Clients.AnyAsync(c => c.Email == cmd.Email, ct);
        if (existingEmail)
        {
            throw new InvalidOperationException("Ja existe um cliente cadastrado com este e-mail.");
        }

        var client = new Client
        {
            Name = cmd.Name,
            Email = cmd.Email,
            Phone = cmd.Phone,
            ZipCode = cmd.ZipCode,
            Street = cmd.Street,
            Number = cmd.Number,
            Neighborhood = cmd.Neighborhood,
            City = cmd.City,
            State = cmd.State,
            Complement = cmd.Complement,
            PasswordHash = ClientPasswordHasher.HashPassword(cmd.Password)
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync(ct);

        return new ClientDto(
            client.Id,
            client.Name,
            client.Email,
            client.Phone,
            client.ZipCode,
            client.Street,
            client.Number,
            client.Neighborhood,
            client.City,
            client.State,
            client.Complement,
            ClientAddressFormatter.Format(
                client.Street,
                client.Number,
                client.Neighborhood,
                client.City,
                client.State,
                client.ZipCode,
                client.Complement),
            client.RegisteredAt.ToString("dd/MM/yyyy"),
            0,
            0m
        );
    }
}
