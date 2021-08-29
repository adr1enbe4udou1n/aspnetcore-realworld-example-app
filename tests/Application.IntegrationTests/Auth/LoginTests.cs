using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Auth;
using Domain.Entities;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class LoginTests : TestBase
    {
        private LoginValidator _validator;

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { new LoginDTO {
                Email = "john.doe",
                Password = "password",
            } },
            new object[] { new LoginDTO {
                Email = "john.doe@example.com",
                Password = "pass",
            } },
            new object[] { new LoginDTO {
                Email = "jane.doe@example.com",
                Password = "password",
            } },
            new object[] { new LoginDTO {
                Email = "john.doe@example.com",
                Password = "badpassword",
            } },
        };

        public LoginTests()
        {
            _validator = new LoginValidator(_context, _passwordHasher);
        }

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

            var result = _validator.TestValidate(new LoginCommand(credentials));

            result.ShouldHaveAnyValidationError();
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

            Assert.Equal(currentUser.User.Username, "John Doe");
            Assert.Equal(currentUser.User.Email, "john.doe@example.com");

            var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

            Assert.Equal(payload["jti"], (long)user.Id);
            Assert.Equal(payload["name"], "John Doe");
            Assert.Equal(payload["email_verified"], "john.doe@example.com");
        }
    }
}