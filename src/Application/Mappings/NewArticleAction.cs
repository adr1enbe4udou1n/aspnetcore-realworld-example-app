using Application.Features.Articles.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class NewArticleAction : IMappingAction<NewArticleDTO, Article>
{
    private readonly ICurrentUser _currentUser;
    private readonly ISlugifier _slugifier;

    public NewArticleAction(ICurrentUser currentUser, ISlugifier slugifier)
    {
        _currentUser = currentUser;
        _slugifier = slugifier;
    }

    public void Process(NewArticleDTO source, Article destination, ResolutionContext context)
    {
        destination.Author = _currentUser.User!;
        destination.Slug = _slugifier.Generate(source.Title);
    }
}