using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailFinderLib
{
    public class FoundResult
    {
        public string Url { get; set; }
        public List<EmailResult> Emails { get; set; } = new List<EmailResult>();
        public Status Status { get; set; }
    }

    public class EmailResult
    {
        public string Email { get; set; }
        public string Source { get; set; }
    }

    public enum Status
    {
        OK,
        BadUrl,
        NotConnect,
        NotEmails,
    }
}
