﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace hackerone
{
    class Program
    {
        static void Main(string[] args)
        {

            //A simple Selenium script which extracts the Twitter accounts of top-ranking (Highest Reputation)
            //bug hunters from HackerOne and saves it to file.

            var chromeOptions = new ChromeOptions();
            //chromeOptions.AddArguments("headless");
            var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                chromeOptions);

            var allTwitterLinks = new List<string>();

            var urlsToScrape = new List<string>()
            {
                "https://hackerone.com/leaderboard/reputation", //Highest Reputation
                "https://hackerone.com/leaderboard/up_and_comers",  //Up and Comers
                "https://hackerone.com/leaderboard/high_critical", //Highest Critical Reputation
                "https://hackerone.com/leaderboard/upvotes", //Most Upvoted,
                "https://hackerone.com/leaderboard/high_critical" //Highest Critical Reputation
            };


            foreach (var url in urlsToScrape)
            {
                driver.Navigate().GoToUrl(url);

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

                WebDriverWait wait = new WebDriverWait(driver,
                    TimeSpan.FromSeconds(3));
                wait.Until((x) =>
                {
                    return ((IJavaScriptExecutor)driver).ExecuteScript(
                        "return document.readyState").Equals("complete");
                });


                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//tbody['daisy-table-body']")));
                
                var hackers = driver.FindElements(By.XPath("//tbody['daisy-table-body']/tr//a"));
                

                for (int i = 0; i < hackers.Count; i++)
                {
                    try
                    {
                        hackers = driver.FindElements(By.XPath("//tbody['daisy-table-body']/tr//a"));
                        hackers[i].Click();

                        wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(
                            By.XPath("//div/div[@class='card__content']")));
                        var hackerCard = driver.FindElements(By.XPath("//div/div[@class='card__content']")).First();

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
                        wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(
                            By.XPath("//a[@class='daisy-link ahref margin-5--left margin-5--right spec-twitter-handle']")));

                        var twitterIcon =
                            hackerCard.FindElement(
                                By.XPath(
                                    "//a[@class='daisy-link ahref margin-5--left margin-5--right spec-twitter-handle']"));
                        var twitterLink = twitterIcon.GetAttribute("href");
                        allTwitterLinks.Add(twitterLink);
                        driver.Navigate().Back();
                    }
                    catch
                    {
                        driver.Navigate().Back();
                        continue;
                    }

                }

            }

            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            var directoryFullName = new FileInfo(location.AbsolutePath).Directory.FullName;

            var pathFile = $@"{directoryFullName}\HackerOne_{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}.txt";

            File.AppendAllLines(pathFile, allTwitterLinks.Distinct(), Encoding.UTF8);
            
            driver.Quit();

        }

    }

}


