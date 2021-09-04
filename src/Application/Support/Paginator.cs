using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Support
{
    public class PagedQuery
    {
        public const int MaxLimit = 20;

        public int _limit = MaxLimit;

        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _limit = value > MaxLimit ? MaxLimit : value;
            }
        }

        public int Offset { get; set; } = 0;
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int Total { get; set; } = 0;
    }
}