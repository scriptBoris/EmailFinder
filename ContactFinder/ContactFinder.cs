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
        private string url;

        private const string emailPattern =
               @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})";

        private const string pattern =
                "(" +
                    @"(?<withsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?=\?)" +
                ")" +
                    "|" +
                    @"(?<withoutsubject>" + @"(?<=mailto\:)" + emailPattern + @"(?="")" + ")" +
                ")";

        public ContactFinder(string url)
        {
            this.url = url;
        }

        private List<string> GetEmails(string target)
        {
            var result = new List<string>();
            var regex = new Regex(pattern);
            var matches = regex.Matches(target);

            if (matches.Count > 0)
            {
                foreach (var item in matches)
                    result.Add(item.ToString());
            }
            return result;
        }

        public async Task<List<string>> GetResult()
        {
            var cl = new HttpClient();
            var responseHtml = await cl.GetAsync(url);
            string response = await responseHtml.Content.ReadAsStringAsync();

            //string response = "<a href=\"http://dota2.ru/contact-us\"></a>";
            //string response = "<a href=\"https://rusdefense.ru/contact-us/\"><i class=\"fa fa-phone\"></i></a> <span class=\"hidden-xs hidden-sm hidden-md";

            var list = GetEmails(response);
            if (list.Count == 0)
            {
                const string refPattern = "<a.*?href=\"(.*?(.*(contact|about|kontakti).*))\".*?>";
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
                        string url = split[0];

                        //list.Add(url);
                        //return list;

                        responseHtml = await cl.GetAsync(url);
                        response = await responseHtml.Content.ReadAsStringAsync();

                        foreach (var em in GetEmails(response))
                            list.Add(em);
                    }
                }
            }
            
            // Remove duplicate strings
            for(int i = 0; i<= list.Count - 1; i++)
            {
                string s = list[i];
                Console.WriteLine(s);
                var match = list.FirstOrDefault(x => s.Equals(x, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    list.Remove(list[i]);
            }

            return list;
        }
    }
}
