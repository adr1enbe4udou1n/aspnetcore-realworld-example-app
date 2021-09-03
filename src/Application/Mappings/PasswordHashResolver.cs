using Application.Features.Auth.Commands;
using Application.Interfaces;
using AutoMapper;

namespace Application.Mappings
{
    public class PasswordHashResolver : IValueResolver<RegisterDTO, object, string>
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHashResolver(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        string IValueResolver<RegisterDTO, object, string>.Resolve(RegisterDTO source, object destination, string destMember, ResolutionContext context)
        {
            return _passwordHasher.Hash(source.Password);
        }
    }
}