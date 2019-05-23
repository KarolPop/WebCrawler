using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace WebCrawler
{
    class Program
    {
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
                string[] urls = File.ReadAllLines(@"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\rw_sites.txt");


            //var httpClient = new HttpClient();

            foreach (string value in urls)
                {

                    int index = value.IndexOf('/', 10);
                    string origin = value.Substring(0, index);
                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync(value);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);


                    //a list to add all the list of ROCKWOOL category products with all products
                    var productCategories = new List<ProductCategory>();
                    List<HtmlNode> productSections =
                    htmlDocument.DocumentNode.Descendants("section")
                        .Where(node => node.GetAttributeValue("class", "").Equals("O90-product-list")).ToList();

                    System.IO.File.WriteAllText(@"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\WriteText.txt", "");
                

                    foreach (HtmlNode section in productSections)
                    {
                        List<HtmlNode> categoryNames = section.Descendants("h3").ToList();
                        foreach (HtmlNode categoryName in categoryNames)
                        {
                            string category = categoryName.InnerText;
                            Console.WriteLine("Product category name :" + ClearString(category) + $" site:{origin}");
                            System.IO.File.AppendAllText(@"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\WriteText.txt", ClearString(category) + Environment.NewLine);


                            List<HtmlNode> products = section.Descendants("li").ToList();
                            foreach (HtmlNode product in products)
                            {
                                if (String.IsNullOrWhiteSpace(product.InnerText))
                                    continue;

                                var a = product.ChildNodes["a"];
                                string productName = product.InnerText;
                                var uri = new Uri($"{origin}{a.Attributes["href"].Value}");
                                var productResponse = await httpClient.GetAsync(uri);

                                //if (productResponse.IsSuccessStatusCode == false)
                                //{
                                //log unsuccessful request to product
                                Console.WriteLine ($"{ClearString(productName)} ({uri}): {productResponse.StatusCode}: {DateTime.Now:yyyy-MM-dd hh-mm-ss}");
                                //}

                                //Console.WriteLine("Product name :" + ClearString(productName));
                                System.IO.File.AppendAllText(@"C:\Users\karpo\OneDrive - ROCKWOOL Group\Desktop\WriteText.txt", ClearString(productName) + Environment.NewLine);
                            }
                        }
                
                        // var catName = section.Descendants("h3").Single().GetAttributeValue()

                        /*
                        ProductCategory productCategory = new ProductCategory
                        {

                            MainCategory = section.Descendants("h3").FirstOrDefault()?.InnerText,
                            SubCategory = section.Descendants("h3").FirstOrDefault()?.InnerText,//.GetAttributeValue("class", "").Equals("O90-1-product-sublist__category__headline").InnerText,
                            ProductName = section.Descendants("li").FirstOrDefault()?.InnerText,//.GetAttributeValue("class", "").Equals("O90-1-product-sublist__category__section__item").InnerText,
                            ProductUrl = section.Descendants("a").FirstOrDefault()?.ChildAttributes("href").FirstOrDefault().Value
                        };
               
                        productCategories.Add(productCategory);
                        */

                        var pr = await httpClient.GetAsync(new Uri($"https://www.rockwool.pl/dupa"));

                        //if (productResponse.IsSuccessStatusCode == false)
                        //{
                        //log unsuccessful request to product
                        Console.WriteLine($"Failed: {pr.StatusCode}");
                    }
            }
        }
    }
}
