using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Controllers
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
            
            return View(blogs);
        }

        /*********SEARCH*********/
        public ActionResult Search(string phrase)
        {
            BlogDao blogDao = new BlogDao();
            IList<Blog> blogs = blogDao.SearchBlogs(phrase);

            return View("Index", blogs);
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

    }
}