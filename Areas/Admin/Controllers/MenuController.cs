using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Areas.Admin.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        [ChildActionOnly]
        // GET: Admin/Menu
        public ActionResult Index()
        {
            KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
            KnihovnaUser knihovnaUser = knihovnaUserDao.GetByLogin(User.Identity.Name);



            return View(knihovnaUser);
        }
    }
}