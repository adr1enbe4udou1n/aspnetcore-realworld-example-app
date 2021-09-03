using Application.Features.Articles.Commands;
using Application.Features.Auth.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class SlugResolver : IValueResolver<ArticleCreateDTO, object, string>
    {
        private readonly ISlugifier _slugifier;

        public SlugResolver(ISlugifier slugifier)
        {
            _slugifier = slugifier;
        }

        string IValueResolver<ArticleCreateDTO, object, string>.Resolve(ArticleCreateDTO source, object destination, string destMember, ResolutionContext context)
        {
            return _slugifier.Generate(source.Title);
        }
    }
}