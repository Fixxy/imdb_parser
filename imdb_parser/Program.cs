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
                Console.WriteLine("No arguments provided.\nSyntax: .exe <start_year> <end_year> <starting_record> <records_per_page>");
            }
            else
            {
                string startYear = args[0];
                string endYear = args[1];
                string startFrom = args[2];
                string rpp = args[3];

                string titleCountPattern = @"of\s(.*?)\stitles";

                //initialize PhantomJS
                PhantomJSDriver driver = new PhantomJSDriver();
                string startURL = "http://www.imdb.com/search/title?year=" + startYear + "," + endYear + "&title_type=feature&sort=moviemeter,asc&count=" + rpp + "&start=" + startFrom;
                driver.Navigate().GoToUrl(startURL);

                //get a number of titles, that we've got
                IWebElement titleCountString = driver.FindElement(By.XPath("//div[@id='main']/div[@class='leftright']/div[@id='left']"));
                int titleCount = Convert.ToInt32((Regex.Match(titleCountString.Text, titleCountPattern)).Groups[1].Value.Replace(",", ""));
                Console.WriteLine("\r\n#titleCount:{0}", titleCount);

                //go through every title on the current page
                for (int i = 1; i <= Convert.ToInt32(rpp); i++)
                {
                    IWebElement number = driver.FindElement(By.XPath("//table[@class='results']/tbody[1]/tr[" + (i + 1) + "]/td[@class='number']"));
                    IWebElement title = driver.FindElement(By.XPath("//table[@class='results']/tbody[1]/tr[" + (i + 1) + "]/td[@class='title']/a[1]"));
                    string movieURL = title.GetAttribute("href");
                    Console.WriteLine("\r\n[{0}] Opening url: {1}", number.Text.Replace(".",""), movieURL);
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
                IWebElement title = driver.FindElement(By.XPath("//table[@id='title-overview-widget-layout']/tbody[1]/tr[1]/td[@id='overview-top']/h1[@class='header']/span[@class='title-extra']"));
                Match origTitle = Regex.Match(title.Text, "\"(.*?)\"");
                Console.WriteLine("Title:{0}", origTitle.Groups[1]);
            }
            catch
            {
                IWebElement title = driver.FindElement(By.XPath("//table[@id='title-overview-widget-layout']/tbody[1]/tr[1]/td[@id='overview-top']/h1[@class='header']/span[@class='itemprop']"));
                Console.WriteLine("Title:{0}", title.Text);
            }

            //description
            IWebElement description = driver.FindElement(By.XPath("//td[@id='overview-top']/p[@itemprop='description']"));
            Console.WriteLine("Description:{0}", description.Text);

            //director
            IWebElement director = driver.FindElement(By.XPath("//td[@id='overview-top']/div[@itemprop='director']/a[1]/span[1]"));
            IWebElement directorURL = driver.FindElement(By.XPath("//td[@id='overview-top']/div[@itemprop='director']/a[1]"));

            Match directorID = Regex.Match(directorURL.GetAttribute("href"), @"\/name\/(.*?)\/");

            Console.WriteLine("Director:{0} (id:{1})", director.Text, directorID.Groups[1]);
        }
    }
}
