using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Auth
{
    public class RegisterTests : TestBase
    {
        [Fact]
        public async Task UserCanRegister()
        {
            var request = new Register.Command(new Register.UserDTO
            {
                Email = "john.doe@example.com",
                Username = "John Doe",
                Password = "password",
            });

            var currentUser = await _mediator.Send(request);

            var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();

            Assert.NotNull(created);
            Assert.Equal(currentUser.User.Username, "John Doe");
            Assert.Equal(currentUser.User.Email, "john.doe@example.com");
            // Assert.Equal(created.Hash, await new PasswordHasher().Hash("password", created.Salt));
        }
    }
}
