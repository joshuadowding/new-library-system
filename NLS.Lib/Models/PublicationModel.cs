using System;
using System.Collections.Generic;
using System.Text;

namespace NLS.Lib.Models
{
    public class PublicationModel
    {
        public List<string> Authors { get; set; }
        public List<string> Types { get; set; }
        public string Series { get; set; }
        public string Publisher { get; set; }
        public string Imprint { get; set; }
        public string ISBN { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Summary { get; set; }
        public string Format { get; set; }
        public string Source { get; set; }
        public string PageCount { get; set; }
        public string Identifier { get; set; }
        public string CopyTotal { get; set; }
        public string Language { get; set; }
        public string Weight { get; set; }
        public string Link { get; set; }

        public PublicationModel()
        {
            Authors = new List<string>();
            Types = new List<string>();
        }
    }
}
