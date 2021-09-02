using System;
using System.Threading.Tasks;
using Application.Auth.Commands;
using Application.Auth.Queries;
using Application.Exceptions;
using Domain.Entities;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class CurrentUserTests : TestBase
    {
        public CurrentUserTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task LoggedUserCanFetchInfos()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            var currentUser = await _mediator.Send(new CurrentUserQuery());

            Assert.Equal("John Doe", currentUser.User.Username);
            Assert.Equal("john.doe@example.com", currentUser.User.Email);
        }

        [Fact]
        public void GuestUserCannotFetchInfos()
        {
            Assert.ThrowsAsync<UnauthorizedException>(async () =>
            {
                await _mediator.Send(new CurrentUserQuery());
            });
        }
    }
}