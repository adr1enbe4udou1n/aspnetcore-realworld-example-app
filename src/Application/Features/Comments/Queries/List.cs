using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries
{
    public class CommentDTO
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public AuthorDTO Author { get; set; }
    }

    public record CommentsEnvelope(IEnumerable<CommentDTO> Comments);

    public record CommentsListQuery(string Slug) : IRequest<CommentsEnvelope>;

    public class CommentsListHandler : IRequestHandler<CommentsListQuery, CommentsEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public CommentsListHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CommentsEnvelope> Handle(CommentsListQuery request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);
            var comments = article.Comments.OrderByDescending(x => x.Id).ToList();

            return new CommentsEnvelope(_mapper.Map<IEnumerable<CommentDTO>>(comments));
        }
    }
}