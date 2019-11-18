using System.Collections.Generic;

namespace NLS.Lib.Models
{
    public class SearchModel
    {
        public List<string> SearchClasses { get; set; }
        public Dictionary<string, string> SearchIndividuals { get; set; }

        public SearchModel()
        {
            SearchClasses = new List<string>();
            SearchIndividuals = new Dictionary<string, string>();
        }
    }
}
