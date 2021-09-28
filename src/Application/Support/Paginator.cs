using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Support
{
    public class PagedQuery
    {
        public const int MaxLimit = 20;

        private int _limit = MaxLimit;

        /// <summary>
        /// Limit number of articles returned (default is 20)
        /// </summary>
        [DefaultValue(20)]
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

        /// <summary>
        /// Offset/skip number of articles (default is 0)
        /// </summary>
        [DefaultValue(0)]
        public int Offset { get; set; }
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int Total { get; set; }
    }
}
