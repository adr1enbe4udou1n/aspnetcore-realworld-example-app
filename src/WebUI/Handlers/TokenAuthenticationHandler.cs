using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebUI.Handlers;

public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUser _currentUser;

    public TokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IJwtTokenGenerator jwtTokenGenerator, ICurrentUser currentUser) : base(options, logger, encoder, clock)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUser = currentUser;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token == null)
        {
            return AuthenticateResult.Fail("Token is null");
        }

        try
        {
            await _jwtTokenGenerator.SetCurrentUserFromToken(token);

            if (_currentUser.User != null)
            {
                return AuthenticateResult.Success(
                    new AuthenticationTicket(new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, _currentUser.User.Name) }, Scheme.Name)
                    ), Scheme.Name)
                );
            }

            return AuthenticateResult.Fail("Unknown user");
        }
        catch (SecurityTokenException)
        {
            return AuthenticateResult.Fail("Invalid token");
        }
    }
}