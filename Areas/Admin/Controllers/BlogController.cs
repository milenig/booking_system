using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Areas.Admin.Controllers
{
    public class BlogController : Controller
    {
        public ActionResult Index(int? pageBlog)
        {
            int itemsOnPageBlog = 9;
            int pgB = pageBlog.HasValue ? pageBlog.Value : 1;
            int totalBlogs;

            BlogDao blogDao = new BlogDao();
            IList<Blog> blogs = blogDao.GetBlogPaged(itemsOnPageBlog, pgB, out totalBlogs);

            ViewBag.PagesBlog = (int)Math.Ceiling((double)totalBlogs / (double)itemsOnPageBlog);
            ViewBag.CurrentPageBlog = pgB;

            KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "blogger" || user.Role.Identificator == "admin")
                return View("BlogEdited", blogs);

            return View(blogs);
        }

        /*********SEARCH*********/
        public ActionResult Search(string phrase)
        {
            BlogDao blogDao = new BlogDao();
            IList<Blog> blogs = blogDao.SearchBlogs(phrase);
            KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "blogger" || user.Role.Identificator == "admin")
                return View("BlogEdited", blogs);

            return View("Index", blogs);
        }

        [HttpPost]
        [Authorize(Roles = "blogger,admin")]
        public ActionResult AddBlog(Blog blog)
        {
            if (ModelState.IsValid)
            {
                BlogDao blogDao = new BlogDao();
                blogDao.Create(blog);

                TempData["message-success"] = "Blog byl uspesne pridan";
            }
            else
            {
                //vracam View Create sa svim podatcima koje je vec ispunio
                return View("CreateBlog", blog);
            }

            return RedirectToAction("Index"); //presmerovani
        }

        [Authorize(Roles = "blogger,admin")]
        public ActionResult CreateBlog()
        {
            return View();
        }

        public ActionResult DetailBlog(int id)
        {
            BlogDao blogDao = new BlogDao();
            Blog blog = blogDao.GetById(id);

            return View(blog);
        }
        

        [ChildActionOnly]
        public ActionResult RenderSide()
        {
            /**
             * Render Side for Detail Index - Reicents Blogs
             */
            int totalBlogs;
            BlogDao blogDao = new BlogDao();
            IList<Blog> blogs = blogDao.GetBlogPaged(3, 1, out totalBlogs);
            return View(blogs);
        }
        [ChildActionOnly]
        public ActionResult RenderSide2()
        {
            /**
             * Render Side for Detail Index - Top Tour
             */
            int totalBlogs;
            TourDao tourDao = new TourDao();
            IList<Tour> tours = tourDao.GetToursPaged(1, 1, out totalBlogs);
            return View(tours);
        }

        [Authorize(Roles = "blogger,admin")]
        public ActionResult DeleteBlog(int id)
        {
            try
            {
                BlogDao blogDao = new BlogDao();
                Blog blog = blogDao.GetById(id);


                blogDao.Delete(blog);

                TempData["message-success"] = "Blog " + blog.Header + " byl smazan";
            }
            catch (Exception e)
            {
                throw;
            }

            return RedirectToAction("Index");

        }

        [Authorize(Roles = "blogger,admin")]
        [HttpPost]
        public ActionResult UpdateBlog(Blog blog)
        {
            try
            {
                BlogDao blogDao = new BlogDao();

                blogDao.Update(blog);

                TempData["message-success"] = "Blog " + blog.Header + " byl upraven";
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("Index", "Blog");
        }

        [Authorize(Roles = "blogger,admin")]
        public ActionResult EditBlog(int id)
        {
            BlogDao blogDao = new BlogDao();

            Blog blog = blogDao.GetById(id);

            return View(blog);
        }



    }
}