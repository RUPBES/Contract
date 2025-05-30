namespace BusinessLayer.Helpers
{
    public class DateComparer
    {
        public DateComparer() { }

        private static int CompareYearMonth(DateTime firstDate, DateTime secondDate)
        {
            if (firstDate.Year < secondDate.Year)
                return -1;
            else if (firstDate.Year > secondDate.Year)
                return 1;
            else
                return firstDate.Month.CompareTo(secondDate.Month);
        }

        /// <summary>
        /// Сравнивает первую дату с второй по году и месяцу, если первая меньше или равна второй
        /// возвращает true, иначе false
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns>если меньше первая дата или равна второй возвращает true, иначе false</returns>
        public static bool IsLessOrSameYearAndMonth(DateTime? firstDate, DateTime? secondDate)
        {
            if (!firstDate.HasValue || !secondDate.HasValue)
            {
                return false;
            }
            return CompareYearMonth(firstDate.Value, secondDate.Value) <= 0;
        }


        /// <summary>
        /// Сравнивает первую дату с второй по году и месяцу, если первая меньше второй
        /// возвращает true, иначе false
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns></returns>
        public static bool IsLessYearAndMonth(DateTime? firstDate, DateTime? secondDate)
        {
            if (!firstDate.HasValue || !secondDate.HasValue)
            {
                return false;
            }
            return CompareYearMonth(firstDate.Value, secondDate.Value) < 0;
        }


        /// <summary>
        /// Сравнивает первую дату с второй по году и месяцу, если первая равна второй
        /// возвращает true, иначе false
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns></returns>
        public static bool IsSameYearAndMonth(DateTime? firstDate, DateTime? secondDate)
        {
            if (!firstDate.HasValue || !secondDate.HasValue)
            {
                return false;
            }
            return CompareYearMonth(firstDate.Value, secondDate.Value) == 0;
        }
    }
}
