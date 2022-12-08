using System.Collections.Generic;

namespace DATIS.Models
{
    public class Airport
    {
        public string Name { get; set; }
        public List<Atis> AtisList { get; set; }

        public Airport(string name, List<Atis> atisList = null)
        {
            Name = name;
            AtisList = atisList;
        }
    }
}
