using EmailFinderLib;
using Flurl;
using System;
using System.Collections.Generic;
using System.IO;
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

        // Parse patern mailto:
        private const string emailPattern =
               @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})";

        private const string pattern =
                "(" +
                    @"(?<withsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?=\?)" +
                ")" +
                    "|" +
                    @"(?<withoutsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?="")" + ")" +
                ")";

        // Parse patern email
        const string patternEmailParse =
           @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
             + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";

        public ContactFinder(FoundResult emailResult)
        {
            dataResult = emailResult;
        }

        private List<EmailResult> GetEmails(string targetHTML, string addresHTML)
        {
            var tempList = new List<EmailResult>();

            // parse mailto
            var regex = new Regex(pattern);
            var matches = regex.Matches(targetHTML);
            foreach (var item in matches)
                tempList.Add(new EmailResult
                {
                    Email = item.ToString(),
                    GotSource = addresHTML,
                    Method = Method.ByTag,
                });

            // parse email
            regex = new Regex(patternEmailParse);
            matches = regex.Matches(targetHTML);
            foreach (var item in matches)
                tempList.Add(new EmailResult
                {
                    Email = item.ToString(),
                    GotSource = addresHTML,
                    Method = Method.ByParse,
                });

            // Filter
            var result = new List<EmailResult>();
            foreach (var item in tempList)
            {
                if (item.Email.Equals("Rating@Mail.ru", StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(item.Email, @"\p{IsCyrillic}"))
                    continue;

                result.Add(item);
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

            // test
            //response = "<nobr><a href=\"/about/index.php\"style=\"text-decoration:none;\">";
            Console.WriteLine($"{url}: парсинг HTML");
            dataResult.Emails = GetEmails(response, url);
            if (dataResult.Emails.Count == 0)
            {
                //const string refPattern = "<a.*?href=\"(.*?(contact|about|kontakti|kontakty))\".*?>";
                const string refPattern = "<a.*?href\\s*=\\s*[\"\'].*?(about|contact|kontakti|kontakty|kompanii).*?([^\"\'>]+)[\"\']*>";
                var reg = new Regex(refPattern, RegexOptions.IgnoreCase);
                var mat = reg.Matches(response);

                if (mat.Count > 0)
                {
                    foreach (var item in mat)
                    {
                        string urlImmersive;
                        var urlReg = new Regex("http.*");
                        var urlMat = urlReg.Match(item.ToString());
                        if (urlMat.Success == true)
                        {
                            var split = urlMat.Value.Split('"');
                            urlImmersive = split[0];
                        }
                        else
                        {
                            urlReg = new Regex("/{1}.*");
                            urlMat = urlReg.Match(item.ToString());
                            if (urlMat.Success == false)
                                continue;

                            var split = urlMat.Value.Split('"');
                            urlImmersive = split[0];
                            urlImmersive = Url.Combine(url, urlImmersive);
                        }

                        //list.Add(url);
                        //return list;

                        try
                        {
                            Console.WriteLine($"{urlImmersive} погружение");
                            var responseHtml = await cl.GetAsync(urlImmersive);
                            response = await responseHtml.Content.ReadAsStringAsync();
                            Console.WriteLine($"{urlImmersive}: парсинг HTML");
                            var listImmersive = GetEmails(response, urlImmersive);

                            foreach (var em in listImmersive)
                                dataResult.Emails.Add(em);
                        }
                        catch (Exception ex)
                        {
                            ConsoleEx.WriteError($"{urlImmersive}:", $"ошибка. Причина - {ex.Message}");
                        }
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
            for(int i = 0; i <= list.Count -1; i++)
            {
                var email = list[i];

                var match = list.FindAll(x => email.Email.Equals(x.Email, StringComparison.OrdinalIgnoreCase));
                if (match.Count > 1)
                {
                    list.RemoveAt(i);
                    i = 0;
                }
            }

            // Output on console
            foreach(var item in list)
            {
                ConsoleEx.WriteEmail($"{url}: найден", $"{item.Email}");
            }

            dataResult.Status = Status.OK;
            return dataResult;
        }
    }
}
