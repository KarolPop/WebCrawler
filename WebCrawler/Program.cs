using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            startCrawlerAsync();
            Console.ReadLine();
        }

        private static async Task startCrawlerAsync() {

            var url = "https://www.rockwool.pl/produkty/";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // a list to add all the list of ROCKWOOL category products with all products
            var productCategories = new List<ProductCategory>();
            var sections =
            htmlDocument.DocumentNode.ChildAttributes("section")
             .Where(node => node.GetAttributeValue("class", "").Equals("O90-product-list")).ToList();

            foreach (var section in sections.rows)
            {

                var productCategory = new ProductCategory
                {

                    MainCategory = section.Descendants("h3").GetAttributeValue("class", "").Equals("O90-product-list__item__content__headline").InnerText,
                    SubCategory = section.Descendants("h3").GetAttributeValue("class", "").Equals("O90-1-product-sublist__category__headline").InnerText,
                    ProductName = section.Descendants("li").GetAttributeValue("class", "").Equals("O90-1-product-sublist__category__section__item").InnerText,
                    ProductUrl = section.Descendants("a").FirstOrDefault().ChildAttributes("src").FirstOrDefault().Value
                };

                productCategories.Add(productCategory);
            }
        }
    }
}
