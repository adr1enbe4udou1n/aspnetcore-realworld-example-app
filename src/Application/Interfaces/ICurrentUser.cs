using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICurrentUser
{
    User User { get; }

    long Identifier { get; }

    bool IsAuthenticated { get; }

    Task SetIdentifier(long identifier);

    Task Fresh();
}
