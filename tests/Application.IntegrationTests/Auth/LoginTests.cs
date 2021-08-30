using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Auth;
using Domain.Entities;
using FluentValidation;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class LoginTests : TestBase
    {
        public LoginTests(Startup factory) : base(factory) { }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { new LoginDTO {
                Email = "jane.doe@example.com",
                Password = "password",
            } },
            new object[] { new LoginDTO {
                Email = "john.doe@example.com",
                Password = "badpassword",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task UserCannotLoginWithInvalidData(LoginDTO credentials)
        {
            await _context.Users.AddAsync(new User
            {
                Email = "john.doe@example.com",
                Name = "John Doe",
                Password = _passwordHasher.Hash("password"),
            });
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(new LoginCommand(credentials)));
        }

        [Fact]
        public async Task UserCanLogin()
        {
            var user = new User
            {
                Email = "john.doe@example.com",
                Name = "John Doe",
                Password = _passwordHasher.Hash("password"),
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var request = new LoginCommand(new LoginDTO
            {
                Email = "john.doe@example.com",
                Password = "password",
            });

            var currentUser = await _mediator.Send(request);

            Assert.Equal("John Doe", currentUser.User.Username);
            Assert.Equal("john.doe@example.com", currentUser.User.Email);

            var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

            Assert.Equal((long)user.Id, payload["jti"]);
            Assert.Equal("John Doe", payload["name"]);
            Assert.Equal("john.doe@example.com", payload["email_verified"]);
        }
    }
}