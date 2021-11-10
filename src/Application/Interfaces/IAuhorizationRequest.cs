using MediatR;

namespace Application.Interfaces;

public interface IAuthorizationRequest : IRequest { }
public interface IAuthorizationRequest<TRequest> : IRequest<TRequest> { }
