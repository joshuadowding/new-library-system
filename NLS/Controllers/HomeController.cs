using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NLS.Lib;
using NLS.Lib.Models;
using NLS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NLS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            SearchViewModel viewModel = new SearchViewModel();
            PopulateSearchFilters(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(SearchViewModel viewModel)
        {
            SearchModel searchModel = new SearchModel();

            if (!String.IsNullOrWhiteSpace(viewModel.Author))
            {
                searchModel.Author = viewModel.Author;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Form))
            {
                searchModel.Form = viewModel.Form;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Genre))
            {
                searchModel.Genre = viewModel.Genre;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Location))
            {
                searchModel.Location = viewModel.Location;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Publisher))
            {
                searchModel.Publisher = viewModel.Publisher;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Series))
            {
                searchModel.Series = viewModel.Series;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Type))
            {
                searchModel.Type = viewModel.Type;
            }

            List<string> results = Server.QueryWithSearchModel(searchModel); //TODO: Query Ontology - Take values from viewModel and construct a query with them.

            viewModel = new SearchViewModel();
            PopulateSearchFilters(viewModel);

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void PopulateSearchFilters(SearchViewModel viewModel)
        {
            foreach (string option in Server.QueryClassesForSelectors("Location"))
            {
                viewModel.AvailableLocations.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Prose"))
            {
                viewModel.AvailableForms.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Poetry"))
            {
                viewModel.AvailableForms.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Fiction"))
            {
                viewModel.AvailableGenres.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Non-Fiction"))
            {
                viewModel.AvailableGenres.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Publication"))
            {
                viewModel.AvailableTypes.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryClassesForSelectors("Book"))
            {
                viewModel.AvailableTypes.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryIndividualsForSelectors("Series"))
            {
                viewModel.AvailableSeries.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryIndividualsForSelectors("Publisher"))
            {
                viewModel.AvailablePublishers.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryIndividualsForSelectors("Author"))
            {
                viewModel.AvailableAuthors.Add(new SelectListItem(option, option));
            }
        }
    }
}
