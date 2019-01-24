using EmailFinderLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContactFinderLib
{
    public class ContactFinder
    {
        private FoundResult dataResult;

        private const string emailPattern =
               @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})";

        private const string pattern =
                "(" +
                    @"(?<withsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?=\?)" +
                ")" +
                    "|" +
                    @"(?<withoutsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?="")" + ")" +
                ")";

        public ContactFinder(FoundResult emailResult)
        {
            dataResult = emailResult;
        }

        private List<EmailResult> GetEmails(string targetHTML, string addresHTML)
        {
            var result = new List<EmailResult>();
            var regex = new Regex(pattern);
            var matches = regex.Matches(targetHTML);

            if (matches.Count > 0)
            {
                foreach (var item in matches)
                    result.Add(new EmailResult
                    {
                        Email = item.ToString(),
                        Source = addresHTML
                    });
            }
            return result;
        }

        public async Task<FoundResult> GetResult()
        {
            var cl = new HttpClient();
            string url = dataResult.Url;
            string response = "";

            try
            {
                Console.WriteLine($"{url}: подключение");
                var responseHtml = await cl.GetAsync(url);
                response = await responseHtml.Content.ReadAsStringAsync();
                Console.WriteLine($"{url}: соединение установлено");
            }
            catch (Exception)
            {
                Console.WriteLine($"{url}: не правильный url");
                dataResult.Status = Status.BadUrl;
                return dataResult;
            }

            //string response = "<a href=\"http://dota2.ru/contact-us\"></a>";
            //string response = "<a href=\"https://rusdefense.ru/contact-us/\"><i class=\"fa fa-phone\"></i></a> <span class=\"hidden-xs hidden-sm hidden-md";
            Console.WriteLine($"{url}: парсинг HTML");
            dataResult.Emails = GetEmails(response, url);
            if (dataResult.Emails.Count == 0)
            {
                const string refPattern = "<a.*?href=\"(.*?(.*(contact|about|kontakti|kontakty).*))\".*?>";
                var reg = new Regex(refPattern);
                var mat = reg.Matches(response);

                if (mat.Count > 0)
                {
                    foreach (var item in mat)
                    {
                        var urlReg = new Regex("http.*");
                        var urlMat = urlReg.Match(item.ToString());
                        if (urlMat.Success == false)
                            continue;
                        var split = urlMat.Value.Split('"');
                        string urlImmersive = split[0];

                        //list.Add(url);
                        //return list;

                        var responseHtml = await cl.GetAsync(urlImmersive);
                        response = await responseHtml.Content.ReadAsStringAsync();

                        foreach (var em in GetEmails(response, urlImmersive))
                            dataResult.Emails.Add(em);
                    }
                }
            }
            
            if (dataResult.Emails.Count == 0)
            {
                dataResult.Status = Status.NotEmails;
                Console.WriteLine($"{url}: не найден ни один email");

                return dataResult;
            }

            // Remove duplicate strings
            var list = dataResult.Emails;
            while (true)
            {
                var email = list.First();
                var match = list.FindAll(x => email.Email.Equals(x.Email, StringComparison.OrdinalIgnoreCase));
                if (match.Count > 1)
                    dataResult.Emails.Remove(email);
                else
                    break;
            }

            foreach(var item in dataResult.Emails)
            {
                Console.WriteLine($"{url}: {item.Email}");
            }

            dataResult.Status = Status.OK;
            return dataResult;
        }
    }
}
