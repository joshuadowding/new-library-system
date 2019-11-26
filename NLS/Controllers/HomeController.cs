using Microsoft.AspNetCore.Diagnostics;
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
        private const string NO_FILTER_WARNING = @"No filters selected - returning all publications by default.";

        private readonly ILogger<HomeController> logger;

        public Server Server { get; set; }

        private bool initializeServer;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;

            if (!initializeServer)
            {
                Server = new Server();

                if (Server.Connect())
                {
                    initializeServer = true;
                }
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            SearchViewModel viewModel = new SearchViewModel();
            PopulateSearchFilters(viewModel);

            if (viewModel.Results != null)
            {
                if (viewModel.Results.Count == 0)
                {
                    viewModel.Results = Server.Query.QueryAllIndividuals();
                }
            }
            else
            {
                viewModel.Results = Server.Query.QueryAllIndividuals();
            }

            logger.LogInformation("Index:GET");
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

            if (!String.IsNullOrWhiteSpace(viewModel.Age))
            {
                searchModel.SearchClasses.Add(viewModel.Age);
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
                //viewModel.Results = Server.QueryAllIndividuals(); // Get all individuals by default.
                viewModel.Message = NO_FILTER_WARNING;
            }
            else
            {
                List<string> results = Server.Query.QueryIndividualsWithSearchModel(searchModel); //TODO: Query Ontology - Take values from viewModel and construct a query with them.

                if (results != null)
                {
                    if (results.Count > 0)
                    {
                        viewModel.Results = results;
                    }
                }
            }

            PopulateSearchFilters(viewModel);

            logger.LogInformation("Index:POST");
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Search(SearchViewModel viewModel)
        {
            if (!String.IsNullOrWhiteSpace(viewModel.Title))
            {
                string title = viewModel.Title.Trim().ToLower();
                List<string> results = Server.Query.QueryIndividualsWithTextContains(title);

                if (results != null)
                {
                    if (results.Count > 0)
                    {
                        viewModel.Results = results;
                    }
                }
            }
            else
            {
                //viewModel.Results = Server.QueryAllIndividuals(); // Get all individuals by default.
                viewModel.Message = NO_FILTER_WARNING;
            }

            PopulateSearchFilters(viewModel);

            logger.LogInformation("Search:POST");
            return View("Index", viewModel);
        }

        [HttpGet]
        public IActionResult Select()
        {
            string selectedItem = HttpContext.Request.RouteValues["id"].ToString();

            ProductViewModel productViewModel = new ProductViewModel();
            productViewModel.SelectedItem = selectedItem;

            logger.LogInformation("Select:GET");
            return RedirectToAction("Product", productViewModel);
        }

        [HttpGet]
        public IActionResult Reset()
        {
            logger.LogInformation("Reset:GET");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Product(ProductViewModel viewModel)
        {
            viewModel.Publication = Server.Query.QueryIndividualPublication(viewModel.SelectedItem);

            logger.LogInformation("Product:GET");
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            logger.LogError(HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error.ToString());
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void PopulateSearchFilters(SearchViewModel viewModel)
        {
            foreach (string option in Server.Query.QueryFilterOptions("Location", QueryFilterType.Individual))
            {
                viewModel.AvailableLocations.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Literary_Form", QueryFilterType.Class))
            {
                viewModel.AvailableForms.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Genre", QueryFilterType.Class))
            {
                viewModel.AvailableGenres.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Publication", QueryFilterType.Class))
            {
                viewModel.AvailableTypes.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Age_Range", QueryFilterType.Class))
            {
                viewModel.AvailableAges.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Series", QueryFilterType.Individual))
            {
                viewModel.AvailableSeries.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Publisher", QueryFilterType.Individual))
            {
                viewModel.AvailablePublishers.Add(new SelectListItem(option, option));
            }

            foreach (string option in Server.Query.QueryFilterOptions("Author", QueryFilterType.Individual))
            {
                viewModel.AvailableAuthors.Add(new SelectListItem(option, option));
            }
        }
    }
}
