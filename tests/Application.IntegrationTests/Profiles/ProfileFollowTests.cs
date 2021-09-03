using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Profiles.Commands;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Profiles
{
    public class ProfileFollowTests : TestBase
    {
        public ProfileFollowTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotFollowProfile()
        {
            await _mediator.Invoking(m => m.Send(new ProfileFollowCommand("john", true)))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CanFollowProfile()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _context.Users.AddRangeAsync(new List<User>
            {
                new User
                {
                    Name = "Jane Doe",
                    Email = "jane.doe@example.com",
                },
                new User
                {
                    Name = "Alice",
                    Email = "alice@example.com",
                },
            });
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new ProfileFollowCommand("Jane Doe", true));

            response.Profile.Should().BeEquivalentTo(new ProfileDTO
            {
                Username = "Jane Doe",
                Following = true
            });

            (await _context.Set<FollowerUser>().CountAsync()).Should().Be(1);
            (await _context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Jane Doe"))
                .Should().BeTrue();
        }

        [Fact]
        public async Task CanUnfollowProfile()
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
                        }
                    },
                    new FollowerUser
                    {
                        Following = new User
                        {
                            Name = "Alice",
                            Email = "alice@example.com",
                        }
                    }
                }
            });

            var response = await _mediator.Send(new ProfileFollowCommand("Jane Doe", false));

            response.Profile.Should().BeEquivalentTo(new ProfileDTO
            {
                Username = "Jane Doe",
                Following = false
            });

            (await _context.Set<FollowerUser>().CountAsync()).Should().Be(1);
            (await _context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Alice"))
                .Should().BeTrue();
        }
    }
}