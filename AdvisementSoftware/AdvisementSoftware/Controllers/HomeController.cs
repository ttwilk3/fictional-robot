﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdvisementSoftware.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult Profile()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult ViewCatalog()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult ViewCourses()
        {
            ViewBag.Message = "";

            return View();
        }
    }
}