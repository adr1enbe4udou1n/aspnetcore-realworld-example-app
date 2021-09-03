using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Application.Features.Articles.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class TagsResolver : IValueResolver<ArticleCreateDTO, object, List<ArticleTag>>
    {
        private readonly IAppDbContext _context;

        public TagsResolver(IAppDbContext context)
        {
            _context = context;
        }

        public List<ArticleTag> Resolve(ArticleCreateDTO source, object destination, List<ArticleTag> destMember, ResolutionContext context)
        {
            return source.TagList.Where(x => !String.IsNullOrEmpty(x)).Select(x =>
            {
                var tag = _context.Tags.FirstOrDefault(t => t.Name == x);

                return new ArticleTag
                {
                    Tag = tag == null ? new Tag { Name = x } : tag
                };
            }).ToList();
        }
    }
}