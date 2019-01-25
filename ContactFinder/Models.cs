using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailFinderLib
{
    public class FoundResult
    {
        /// <summary>
        /// Название фирмы сайта на котором происходит поиск
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// URL сайт на котором нужно искать почту
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Список добытых почт
        /// </summary>
        public List<EmailResult> Emails { get; set; } = new List<EmailResult>();

        /// <summary>
        /// Статус
        /// </summary>
        public Status Status { get; set; }
    }

    public class EmailResult
    {
        
        /// <summary>
        /// Добытая почта
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Источник URL откуда был добыта почта
        /// </summary>
        public string GotSource { get; set; }

        /// <summary>
        /// Каким методом была добыта почта
        /// </summary>
        public Method Method { get; set; }

        /// <summary>
        /// Статус результата
        ///// </summary>
        //public Status Status { get; set; }

        /// <summary>
        /// Количество иммерсий
        /// </summary>
        public int ImmersionCount { get; set; }
    }

    public enum Status
    {
        OK,
        BadUrl,
        NotConnect,
        NotEmails,
    }

    public enum Method
    {
        ByTag,
        ByParse
    }
}
