using MediatR;

public record OpenAccountCommand(Guid CustomerId, decimal InitialDeposit) : IRequest<Guid>;
