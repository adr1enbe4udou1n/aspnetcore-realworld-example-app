using System.ComponentModel;
using Application.Extensions;
using Application.Features.Comments.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Comments.Commands;

public class NewCommentDTO
{

    public string Body { get; set; } = default!;
}

public record SingleCommentResponse(CommentDTO Comment);

[DisplayName("NewCommentRequest")]
public record NewCommentBody(NewCommentDTO Comment);
public record NewCommentRequest(string Slug, NewCommentDTO Comment) : ICommand<SingleCommentResponse>;

public class CommentCreateValidator : AbstractValidator<NewCommentRequest>
{
    public CommentCreateValidator()
    {
        RuleFor(x => x.Comment.Body).NotNull().NotEmpty();
    }
}

public class CommentCreateHandler : ICommandHandler<NewCommentRequest, SingleCommentResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CommentCreateHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SingleCommentResponse> Handle(NewCommentRequest request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comment = new Comment
        {
            Body = request.Comment.Body,
            Article = article,
            Author = _currentUser.User!
        };

        await _context.Comments.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleCommentResponse(new CommentDTO(comment, _currentUser.User));
    }
}