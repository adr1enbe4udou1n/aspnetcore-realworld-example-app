using System.ComponentModel;
using Application.Extensions;
using Application.Features.Comments.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands;

public class NewCommentDTO
{

    public string? Body { get; set; }
}

public record SingleCommentResponse(CommentDTO Comment);

[DisplayName("NewCommentRequest")]
public record NewCommentBody(NewCommentDTO Comment);
public record NewCommentRequest(string Slug, NewCommentDTO Comment) : IAuthorizationRequest<SingleCommentResponse>;

public class CommentCreateValidator : AbstractValidator<NewCommentRequest>
{
    public CommentCreateValidator()
    {
        RuleFor(x => x.Comment.Body).NotNull().NotEmpty();
    }
}

public class CommentCreateHandler : IAuthorizationRequestHandler<NewCommentRequest, SingleCommentResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public CommentCreateHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<SingleCommentResponse> Handle(NewCommentRequest request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comment = _mapper.Map<Comment>(request.Comment);
        comment.AuthorId = _currentUser.User!.Id;
        comment.ArticleId = article.Id;

        await _context.Comments.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleCommentResponse(_mapper.Map<CommentDTO>(comment));
    }
}