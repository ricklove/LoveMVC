using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TodoMVC.Models.App;
using TodoMVC.ViewModels;

namespace TodoMVC.Controllers.App
{
    public class TodosController : Controller
    {
        private static TodosViewModel CreateDefault()
        {
            var viewModel = new TodosViewModel() { Todos = new List<TodoItemViewModel>() { new TodoItemViewModel() { TodoItem = new TodoItem() { Text = "Do This!" } } } };
            return viewModel;
        }

        public ActionResult Index()
        {
            var viewModel = CreateDefault();

            return View(viewModel);
        }

        public ActionResult Details()
        {
            var viewModel = CreateDefault();

            return View("Details", viewModel);
        }

        public ActionResult Edit()
        {
            var viewModel = CreateDefault();

            return View("Edit", viewModel);
        }


        public ActionResult Create()
        {
            var viewModel = CreateDefault();

            return View("Create", viewModel);
        }
    }
}