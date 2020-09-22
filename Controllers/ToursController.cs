using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;
using WebApplication6.Class;

namespace WebApplication6.Controllers
{
        public class ToursController : Controller
        {
            // GET: Books
            public ActionResult Index(int? page)
            {
                int itemsOnPage = 5;
                int pg = page.HasValue ? page.Value : 1;
                int totalBooks;

                TourDao tourDao = new TourDao();
                IList<Tour> tours = tourDao.GetToursPaged(itemsOnPage,pg,out totalBooks);
                foreach (Tour tour in tours)
                {
                    if (tour.StartDate.CompareTo(DateTime.Now) > 0)
                    {
                        if (tour.CurrentCapacity == tour.Capacity)
                        {
                            tour.Status = "obsazeno";
                        }
                        else
                        {
                            tour.Status = "volno";
                        }
                    }
                    int result = DateTime.Compare(DateTime.Now, tour.EndDate);
                    if (result > 0)
                    {
                        //uskutecneno
                        tour.Status = "uskutecneno";
                        tours.Remove(tour);
                        break;
                    }
                    if (tour.StartDate.CompareTo(DateTime.Now) < 0 && tour.EndDate.CompareTo(DateTime.Now) > 0)
                    {
                        tour.Status = "kona_se";
                    }
                }
            
                //List<Tour> tours2 = (from Tour t in tours orderby t.Priority descending select t).ToList();
                ViewBag.Pages = (int)Math.Ceiling((double) totalBooks / (double) itemsOnPage);
                ViewBag.CurrentPage = pg;

                ViewBag.Categories = new DestinationDao().GetAll();

                return View(tours); //admin index
            }
        

        ///////////////////SEARCH/////////////////////
        public ActionResult Category(int id)
            {
                IList<Tour> tours = new TourDao().GetToursInCategoryId(id);
                ViewBag.Categories = new DestinationDao().GetAll();

                return View("Index", tours);
            }

            public ActionResult Search(string phrase)
            {
                TourDao tourDao = new TourDao();
                IList<Tour> tours = tourDao.SearchTours(phrase);

                return View("Index", tours);
            }

            public ActionResult SearchPrice(int hi)
            {
                int totalBooks;
                TourDao tourDao = new TourDao();
                IList<Tour> tours = tourDao.SearchByPrice(hi);

                return View("Index", tours);
            }
        ///////////////////END SEARCH///////////////////


        public ActionResult Detail(int id)
            {
                TourDao bookDao = new TourDao();
                Tour b = bookDao.GetById(id);

                return View(b);
            }
    }
}