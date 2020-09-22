using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Controllers
{
    public class MenuuController : Controller
    {
        [ChildActionOnly]
        // GET: Admin/Menu
        public ActionResult Index()
        {
            return View();
        }
    }
}