using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace NLS.Models
{
    public class SearchViewModel
    {
        public string Age { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Form { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string Publisher { get; set; }
        public string Series { get; set; }
        public string Message { get; set; }

        public List<SelectListItem> AvailableAges { get; set; }
        public List<SelectListItem> AvailableAuthors { get; set; }
        public List<SelectListItem> AvailableGenres { get; set; }
        public List<SelectListItem> AvailableForms { get; set; }
        public List<SelectListItem> AvailableLocations { get; set; }
        public List<SelectListItem> AvailableTypes { get; set; }
        public List<SelectListItem> AvailablePublishers { get; set; }
        public List<SelectListItem> AvailableSeries { get; set; }

        public List<string> Results { get; set; }

        public SearchViewModel()
        {
            AvailableAges = new List<SelectListItem>();
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
