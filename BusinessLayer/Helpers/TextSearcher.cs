using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models.PRO;

namespace BusinessLayer.Helpers
{
    internal class TextSearcher : ITextSearcher
    {
        private readonly IAbbreviationKindOfWorkService _abbreviationKind;

        public TextSearcher(IAbbreviationKindOfWorkService abbreviationKind)
        {
            _abbreviationKind = abbreviationKind;
        }

        public string? SearchNumberWithEnd(string args)
        {
            string? result = null;
            var argsTrim = args.Trim();

            for (int index = argsTrim.Length - 1; index >= 0; index--)
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

        public string? SearchNumberWithStart(string args)
        {
            string result = string.Empty;
            var argsTrim = args.Trim();
            //int indexStart = argsTrim.IndexOf(':');
            //argsTrim = argsTrim.Substring(indexStart + 1);

            int index = 0;
            while (index <= argsTrim.Length-1)
            {
                if (char.IsDigit(argsTrim[index]) || argsTrim[index].Equals('.') || argsTrim[index].Equals('-'))
                {
                    index++;
                }
                else
                {
                    if (index != 0)
                    {
                        result = argsTrim.Substring(0, index); 
                        break;
                    }
                    else
                    {
                        argsTrim = argsTrim.Substring(1);
                    }
                    index = 0;
                }

            }
            if (index == argsTrim.Length)
            {
                if (argsTrim.LastIndexOf('.') == argsTrim.Length)
                {
                    argsTrim = argsTrim.Substring(0, argsTrim.Length - 1);
                }
                return argsTrim.Trim();
            }
            else
            {
                if (result.LastIndexOf('.') != -1)
                {
                    result = result.Substring(0, result.Length - 1);
                }
                return result.Trim();
            }
        }

        public AbbreviationKindOfWorkDTO? SearchKindOfWork(string args)
        {
            string? checkStr = null;
            var argsTrim = args.Trim();
            var listKinds = _abbreviationKind.GetAll();
            AbbreviationKindOfWorkDTO abbr = null;
            int index = 0;

            while (index <= argsTrim.Length)
            {
                if (index == argsTrim.Length)
                {
                    abbr = listKinds?.Where(x => x.Name == argsTrim)?.FirstOrDefault();
                    break;
                }


                if (char.IsLetter(argsTrim[index]))
                {
                    index++;
                }
                else
                {
                    if (index != 0)
                    {
                        checkStr = argsTrim.Substring(0, index);
                        abbr = listKinds?.Where(x => x.Name == checkStr)?.FirstOrDefault();

                        if (abbr is not null)
                        {
                            break;
                        }

                        argsTrim = argsTrim.Substring(index + 1);
                    }
                    else
                    {
                        argsTrim = argsTrim.Substring(1);
                    }
                    index = 0;
                }
            }

            return abbr;
        }
    }
}