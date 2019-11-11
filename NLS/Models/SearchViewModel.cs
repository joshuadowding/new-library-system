using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace NLS.Models
{
    public class SearchViewModel
    {
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Form { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string Publisher { get; set; }
        public string Series { get; set; }

        public List<SelectListItem> AvailableAuthors { get; set; }
        public List<SelectListItem> AvailableGenres { get; set; }
        public List<SelectListItem> AvailableForms { get; set; }
        public List<SelectListItem> AvailableLocations { get; set; }
        public List<SelectListItem> AvailableTypes { get; set; }
        public List<SelectListItem> AvailablePublishers { get; set; }
        public List<SelectListItem> AvailableSeries { get; set; }

        public SearchViewModel()
        {
            AvailableAuthors = new List<SelectListItem>();
            AvailableGenres = new List<SelectListItem>();
            AvailableForms = new List<SelectListItem>();
            AvailableLocations = new List<SelectListItem>();
            AvailableTypes = new List<SelectListItem>();
            AvailablePublishers = new List<SelectListItem>();
            AvailableSeries = new List<SelectListItem>();
        }
    }
}
