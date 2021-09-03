using Application.Features.Articles.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class AuthorResolver<TSource> : IValueResolver<TSource, object, User>
    {
        private readonly ICurrentUser _currentUser;

        public AuthorResolver(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        User IValueResolver<TSource, object, User>.Resolve(TSource source, object destination, User destMember, ResolutionContext context)
        {
            return _currentUser.User;
        }
    }
}