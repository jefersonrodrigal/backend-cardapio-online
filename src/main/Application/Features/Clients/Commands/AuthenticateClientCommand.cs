using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Features.Clients.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Clients.Commands;

public record AuthenticateClientCommand(
    string Email,
    string Password
) : IRequest<ClientDto>;

public class AuthenticateClientValidator : AbstractValidator<AuthenticateClientCommand>
{
    public AuthenticateClientValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(64);
    }
}

public class AuthenticateClientHandler(IApplicationDbContext db)
    : IRequestHandler<AuthenticateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(AuthenticateClientCommand cmd, CancellationToken ct)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Email == cmd.Email, ct);
        if (client is null || !ClientPasswordHasher.VerifyPassword(cmd.Password, client.PasswordHash))
        {
            throw new UnauthorizedAccessException("E-mail ou senha invalidos.");
        }

        var orders = await db.Orders
            .Where(order => order.ClientId == client.Id)
            .ToListAsync(ct);

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
            orders.Count,
            orders.Sum(order => order.Total)
        );
    }
}
