using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class RegisterTests : TestBase
    {
        public RegisterTests(Startup factory) : base(factory) { }

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
        public async Task UserCannotRegisterWithInvalidData(RegisterDTO user)
        {
            var ex = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _mediator.Send(new RegisterCommand(user));
            });

            Assert.NotEmpty(ex.Errors);
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

            var ex = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _mediator.Send(new RegisterCommand(
                    new RegisterDTO
                    {
                        Email = "john.doe@example.com",
                        Username = "John Doe",
                        Password = "password",
                    }
                ));
            });

            Assert.Equal("Email is already used", ex.Errors.First(x => x.PropertyName == "User.Email").ErrorMessage);
        }
    }
}
