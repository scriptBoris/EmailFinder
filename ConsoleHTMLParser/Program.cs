﻿using ContactFinderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleHTMLParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Программа запущена...\n");
            var loader = new Loader();

            if (loader.Urls == null || loader.Urls.Count == 0)
            {
                Console.WriteLine("Не удалось получить список адресов, на которых надо искать почту.");
                Console.ReadLine();
                return;
            }

            Run(loader);

            Console.ReadLine();
        }

        private static async void Run(Loader loader)
        {
            Console.WriteLine("\nПоиск...\n\n");
            foreach (var item in loader.Urls)
            {
                var conFinder = new ContactFinder(item);
                loader.LoadResult(await conFinder.GetResult());
            }

            if (loader.Emails.Count == 0)
            {
                Console.WriteLine("Поиск не дал результатов.");
                return;
            }

            string filePath = Path.Combine(Environment.CurrentDirectory, "EmailFinderResult " + DateTime.Now.ToString("yyyy.M.dd HH.mm") + ".txt" );
            Console.WriteLine($"\n\nПоиск завершен.\nВыгружаю данные в файл: \"{Path.GetFileName(filePath)}\"\n");
            await loader.UploadResult(filePath);
            Console.WriteLine($"Выгрузка завершена.");
        }
    }
}
