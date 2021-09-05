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

        private int _limit = MaxLimit;

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

        public int Offset { get; set; }
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int Total { get; set; }
    }
}
