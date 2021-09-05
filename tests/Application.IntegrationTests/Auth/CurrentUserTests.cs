using System;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Auth
{
    public class CurrentUserTests : TestBase
    {
        public CurrentUserTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task LoggedUserCanFetchInfos()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            var currentUser = await Act(() =>
                _mediator.Send(new CurrentUserQuery())
            );

            currentUser.User.Username.Should().Be("John Doe");
            currentUser.User.Email.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task GuestUserCannotFetchInfos()
        {
            await Act(() =>
                _mediator.Invoking(m => m.Send(new CurrentUserQuery()))
                    .Should().ThrowAsync<UnauthorizedException>()
            );
        }
    }
}
