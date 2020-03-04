using Formula1.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Formula1.Core
{
    /// <summary>
    /// Daten sind in XML-Dateien gespeichert und werden per Linq2XML
    /// in die Collections geladen.
    /// </summary>
    public static class ImportController
    {

        /// <summary>
        /// Daten der Rennen werden aus der
        /// XML-Datei ausgelesen und in die Races-Collection gespeichert.
        /// Grund: Races werden nicht aus den Results geladen, weil sonst die
        /// Rennen in der Zukunft fehlen
        /// </summary>
        public static IEnumerable<Race> LoadRacesFromRacesXml()
        {
            List<Race> races = new List<Race>();
            string racesPath = MyFile.GetFullNameInApplicationTree("Races.xml");
            var xElement = XDocument.Load(racesPath).Root;
            if (xElement != null)
            {
                races =
                    xElement.Elements("Race")
                    .Select(race =>
                    new Race
                    {
                        Number = (int)race.Attribute("round"),
                        Date = (DateTime)race.Element("Date"),
                        Country = race.Element("Circuit")
                                        ?.Element("Location")
                                        ?.Element("Country")?.Value,
                        City = race.Element("Circuit")
                                    ?.Element("Location")
                                    ?.Element("Locality")?.Value
                    })
                    .ToList();
            }
            return races;
        }

        /// <summary>
        /// Aus den Results werden alle Collections, außer Races gefüllt.
        /// Races wird extra behandelt, um auch Rennen ohne Results zu verwalten
        /// </summary>
        public static IEnumerable<Result> LoadResultsFromXmlIntoCollections()
        {
            var races = LoadRacesFromRacesXml();
            var drivers = new List<Driver>();
            var teams = new List<Team>();
            var result = new List<Result>();
            string racesPath = MyFile.GetFullNameInApplicationTree("Results.xml");
            var xElement = XDocument.Load(racesPath).Root;
            if (xElement != null)
            {
                result = xElement.Elements("Race").Elements("ResultsList").Elements("Result")
                    .Select(result => new Result
                    {
                        Race = GetRace(result, races),
                        Driver = GetDriver(result, drivers),
                        Team = GetTeam(result, teams),
                        Position = (int)result.Attribute("position"),
                        Points = (int)result.Attribute("points")
                    }).ToList();
            }
            return result;

        }

        private static Team GetTeam(XElement result, List<Team> teams)
        {

            var teamName = (string)result.Element("Constructor").Element("Name");
            var teamNationality = (string)result.Element("Constructor").Element("Nationality");

            var inList = teams.SingleOrDefault(s => s.Name.Equals(teamName));
            if (inList == default(Team))
            {
                inList = new Team()
                {
                    Name = teamName,
                    Nationality = teamNationality
                };
                teams.Add(inList);

            }
            return inList;
        }

        private static Driver GetDriver(XElement result, List<Driver> drivers)
        {
            var driver = new Driver()
            {
                Firstname = (string)result?.Element("Driver")?.Element("GivenName"),
                Lastname = (string)result?.Element("Driver")?.Element("FamilyName"),
                Nationality = (string)result?.Element("Driver")?.Element("Nationality")
            };

            var inList = drivers.SingleOrDefault(s => s.ToString().Equals(driver.ToString()));
            if (inList == default(Driver))
            {
                drivers.Add(driver);
                inList = driver;
            }


            return inList;

        }

        private static Race GetRace(XElement result, IEnumerable<Race> race)
        {
            int raceNumber = (int)result.Parent?.Parent?.Attribute("round");
            return race.Single(race => race.Number == raceNumber);
        }
    }
}