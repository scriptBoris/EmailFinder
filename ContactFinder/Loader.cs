﻿using System;
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
        public List<string> Urls { get; private set; }
        public List<string> Emails { get; private set; }

        public Loader()
        {
            Emails = new List<string>();
            if (File.Exists(config))
            {
                Urls = new List<string>();
                var file = new StreamReader(config);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    // Если строка пустая, то не добавлять её в список URLs
                    if (string.IsNullOrWhiteSpace(line) == true)
                        continue;

                    Console.WriteLine($"загрузка: {line}");
                    Urls.Add(line);
                }
                file.Close();
                //var body = file.ReadToEndAsync();
            }
            else
            {
                Console.WriteLine($"Не удалось найти файл: {config}");
            }
        }

        public void LoadResult(List<string> emails)
        {
            if (emails == null)
                return;

            foreach (var item in emails)
               Emails.Add(item);
        }

        public async Task UploadResult(string filePath)
        {
            using(var sw = File.CreateText(filePath))
            {
                foreach (var s in Emails)
                {
                    await sw.WriteLineAsync(s);
                }
            }
        }
    }
}
