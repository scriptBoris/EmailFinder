using ContactFinderLib;
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

            if (loader.Results == null || loader.Results.Count == 0)
            {
                Console.WriteLine("Не удалось получить список адресов, на которых надо искать почту.");
                Console.ReadLine();
                return;
            }

            try
            {
                Run(loader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка в работе программы. Причина:\n{ex.Message}\n{ex.InnerException?.Message}");
            }

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static async void Run(Loader loader)
        {
            Console.WriteLine("\nПоиск...");
            foreach (var item in loader.Results)
            {
                var conFinder = new ContactFinder(item);
                await conFinder.GetResult();
            }

            int count = loader.GetCountResult();
            if (count == 0)
            {
                Console.WriteLine("Поиск не дал результатов.");
                return;
            }

            string filePath = Path.Combine(Environment.CurrentDirectory, "EmailFinderResult " + DateTime.Now.ToString("yyyy.M.dd HH.mm") + ".slk" );
            Console.WriteLine($"\n\nПоиск завершен.\nВыгружаю данные в файл: \"{Path.GetFileName(filePath)}\"\n");
            await loader.UploadResult(filePath);
            Console.WriteLine($"Выгрузка завершена. Найденно данных: {count}");
        }
    }
}
