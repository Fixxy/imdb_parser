using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;

namespace imdb_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided.\nSyntax: .exe <start_year> <end_year> <starting_record> <genres> <records_per_page>");
                Console.WriteLine("\r\nGenres: all - all genres;");
                Console.WriteLine("        specific - action,adventure,animation,biography,comedy,crime,documentary,");
                Console.WriteLine("        drama,family,fantasy,film_noir,game_show,history,horror,music,musical,");
                Console.WriteLine("        mystery,news,reality_tv,romance,sci_fi,sport,talk_show,thriller,war,western");
            }
            else
            {
                string startYear = args[0];
                string endYear = args[1];
                string startFrom = args[2];
                string genres = args[3].Trim();
                string genresURL = "";
                string rpp = args[4];

                if (genres != "all") { genresURL = "&genres=" + genres; }

                //regex
                string titleCountPattern = @"of\s(.*?)\stitles";

                //initialize PhantomJS
                Console.WriteLine("Initializing PhantomJS");
                var service = PhantomJSDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                PhantomJSDriver driver = new PhantomJSDriver(service);
                Console.WriteLine("Opening IMDb.com");
                string startURL = "http://www.imdb.com/search/title?year=" + startYear + "," + endYear + "&title_type=feature&sort=moviemeter,asc&count=" + rpp + "&start=" + startFrom + "" + genresURL;
                driver.Navigate().GoToUrl(startURL);

                //get a number of titles, that we've got
                IWebElement titleCountString = driver.FindElement(By.XPath("//div[@class='nav']/div[@class='desc']"));

                int titleCount = Convert.ToInt32((Regex.Match(titleCountString.Text, titleCountPattern)).Groups[1].Value.Replace(" ", ""));
                Console.WriteLine("Found {0} records", titleCount);

                //go through every title on the current page
                for (int i = 0; i < Convert.ToInt32(rpp); i++)
                {
                    IWebElement title = driver.FindElement(By.XPath("//div[@class='lister-list']/div[@class='lister-item mode-advanced'][" + (i + 1) + "]/div[@class='lister-item-content']/h3[@class='lister-item-header']/a[1]"));

                    string movieURL = title.GetAttribute("href");
                    Console.WriteLine("=============================");
                    Console.WriteLine("Opening url: {0}", movieURL);
                    driver.Navigate().GoToUrl(movieURL);
                    getMovieInfo(driver);
                    driver.Navigate().Back();
                }
                driver.Close();
                driver.Quit();
            }
        }

        //getting movie info
        static void getMovieInfo(PhantomJSDriver driver)
        {
            //title
            try
            {
                IWebElement title = driver.FindElement(By.XPath("//div[@class='originalTitle']"));
                Match origTitle = Regex.Match(title.Text, @"(.*?)\s\((.*?)");
                Console.WriteLine("Title: {0}", origTitle.Groups[1]);
            }
            catch
            {
                IWebElement title = driver.FindElement(By.XPath("//div[@class='title_wrapper']/h1[1]"));
                Console.WriteLine("Title: {0}", title.Text);
            }

            //description
            IWebElement description = driver.FindElement(By.XPath("//div[@class='plot_summary ']/div[@class='summary_text']"));
            Console.WriteLine("Description: {0}", description.Text);

            //first few stars
            var briefStars = driver.FindElementsByXPath("//div[@class='credit_summary_item']/span[@itemprop='actors']/a");
            foreach (IWebElement star in briefStars)
            {
                Console.WriteLine("- star: {0} ({1})", star.Text, star.GetAttribute("href"));
            }

            //todo: director
        }
    }
}
