using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebUI.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtTokenGenerator jwtTokenGenerator, ICurrentUser currentUser)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var userId = long.Parse(jwtTokenGenerator.DecodeToken(token)["id"]);

                await currentUser.SetIdentifier(userId);
            }

            await _next(context);
        }
    }
}