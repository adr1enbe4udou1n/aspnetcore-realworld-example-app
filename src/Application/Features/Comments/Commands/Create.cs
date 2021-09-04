using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Comments.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands
{
    public class CommentCreateDTO
    {

        public string Body { get; set; }
    }

    public record CommentEnvelope(CommentDTO Comment);

    public record CommentCreateCommand(string Slug, CommentCreateDTO Comment) : IAuthorizationRequest<CommentEnvelope>;

    public class CommentCreateValidator : AbstractValidator<CommentCreateCommand>
    {
        public CommentCreateValidator()
        {
            RuleFor(x => x.Comment.Body).NotNull().NotEmpty();
        }
    }

    public class CommentCreateHandler : IAuthorizationRequestHandler<CommentCreateCommand, CommentEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public CommentCreateHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CommentEnvelope> Handle(CommentCreateCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            var comment = _mapper.Map<Comment>(request.Comment);
            comment.ArticleId = article.Id;

            await _context.Comments.AddAsync(comment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new CommentEnvelope(_mapper.Map<CommentDTO>(comment));
        }
    }
}