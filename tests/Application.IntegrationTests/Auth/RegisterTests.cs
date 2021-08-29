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
        private Register.CommandValidator _validator;

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { new Register.UserDTO {
                Email = "john.doe",
                Username = "John Doe",
                Password = "password",
            } },
            new object[] { new Register.UserDTO {
                Email = "john.doe@example.com",
            } },
            new object[] { new Register.UserDTO {
                Email = "john.doe@example.com",
                Username = "John Doe",
                Password = "pass",
            } },
        };

        public RegisterTests()
        {
            _validator = new Register.CommandValidator(_context);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void UserCannotRegisterWithInvalidData(Register.UserDTO user)
        {
            var result = _validator.TestValidate(new Register.Command(user));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task UserCanRegister()
        {
            var request = new Register.Command(new Register.UserDTO
            {
                Email = "john.doe@example.com",
                Username = "John Doe",
                Password = "password",
            });

            var currentUser = await _mediator.Send(request);

            Assert.Equal(currentUser.User.Username, "John Doe");
            Assert.Equal(currentUser.User.Email, "john.doe@example.com");

            var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
            Assert.NotNull(created);

            Assert.True(_passwordHasher.Check("password", created.Password));

            var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

            Assert.Equal(payload["jti"], (long)created.Id);
            Assert.Equal(payload["name"], "John Doe");
            Assert.Equal(payload["email_verified"], "john.doe@example.com");
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

            var result = _validator.TestValidate(new Register.Command(
                new Register.UserDTO
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
