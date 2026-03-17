namespace BusinessLayer.Models.KDO
{
    public class PageViewModel
    {
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public int Count { get; private set; }

        public PageViewModel(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Count = count;
        }

        public bool HasPreviousPage
        {
            get
            {
                return PageNumber > 1;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return PageNumber < TotalPages;
            }
        }
    }
}
