using Formula1.Core.Contracts;
using System.ComponentModel.DataAnnotations;

namespace Formula1.Core.Entities
{
    public class Driver : ICompetitor
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public string Nationality { get; set; }
        public string Name => ToString();

        public override string ToString()
        {
            return Firstname + " " + Lastname;
        }
    }
}
