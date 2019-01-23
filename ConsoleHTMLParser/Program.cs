using ContactFinderLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleHTMLParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EmailFinder is running...\n");
            var loader = new Loader();

            if (loader.Urls == null || loader.Urls.Count == 0)
            {
                Console.WriteLine("Not found internal data.");
                Console.ReadLine();
                return;
            }

            Run(loader);

            Console.ReadLine();
        }

        private static async void Run(Loader loader)
        {
            Console.WriteLine("\nFind...\n");
            foreach (var item in loader.Urls)
            {
                var conFinder = new ContactFinder(item);
                loader.UploadResult(await conFinder.GetResult());
            }
            Console.WriteLine("\nEnd...");
        }
    }
}
