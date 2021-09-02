using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Auth.Commands;
using Application.Exceptions;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class UpdateUserTests : TestBase
    {
        public UpdateUserTests(Startup factory) : base(factory) { }

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

            var currentUser = await _mediator.Send(request);

            Assert.Equal("John Doe", currentUser.User.Username);
            Assert.Equal("jane.doe@example.com", currentUser.User.Email);

            var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
            Assert.NotNull(created);
        }

        [Fact]
        public void GuestUserCannotUpdateInfos()
        {
            Assert.ThrowsAsync<UnauthorizedException>(async () =>
            {
                await _mediator.Send(new UpdateUserCommand(
                    new UpdateUserDTO()
                ));
            });
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

            var ex = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _mediator.Send(new UpdateUserCommand(
                    new UpdateUserDTO
                    {
                        Email = "jane.doe@example.com",
                    }
                ));
            });

            Assert.Equal("Email is already used", ex.Errors.First(x => x.PropertyName == "User.Email").ErrorMessage);
        }
    }
}