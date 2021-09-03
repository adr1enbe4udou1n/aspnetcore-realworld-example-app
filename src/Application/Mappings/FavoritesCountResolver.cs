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
    public class FavoritesCountResolver : IValueResolver<Article, object, int>
    {
        private readonly IAppDbContext _context;

        public FavoritesCountResolver(IAppDbContext context)
        {
            _context = context;
        }

        public int Resolve(Article source, object destination, int destMember, ResolutionContext context)
        {
            return source.FavoredUsers.Count();
        }
    }
}