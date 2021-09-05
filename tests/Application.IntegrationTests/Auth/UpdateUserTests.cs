using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Auth.Commands;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Auth
{
    public class UpdateUserTests : TestBase
    {
        public UpdateUserTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task LoggedUserCanUpdateInfos()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            var request = new UpdateUserCommand(new UpdateUserDTO
            {
                Email = "jane.doe@example.com"
            });

            var currentUser = await Act(() =>
                _mediator.Send(request)
            );

            currentUser.User.Username.Should().Be("John Doe");
            currentUser.User.Email.Should().Be("jane.doe@example.com");

            var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
            created.Should().NotBeNull();
        }

        [Fact]
        public async Task GuestUserCannotUpdateInfos()
        {
            await Act(() =>
                _mediator.Invoking(m => m.Send(new UpdateUserCommand(
                    new UpdateUserDTO
                    {
                        Email = "jane.doe@example.com"
                    }
                )))
                    .Should().ThrowAsync<UnauthorizedException>()
            );
        }

        [Fact]
        public async Task LoggedUserCannotUpdateWithAlreadyUsedEmail()
        {
            var created = await _context.Users.AddAsync(new User
            {
                Name = "John Doe",
                Email = "jane.doe@example.com"
            });
            await _context.SaveChangesAsync();

            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Act(() =>
                _mediator.Invoking(m => m.Send(new UpdateUserCommand(
                    new UpdateUserDTO
                    {
                        Email = "jane.doe@example.com",
                    }
                )))
                    .Should().ThrowAsync<ValidationException>()
                    .Where(e => e.Errors.First(x => x.PropertyName == "User.Email")
                        .ErrorMessage == "Email is already used")
            );
        }
    }
}
