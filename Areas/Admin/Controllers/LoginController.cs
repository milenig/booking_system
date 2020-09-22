using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        // GET: Admin/Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string login, string password)
        {
            if (Membership.ValidateUser(login, password))
            {
                FormsAuthentication.SetAuthCookie(login, false);
                return RedirectToAction("Index", "Home");
            }

            TempData["message-danger"] = "Login nebo heslo neni spravne";
            return RedirectToAction("Index", "Login");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();

            return RedirectToAction("Index", "Login");
        }

        public ActionResult CreateAccount()
        {
            return View();
        }

        public ActionResult AddAccount(KnihovnaUser knihovnaUser)
        {
            if (ModelState.IsValid)
            {
                KnihovnaRoleDao knihovnaRoleDao = new KnihovnaRoleDao();
                KnihovnaRole knihovnaRole = knihovnaRoleDao.GetById(2);

                knihovnaUser.Role = knihovnaRole;

                KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
                knihovnaUserDao.Create(knihovnaUser);

                TempData["message-success"] = "Uspesne vytvoren ucet";
            }
            else
            {
                return View("CreateAccount", knihovnaUser);
            }

            return RedirectToAction("Index","Login");
        }
    }
}