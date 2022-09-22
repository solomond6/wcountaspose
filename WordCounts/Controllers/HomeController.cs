using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WordCounts.Models;

namespace WordCounts.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
           
            return View();
        }


        [HttpPost]
        public IActionResult Index(WordCountRequest request)
        {
            try
            {
                var httpClient = new HttpClient();
                var html = httpClient.GetStringAsync(request.url);

                html.Wait();
                var res = html.Result;

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


                IList<WordCountResponse> duplicates = words.GroupBy(g => g)
                                                        .Where(g => g.Count() <= request.maxNumber)
                                                        .Select(g => new WordCountResponse
                                                        {
                                                            Word = g.Key,
                                                            Count = g.Count(),
                                                            Percentage = (decimal)(g.Count()* 100)/words.Count()
                                                        }).ToList();

                ViewBag.Response = duplicates;
                ViewBag.MAxNumber = request.maxNumber;
            }
            catch (Exception ex)
            {
                ViewBag.Response = null;
            }
            
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
