using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Collections.Async;
using System.Collections.Concurrent;

namespace WebCrawler
{
    class Program
    {
        static BlockingCollection<ProductLog> Logs = new BlockingCollection<ProductLog>();
        static string FileWritePath = @"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\WriteText.csv";
        static void Main(string[] args)
        {
            startCrawlerAsync().Wait();
            Console.ReadLine();
        }

        private static string ClearString(string val)
        {
            string replacement = Regex.Replace(val, @"\t|\n|\r", "");
            return replacement;
        }

        private static async Task startCrawlerAsync()
        {
            string[] brandsUrls = File.ReadAllLines(@"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\rw_sites.txt");

            var tasks = new Task[] {
                Task.Run(() => CheckSites(brandsUrls)),
                Task.Run(() => LogStatuses())
            };

            Task.WaitAll();

        }

        private static void LogStatuses()
        {
            foreach(var log in Logs.GetConsumingEnumerable())
            {
                System.IO.File.AppendAllText(FileWritePath, $"{log.BrandUrl},{log.Name},{log.Status}{Environment.NewLine}");
            }
        }

        private static async Task CheckSites(string[] brandsUrls)
        {
            foreach (string brandUrl in brandsUrls)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                await CheckProductsForBrand(brandUrl);

                sw.Stop();
                Console.WriteLine($"Done {sw.Elapsed}");

                Logs.CompleteAdding();
            }
        }

        private static async Task CheckProductsForBrand(string brandUrl)
        {
            int index = brandUrl.IndexOf('/', 10);
            string origin = brandUrl.Substring(0, index);
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(brandUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            List<Product> products =
            htmlDocument.DocumentNode.Descendants("li")
                .AsParallel()
                .Where(node => node.GetAttributeValue("class", "").Equals("O90-1-product-sublist__category__section__item") && node.InnerHtml != null)
                .Select(x => MapToProduct(x, origin))
                .DistinctBy(x => x.Url).ToList();

            System.IO.File.WriteAllText(FileWritePath, $"Brand url,Product Name,Status{Environment.NewLine}");


            await products.ParallelForEachAsync(async product =>
            {
                var productResponse = await httpClient.GetAsync(product.Url);
                Console.WriteLine($"{product.Name} - {productResponse.StatusCode}");
                Logs.Add(new ProductLog
                {
                    Name = product.Name,
                    Status = productResponse?.StatusCode.ToString(),
                    BrandUrl = brandUrl
                });
            });
        }

        private static Product MapToProduct(HtmlNode node, string origin)
        {
            var a = node.ChildNodes["a"];
            string productName = ClearString(node.InnerText);
            var uri = new Uri($"{origin}{a.Attributes["href"].Value}");

            return new Product
            {
                Name = productName,
                Url = uri
            };
        }
    }
}
