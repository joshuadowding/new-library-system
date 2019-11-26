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

        [HttpGet]
        public IActionResult Index()
        {
            SearchViewModel viewModel = new SearchViewModel();
            PopulateSearchFilters(viewModel);

            viewModel.Results = Server.QueryAllIndividuals();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(SearchViewModel viewModel)
        {
            SearchModel searchModel = new SearchModel();

            int tries = 0;
            if (!String.IsNullOrWhiteSpace(viewModel.Author))
            {
                searchModel.SearchIndividuals.Add("Author", viewModel.Author);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Form))
            {
                searchModel.SearchClasses.Add(viewModel.Form);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Genre))
            {
                searchModel.SearchClasses.Add(viewModel.Genre);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Location))
            {
                searchModel.SearchIndividuals.Add("Location", viewModel.Location);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Publisher))
            {
                searchModel.SearchIndividuals.Add("Publisher", viewModel.Publisher);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Series))
            {
                searchModel.SearchIndividuals.Add("Series", viewModel.Series);
                tries++;
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Type))
            {
                searchModel.SearchClasses.Add(viewModel.Type);
                tries++;
            }

            viewModel = new SearchViewModel();

            if (tries == 0)
            {
                viewModel.Results = Server.QueryAllIndividuals(); // Get all individuals by default.
                viewModel.Message = @"No filters selected - returning all individuals by default.";
            }
            else
            {
                List<string> results = Server.QueryIndividualsWithSearchModel(searchModel); //TODO: Query Ontology - Take values from viewModel and construct a query with them.

                if (results != null)
                {
                    if (results.Count > 0)
                    {
                        viewModel.Results = results;
                    }
                }
            }

            PopulateSearchFilters(viewModel);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Search()
        {
            string selectedItem = HttpContext.Request.RouteValues["id"].ToString();

            ProductViewModel productViewModel = new ProductViewModel();
            productViewModel.SelectedItem = selectedItem;
            return RedirectToAction("Product", productViewModel);
        }

        [HttpGet]
        public IActionResult Reset()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Product(ProductViewModel viewModel)
        {
            viewModel.Publication = Server.QueryIndividualPublication(viewModel.SelectedItem); // Query ontology for product page.
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void PopulateSearchFilters(SearchViewModel viewModel)
        {
            // TODO: Drastically simplify everything below.

            foreach (string option in Server.QueryFilterOptions("Location", QueryFilterType.Individual))
            {
                viewModel.AvailableLocations.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Literary_Form", QueryFilterType.Class))
            {
                viewModel.AvailableForms.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Genre", QueryFilterType.Class))
            {
                viewModel.AvailableGenres.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Publication", QueryFilterType.Class))
            {
                viewModel.AvailableTypes.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Age_Range", QueryFilterType.Class))
            {
                viewModel.AvailableAges.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Series", QueryFilterType.Individual))
            {
                viewModel.AvailableSeries.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Publisher", QueryFilterType.Individual))
            {
                viewModel.AvailablePublishers.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.QueryFilterOptions("Author", QueryFilterType.Individual))
            {
                viewModel.AvailableAuthors.Add(new SelectListItem(option, option));
            }
        }
    }
}
