namespace Application.Features.Clients.Dtos;

public record ClientDto(
    Guid Id,
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
    string FullAddress,
    string RegisteredAt,
    int OrdersCount,
    decimal TotalSpent
);
