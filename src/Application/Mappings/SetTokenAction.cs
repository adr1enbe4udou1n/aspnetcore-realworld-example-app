using Application.Features.Auth.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class SetTokenAction : IMappingAction<User, UserDTO>
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public SetTokenAction(IJwtTokenGenerator jwtTokenGenerator)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public void Process(User source, UserDTO destination, ResolutionContext context)
    {
        destination.Token = _jwtTokenGenerator.CreateToken(source);
    }
}