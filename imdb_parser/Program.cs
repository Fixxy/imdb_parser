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
                string genresURL = "";
                string genres = args[3].Trim();
                if (genres != "all") { genresURL = "&genres=" + genres; }
                string rpp = args[4];

                //initialize PhantomJS
                Console.WriteLine("Initializing PhantomJS");

                var service = PhantomJSDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                PhantomJSDriver driver = new PhantomJSDriver(service);

                Console.WriteLine("Opening IMDb.com");
                string startURL = "http://www.imdb.com/search/title?year=" + startYear + "," + endYear + "&title_type=feature&sort=moviemeter,asc&count=" + rpp + "&start=" + startFrom + "" + genresURL;
                Console.WriteLine(startURL);
                driver.Navigate().GoToUrl(startURL);

                //get a number of titles, that we've got
                IWebElement titleCountString = driver.FindElement(By.XPath("//div[@class='nav']/div[@class='desc']"));

                int titleCount = Convert.ToInt32((Regex.Match(titleCountString.Text, @"of\s(.*?)\stitles")).Groups[1].Value.Replace(" ", ""));
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
                    driver.Navigate().GoToUrl(startURL);
                }
                driver.Close();
                driver.Quit();
            }
        }

        //getting movie info
        static void getMovieInfo(PhantomJSDriver driver)
        {
            //global id
            string movieID = Regex.Match(driver.Url, @"imdb.com\/title\/(.*?)\/\?").Groups[1].Value;
            Console.WriteLine("ID: {0}", movieID);

            //title (original title is a priority)
            string title;
            try
            {
                IWebElement titleSource = driver.FindElement(By.XPath("//div[@class='originalTitle']"));
                Match origTitle = Regex.Match(titleSource.Text, @"(.*?)\s\((.*?)");
                Console.WriteLine("Title: {0}", origTitle.Groups[1]);
                title = origTitle.Groups[1].Value;
            }
            catch
            {
                IWebElement titleSource = driver.FindElement(By.XPath("//div[@class='title_wrapper']/h1[1]"));
                Console.WriteLine("Title: {0}", titleSource.Text);
                title = titleSource.Text;
            }

            //year
            string year = driver.FindElementByXPath("//span[@id='titleYear']").Text;
            Console.WriteLine("Year: {0}", year);

            //genres
            List<string> genresList = new List<string>();
            var genres = driver.FindElementsByXPath("//span[@itemprop='genre']");
            foreach (IWebElement genre in genres)
            {
                genresList.Add(genre.Text);
                Console.WriteLine("Genre: {0}", genre.Text);
            }

            //description 
            string description = driver.FindElement(By.XPath("//div[contains(@class,'plot_summary')]/div[@class='summary_text']")).Text;
            Console.WriteLine("Description: {0}", description);

            //directors
            Dictionary<string, string> directorsList = new Dictionary<string, string>();
            var directors = driver.FindElementsByXPath("//div[@class='credit_summary_item']/span[@itemprop='director']/a");
            foreach (IWebElement director in directors)
            {
                string directorID = (Regex.Match(director.GetAttribute("href"), @"imdb.com\/name\/(.*?)\?")).Groups[1].Value;
                directorsList.Add(director.Text, directorID);
                Console.WriteLine("-- director: {0} ({1})", director.Text, directorID);
            }

            //TODO: add all actors perhaps?
            //first few actors
            Dictionary<string,string> actorsList = new Dictionary<string,string>();
            var briefActors = driver.FindElementsByXPath("//div[@class='credit_summary_item']/span[@itemprop='actors']/a");
            foreach (IWebElement actor in briefActors)
            {
                string actorID = (Regex.Match(actor.GetAttribute("href"), @"imdb.com\/name\/(.*?)\?")).Groups[1].Value;
                actorsList.Add(actor.Text, actorID);
                Console.WriteLine("- actor: {0} ({1})", actor.Text, actorID);
            }

            //get actors' photos
            Dictionary<string, string> actorsPhotos = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> actor in actorsList)
            {
                driver.Navigate().GoToUrl("http://www.imdb.com/name/" + actor.Value);

                IWebElement actorPhoto;
                try
                {
                    actorPhoto = driver.FindElementByXPath("//td[@id='img_primary']/div[@class='image']/a");
                    Console.WriteLine(" -- photo url: {0}", actorPhoto.GetAttribute("href"));
                    actorsPhotos.Add(actor.Value, actorPhoto.GetAttribute("href"));
                }
                catch
                {
                    actorPhoto = driver.FindElementByXPath("//div[@class='no-pic-wrapper']/a/img[@class='no-pic-image']");
                    Console.WriteLine(" -- no photo found");
                    actorsPhotos.Add(actor.Value, actorPhoto.GetAttribute("src"));
                }
                
            }

            //stills
            List<string> stillsList = new List<string>();
            driver.Navigate().GoToUrl("http://www.imdb.com/title/" + movieID + "/mediaindex?refine=still_frame");
            var thumbnailGrid = driver.FindElementsByXPath("//div[@id='media_index_thumbnail_grid']/a/img");
            foreach (IWebElement thumbnail in thumbnailGrid)
            {
                stillsList.Add(thumbnail.GetAttribute("src"));
            }

            //save everything to mysql db
            saveToMySQL(movieID, title, year, genresList, description, actorsList, directorsList, stillsList, actorsPhotos);
            actorsList.Clear();
            directorsList.Clear();
        }

        static void saveToMySQL(string movieID, string title, string year, List<string> genresList, string description, Dictionary<string, string> actorsList, Dictionary<string, string> directorsList, List<string> stillsList, Dictionary<string, string> actorsPhotos)
        {
            MySQL mysql = new MySQL();
            mysql.connect("localhost", "3306", "imdb_test", "root", "");
            mysql.upload(movieID, title, year, genresList, description, actorsList, directorsList, stillsList, actorsPhotos);
            mysql.disconnect();
        }
    }
}
