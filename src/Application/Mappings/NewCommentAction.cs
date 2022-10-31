using Application.Features.Comments.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class NewCommentAction : IMappingAction<NewCommentDTO, Comment>
{
    private readonly ICurrentUser _currentUser;

    public NewCommentAction(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void Process(NewCommentDTO source, Comment destination, ResolutionContext context)
    {
        destination.Author = _currentUser.User!;
    }
}