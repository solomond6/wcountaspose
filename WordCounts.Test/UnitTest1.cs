using System;
using Xunit;
using WordCounts.Models;
using RichardSzalay.MockHttp;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordCounts.Test
{
    public class UnitTest1
    {

        [Fact]
        public void TestUrl()
        {
            var newRequest = new WordCountRequest
            {
                url = "https://www.lipsum.com",
                maxNumber = 4,
            };

            Assert.IsType<string>(newRequest.url);
            Assert.IsType<int>(newRequest.maxNumber);

            var mockHttp = new MockHttpMessageHandler();

            var client = mockHttp.ToHttpClient();

            var response = client.GetAsync(newRequest.url);
            response.Wait();
            
            var res = response.Result.ToString();

            Assert.NotNull(res);

        }


        [Fact]
        public static void TestResponse()
        {
            var newRequest = new WordCountRequest
            {
                url = "https://www.lipsum.com",
                maxNumber = 4,
            };

            var mockHttp = new MockHttpMessageHandler();

            var client = mockHttp.ToHttpClient();

            var response = client.GetAsync(newRequest.url);
            response.Wait();

            var res = response.Result.ToString();

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(res);

            var innerText = htmlDocument.DocumentNode.InnerText;

            char[] delimiters = new char[] { ' ', '\r', '\n' };

            var punctuation = innerText.Where(Char.IsPunctuation).Distinct().ToArray();

            Regex rgx = new Regex("[^A-Za-z0-9]");

            var words = innerText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(x => !String.IsNullOrEmpty(x))
                                    .Where(x => !String.IsNullOrWhiteSpace(x))
                                    .Where(x => !rgx.IsMatch(x))
                                    .Select(x => x.Trim(punctuation));



            var duplicates = words.GroupBy(g => g)
                                                    .Where(g => g.Count() <= newRequest.maxNumber)
                                                    .Select(g => new WordCountResponse
                                                    {
                                                        Word = g.Key,
                                                        Count = g.Count(),
                                                        Percentage = (decimal)(g.Count() * 100) / words.Count()
                                                    }).ToList();


            Assert.IsType<WordCountResponse>(duplicates.First());
        }
    }
}
