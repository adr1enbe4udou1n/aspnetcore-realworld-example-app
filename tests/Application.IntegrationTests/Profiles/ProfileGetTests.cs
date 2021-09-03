using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Profiles
{
    public class ProfileGetTests : TestBase
    {
        public ProfileGetTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanGetProfile()
        {
            await _context.Users.AddAsync(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300",
            });
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new ProfileGetQuery("John Doe"));

            response.Profile.Should().BeEquivalentTo(new ProfileDTO
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300",
                Following = false
            });
        }

        [Fact]
        public async Task CannotGetNotExistingProfile()
        {
            await _mediator.Invoking(m => m.Send(new ProfileGetQuery("John Doe")))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanGetFollowedProfile()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Following = new List<FollowerUser>
                {
                    new FollowerUser
                    {
                        Following = new User
                        {
                            Name = "Jane Doe",
                            Email = "jane.doe@example.com",
                            Bio = "My Bio",
                            Image = "https://i.pravatar.cc/300",
                        }
                    }
                }
            });

            var response = await _mediator.Send(new ProfileGetQuery("Jane Doe"));

            response.Profile.Should().BeEquivalentTo(new ProfileDTO
            {
                Username = "Jane Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300",
                Following = true
            });
        }
    }
}