using MediatR;

public record CreateCustomerCommand(string FirstName, string LastName, string? Email) : IRequest<Guid>;
