using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CensusApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Enter path to xml: ");
            var path = @"acs 2015 variables.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(censusapi));
            
            IList<CensusMetaDaata> dataPoints = new List<CensusMetaDaata>();
            dataPoints = dataPoints.Concat(GetVariablesFrom(@"acs 2015 variables.xml", "Details"))
                .Concat(GetVariablesFrom(@"acs2015profile.xml", "Profile"))
                .Concat(GetVariablesFrom(@"acs2015subject.xml", "Subject")).ToList();
            do
            {
                Console.Clear();
                Console.WriteLine("Search for: (comma separated search tokens)");
                var searchstring = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(searchstring)) continue;
                var tokens = searchstring.Split(',');
                IEnumerable<CensusMetaDaata> presentList= new List<CensusMetaDaata>();
                var firstToken = tokens[0];
                presentList = GetFilteredBy(dataPoints, firstToken);
                var remainingTokens = tokens.Skip(1);
                foreach (var token in remainingTokens)
                {
                    presentList = GetFilteredBy(presentList, token);
                }

                foreach (var vari in presentList)
                {
                    Console.WriteLine($" {vari.DataSet}: {vari.Id}: {vari.Concept}: {vari.Label}");
                }

            } while (Console.ReadLine() != "done");
        }

        private static IList<CensusMetaDaata> GetVariablesFrom(string path, string name)
        {
            XDocument xdoc = XDocument.Load(path);
            var varsList = (from variable in xdoc.Descendants().First().Descendants().First().Descendants()
                select new CensusMetaDaata
                {
                    DataSet = name,
                    Id = variable.Attribute("id")?.Value,
                    Label = variable.Attribute("label")?.Value,
                    Concept = variable.Attribute("concept")?.Value,
                    PredicateOnly = variable.Attribute("predicate-only")?.Value,
                    PredicateType = variable.Attribute("predicate-type")?.Value
                });
            return varsList.ToList();
        }

        private static IEnumerable<CensusMetaDaata> GetFilteredBy(IEnumerable<CensusMetaDaata> list, string token)
        {
            var presentList = new List<CensusMetaDaata>();
            foreach (var vari in list)
            {
                if ( (vari.Concept.ToLower().Contains(token.ToLower()) 
                    || vari.Label.ToLower().Contains(token.ToLower())
                    || vari.Id.Contains(token)) && 
                    (!vari.Label.ToLower().Contains("margin of error") &&
                     !vari.Label.Contains(" MOE")))
                    presentList.Add(vari);
            }

            return presentList.OrderBy(p=>p.Id);
        }
    }
}
