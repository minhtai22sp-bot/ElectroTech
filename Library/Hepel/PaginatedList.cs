using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
namespace Library
{
    public class PaginatedList<T> : IPagedList
    {
        public bool HasNextPage { get; protected set; }
        public bool HasPreviousPage { get; protected set; }
        public int PageCount { get; private set; }

        public int TotalItemCount { get; private set; }

        public int PageNumber { get; private set; }

        public bool IsFirstPage { get; private set; }

        public bool IsLastPage { get; private set; }

        public int FirstItemOnPage { get; private set; }
        //
        public int LastItemOnPage { get; private set; }
        public int CurrentPage { get; private set; }
        public int From { get; private set; }
        public List<T> Items { get; private set; }
        public int PageSize { get; private set; }
        public int To { get; private set; }
        public decimal TotalAmount { get; set; }


        public PaginatedList(List<T> items, int count, int currentPage, int pageSize)
        {
            if (currentPage < 1)
            {
                throw new ArgumentOutOfRangeException($"pageNumber = {currentPage}. PageNumber cannot be below 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException($"pageSize = {pageSize}. PageSize cannot be less than 1.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"totalItemCount = {count}. TotalItemCount cannot be less than 0.");
            }
            PageSize = pageSize;
            PageNumber = currentPage;
            CurrentPage = currentPage;
            TotalItemCount = count;
            PageCount = TotalItemCount > 0
              ? (int)Math.Ceiling(TotalItemCount / (double)pageSize)
              : 0;
            var pageNumberIsGood = PageCount > 0 && PageNumber <= PageCount;
            IsFirstPage = pageNumberIsGood && PageNumber == 1;
            IsLastPage = pageNumberIsGood && PageNumber == PageCount;
            HasPreviousPage = pageNumberIsGood && PageNumber > 1;
            HasNextPage = pageNumberIsGood && PageNumber < PageCount;
            // PageCount = (int)Math.Ceiling(count / (double)pageSize);


            From = ((currentPage - 1) * pageSize) + 1;
            To = (From + pageSize) - 1;
            Items = items;
            var numberOfFirstItemOnPage = (PageNumber - 1) * PageSize + 1;

            FirstItemOnPage = pageNumberIsGood ? numberOfFirstItemOnPage : 0;

            var numberOfLastItemOnPage = numberOfFirstItemOnPage + PageSize - 1;
            LastItemOnPage = pageNumberIsGood
            ? (numberOfLastItemOnPage > TotalItemCount
                ? TotalItemCount
                : numberOfLastItemOnPage)
            : 0;
        }
        //public bool HasPreviousPage
        //{
        //    get
        //    {
        //        return (CurrentPage > 1);
        //    }
        //}
        //public bool HasNextPage
        //{
        //    get
        //    {
        //        return (CurrentPage < PageCount);
        //    }
        //}



        public static async Task<PaginatedList<T>> ToPagedListAsync(
            IQueryable<T> source, int currentPage, int pageSize, string sortOn = "", string sortDirection = "")
        {
            var count = await source.CountAsync();
            if (!string.IsNullOrEmpty(sortOn))
            {
                if (sortDirection.ToUpper() == "ASC")
                    source = source.OrderBy(sortOn + " " + "ASC");
                else
                    source = source.OrderBy(sortOn + " " + "DESC");
                //source = source.OrderByDescending(sortOn);
            }
            if (currentPage <= 0)
            {
                currentPage = 1;
            }
            source = source.Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
            var lstDt = await source.ToListAsync();
            return new PaginatedList<T>(lstDt, count, currentPage, pageSize);
        }

    }
}
