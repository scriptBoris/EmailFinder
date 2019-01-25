using EmailFinderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactFinderLib
{
    public class Loader
    {
        private const string config = "EmailFinder.ini";

        public List<FoundResult> Results { get; private set; }

        public Loader()
        {
            Results = new List<FoundResult>();
            if (File.Exists(config))
            {
                var file = new StreamReader(config);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    // Если строка пустая, то не добавлять её в список URLs
                    if (string.IsNullOrWhiteSpace(line) == true)
                        continue;

                    var split = line.Split(';');
                    if (split.Length < 2)
                    {
                        ConsoleEx.WriteError($"{line}:","ошибка загрузки.");
                        continue;
                    }

                    string source = split[0];
                    // Чистка URL От пробелов
                    string url = split[1].Replace(" ","");


                    Console.WriteLine($"загрузка: {source} - {url}");
                    Results.Add(new FoundResult
                    {
                        Source = source,
                        Url = url,
                    });
                }
                file.Close();
            }
            else
            {
                Console.WriteLine($"Не удалось найти файл: {config}");
            }
        }

        public int GetCountResult()
        {
            int count = 0;
            foreach(var item in Results)
                count += item.Emails.Count;

            return count;
        }

        //public void LoadResult(EmailResult emailRes)
        //{
        //    if (emailRes == null)
        //        return;

        //    foreach (var item in emailRes)
        //       Results.Add(item);
        //}

        public async Task UploadResult(string filePath)
        {
            int counter = 1;
            using (var sw = File.CreateText(filePath))
            {
                await sw.WriteLineAsync("ID;P");
                await sw.WriteLineAsync($"C;Y1;X1;K\"#\""); // номер
                await sw.WriteLineAsync($"C;Y1;X2;K\"Company\""); // название комании
                await sw.WriteLineAsync($"C;Y1;X3;K\"Target\""); // сайт
                await sw.WriteLineAsync($"C;Y1;X4;K\"Result\""); // найденные Емейлы
                await sw.WriteLineAsync($"C;Y1;X5;K\"Source\""); // адрес HTML страницы на которой удалось найти
                foreach (var result in Results)
                {
                    if (result.Status == Status.OK)
                    {
                        foreach (var email in result.Emails)
                        {
                            counter++;
                            await Write(sw, counter, result, email);
                        }
                    }
                    else
                    {
                        counter++;
                        if (result.Status == Status.BadUrl)
                        {
                            await Write(sw, counter, result, "BAD URL");
                        }
                        else if (result.Status == Status.NotConnect)
                        {
                            await Write(sw, counter, result, "NOT CONNECT");
                        }
                        else if (result.Status == Status.NotEmails)
                        {
                            await Write(sw, counter, result, "NO EMAIL");
                        }
                    }
                }
                await sw.WriteLineAsync("E");
            }
        }

        private async Task Write(StreamWriter sw, int counter, FoundResult found, string value)
        {
            await sw.WriteLineAsync($"C;Y{counter};X1;K\"{counter-1})\"");
            await sw.WriteLineAsync($"C;Y{counter};X2;K\"{found.Source}\"");
            await sw.WriteLineAsync($"C;Y{counter};X3;K\"{found.Url}\"");
            await sw.WriteLineAsync($"C;Y{counter};X4;K\"{value}\"");
        }

        private async Task Write(StreamWriter sw, int counter, FoundResult found, EmailResult mail)
        {
            await sw.WriteLineAsync($"C;Y{counter};X1;K\"{counter - 1})\"");
            await sw.WriteLineAsync($"C;Y{counter};X2;K\"{found.Source}\"");
            await sw.WriteLineAsync($"C;Y{counter};X3;K\"{found.Url}\"");
            await sw.WriteLineAsync($"C;Y{counter};X4;K\"{mail.Email}\"");
            await sw.WriteLineAsync($"C;Y{counter};X5;K\"{mail.GotSource}\"");
        }
    }
}
