using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;

namespace Conduit.Application.Features.Comments.Queries;

public class CommentDto
{
    public int Id { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required ProfileDto Author { get; set; }
}

public static class CommentMapper
{
    public static CommentDto Map(this Comment comment, User? user)
    {
        return new()
        {
            Id = comment.Id,
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Author = comment.Author.MapToProfile(user),
        };
    }
}