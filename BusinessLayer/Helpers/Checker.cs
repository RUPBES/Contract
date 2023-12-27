using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Helpers
{
    public class Checker
    {
        public Checker() { }

        public static bool LessOrEquallyFirstDateByMonth(DateTime date1, DateTime date2)
        {
            if (date1.Year < date2.Year)
                return true;
            else if (date1.Year > date2.Year)
                return false;
            else if (date1.Year == date2.Year && date1.Month <= date2.Month)
                return true;
            else
                return false;
        }

        public static bool LessFirstDateByMonth(DateTime date1, DateTime date2)
        {
            if (date1.Year < date2.Year)
                return true;
            else if (date1.Year > date2.Year)
                return false;
            else if (date1.Year == date2.Year && date1.Month < date2.Month)
                return true;
            else
                return false;
        }
    }
}
