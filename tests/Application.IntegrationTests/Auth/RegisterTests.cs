using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Auth;
using Domain.Entities;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class RegisterTests : TestBase
    {
        private RegisterValidator _validator;


        public RegisterTests(Startup factory) : base(factory)
        {
            _validator = new RegisterValidator(_context);
        }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { new RegisterDTO {
                Email = "john.doe",
                Username = "John Doe",
                Password = "password",
            } },
            new object[] { new RegisterDTO {
                Email = "john.doe@example.com",
            } },
            new object[] { new RegisterDTO {
                Email = "john.doe@example.com",
                Username = "John Doe",
                Password = "pass",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public void UserCannotRegisterWithInvalidData(RegisterDTO user)
        {
            var result = _validator.TestValidate(new RegisterCommand(user));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task UserCanRegister()
        {
            var request = new RegisterCommand(new RegisterDTO
            {
                Email = "john.doe@example.com",
                Username = "John Doe",
                Password = "password",
            });

            var currentUser = await _mediator.Send(request);

            Assert.Equal("John Doe", currentUser.User.Username);
            Assert.Equal("john.doe@example.com", currentUser.User.Email);

            var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
            Assert.NotNull(created);

            Assert.True(_passwordHasher.Check("password", created.Password));

            var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

            Assert.Equal(created.Id.ToString(), payload["id"]);
            Assert.Equal("John Doe", payload["name"]);
            Assert.Equal("john.doe@example.com", payload["email"]);
        }

        [Fact]
        public async Task UserCannotRegisterTwice()
        {
            await _context.Users.AddAsync(new User
            {
                Email = "john.doe@example.com",
                Name = "John Doe",
                Password = "password",
            });
            await _context.SaveChangesAsync();

            var result = _validator.TestValidate(new RegisterCommand(
                new RegisterDTO
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            ));

            result.ShouldHaveValidationErrorFor(x => x.User.Email);
        }
    }
}
