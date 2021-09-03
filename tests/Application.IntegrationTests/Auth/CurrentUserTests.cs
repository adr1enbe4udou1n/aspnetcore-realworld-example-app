using System;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Exceptions;
using Domain.Entities;
using Xunit;
using FluentAssertions;

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

            currentUser.User.Username.Should().Be("John Doe");
            currentUser.User.Email.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task GuestUserCannotFetchInfos()
        {
            await _mediator.Invoking(m => m.Send(new CurrentUserQuery()))
                .Should().ThrowAsync<UnauthorizedException>();
        }
    }
}