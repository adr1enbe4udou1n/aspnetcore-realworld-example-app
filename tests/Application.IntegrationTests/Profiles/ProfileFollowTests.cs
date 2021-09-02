using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Profiles.Commands;
using Application.Features.Profiles.Queries;
using Xunit;

namespace Application.IntegrationTests.Profiles
{
    public class ProfileFollowTests : TestBase
    {
        public ProfileFollowTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanFollowProfile()
        {
            // TODO

            var response = await _mediator.Send(new ProfileFollowCommand("john", true));

            // TODO
        }

        [Fact]
        public async Task CanUnfollowProfile()
        {
            // TODO

            var response = await _mediator.Send(new ProfileFollowCommand("john", false));

            // TODO
        }
    }
}