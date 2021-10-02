using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new UpdateUserDTO {
                Username = "John Doe",
                Email = "john.doe",
                Bio = "My Bio",
            } },
            new [] { new UpdateUserDTO {
                Username = "",
                Email = "john.doe@example.com",
                Bio = "My Bio",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task CannotUpdateInfosWithInvalidData(UpdateUserDTO user)
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(new UpdateUserRequest(user)))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task LoggedUserCanUpdateInfos()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            var request = new UpdateUserRequest(new UpdateUserDTO
            {
                Email = "jane.doe@example.com",
                Bio = "My Bio"
            });

            var currentUser = await Act(request);

            currentUser.User.Username.Should().Be("John Doe");
            currentUser.User.Email.Should().Be("jane.doe@example.com");
            currentUser.User.Bio.Should().Be("My Bio");
            currentUser.User.Image.Should().BeNull();

            var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
            created.Should().NotBeNull();
        }

        [Fact]
        public async Task GuestUserCannotUpdateInfos()
        {
            await this.Invoking(x => x.Act(new UpdateUserRequest(
                new UpdateUserDTO
                {
                    Email = "jane.doe@example.com"
                }
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task LoggedUserCannotUpdateWithAlreadyUsedEmail()
        {
            var created = await Context.Users.AddAsync(new User
            {
                Name = "John Doe",
                Email = "jane.doe@example.com"
            });
            await Context.SaveChangesAsync();

            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(
                new UpdateUserRequest(
                    new UpdateUserDTO
                    {
                        Email = "jane.doe@example.com",
                    }
                )))
                    .Should().ThrowAsync<ValidationException>()
                    .Where(e => e.Errors.First(x => x.PropertyName == "User.Email")
                        .ErrorMessage == "Email is already used");
        }
    }
}
