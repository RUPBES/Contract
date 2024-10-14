using BusinessLayer.Interfaces.CommonInterfaces;

namespace BusinessLayer.Helpers
{
    internal class TextSearcher: ITextSearcher
    {
        public string? FindNumberWithEnd(string args)
        {
            string? result = null;            
            var argsTrim = args.Trim();

            for (int index = argsTrim.Length-1; index >= 0 ; index--)
            {
                //проверяем если это не число и не пробел (на случай если тыячные/сотые/.. разряды разделены пробелом), заканчиваем поиск номера
                if (!char.IsNumber(argsTrim[index]) && !char.IsWhiteSpace(argsTrim[index])) 
                {
                    if (index != argsTrim.Length)
                    {
                        result = argsTrim.Substring(index+1);
                    }
                    break;
                }
            }            

            return result?.Trim();
        }

        public string? FindNumberWithStart(string args)
        {
            string? result = null;
            var argsTrim = args.Trim();

            for (int index = 0; index <= argsTrim.Length - 1; index++)
            {
                //проверяем если это не число и не пробел (на случай если тыячные/сотые/.. разряды разделены пробелом), заканчиваем поиск номера
                if (!char.IsNumber(argsTrim[index]) && !char.IsWhiteSpace(argsTrim[index]))
                {
                    if (index != argsTrim.Length)
                    {
                        result = argsTrim.Substring(index + 1);
                    }
                    break;
                }
            }

            return result?.Trim();
        }
    }
}