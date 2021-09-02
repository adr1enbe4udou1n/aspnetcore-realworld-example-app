using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Profiles.Queries;
using Xunit;

namespace Application.IntegrationTests.Profiles
{
    public class ProfileGetTests : TestBase
    {
        public ProfileGetTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanGetProfile()
        {
            // TODO

            var response = await _mediator.Send(new ProfileGetQuery("john"));

            // TODO
        }
    }
}