using Application.Features.Auth.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class NewUserAction : IMappingAction<NewUserDTO, User>
{
    private readonly IPasswordHasher _passwordHasher;

    public NewUserAction(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public void Process(NewUserDTO source, User destination, ResolutionContext context)
    {
        destination.Password = _passwordHasher.Hash(source.Password);
    }
}