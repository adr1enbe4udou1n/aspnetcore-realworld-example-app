using System.Globalization;
using System.Net;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Auth;

public class RegisterTests : TestBase
{
    private static IEnumerable<TestCaseData> InvalidRegisters()
    {
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe",
            Username = "John Doe",
            Password = "password",
        });
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe@example.com",
        });
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "pass",
        });
    }

    [Test, TestCaseSource(nameof(InvalidRegisters))]
    public async Task User_Cannot_Register_With_Invalid_Data(NewUserDTO user)
    {
        var response = await Act(HttpMethod.Post, "/users", new NewUserRequest(user));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task User_Can_Register()
    {
        var request = new NewUserRequest(new NewUserDTO
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "password",
        });

        var currentUser = await Act<UserResponse>(HttpMethod.Post, "/users", request);

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");

        var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        created.Should().NotBeNull();

        _passwordHasher.Check("password", created!.Password!).Should().BeTrue();

        var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

        payload["id"].Should().Be(created.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }

    [Test]
    public async Task User_Cannot_Register_Twice()
    {
        await _context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = "password",
        });
        await _context.SaveChangesAsync();

        var response = await Act(
            HttpMethod.Post, "/users",
            new NewUserRequest(
                new NewUserDTO
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            ));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}