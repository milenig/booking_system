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

namespace WebApplication6.Areas.Admin.Controllers
{
        [Authorize]
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
            

                KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);

                if (user.Role.Identificator != "admin")
                    return View("IndexCtenar", tours); //klasicky pohled navstevnika

                return View(tours); //admin index
            }

        [Authorize(Roles = "admin")]
        public ActionResult Zamestnanci()
            {
                KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
                IList<KnihovnaUser> knihovnaUsers = knihovnaUserDao.GetAll();

                return View(knihovnaUsers);
            }

            public ActionResult Rezervace()
            {
                ReservationDao reservationDao = new ReservationDao();
                IList<Reservation> reservations = reservationDao.GetAll();
                KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);

                if (user.Role.Identificator != "admin")
                    return View("RezervaceCtenar", reservations);
            return View(reservations);
            }
        

        ///////////////////SEARCH/////////////////////
        public ActionResult Category(int id)
            {
                IList<Tour> tours = new TourDao().GetToursInCategoryId(id);
                ViewBag.Categories = new DestinationDao().GetAll();
                KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);
                if (user.Role.Identificator != "admin")
                    return View("IndexCtenar", tours);

                return View("Index", tours);
            }

            public ActionResult Search(string phrase)
            {
                TourDao tourDao = new TourDao();
                IList<Tour> tours = tourDao.SearchTours(phrase);
                KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);
                if (user.Role.Identificator != "admin")
                    return View("IndexCtenar", tours);

                return View("Index", tours);
            }

            public ActionResult SearchPrice(int hi)
            {
                int totalBooks;
                TourDao tourDao = new TourDao();
                IList<Tour> tours = tourDao.SearchByPrice(hi);
                KnihovnaUser user = new KnihovnaUserDao().GetByLogin(User.Identity.Name);
                if (user.Role.Identificator != "admin")
                    return View("IndexCtenar", tours);

                return View("Index", tours);
            }
        ///////////////////END SEARCH///////////////////


        public ActionResult Detail(int id)
            {
                TourDao bookDao = new TourDao();
                Tour b = bookDao.GetById(id);

                return View(b);
            }
        
        [Authorize(Roles = "admin")]
            public ActionResult Create()
            {
                DestinationDao destinationDao = new DestinationDao();
                IList<Destination> destinations = destinationDao.GetAll();

                ViewBag.Categories = destinations;

                return View();
            }


        [Authorize(Roles = "admin")]
            public ActionResult CreateCategory()
            {
                return View();
            }

        [Authorize(Roles = "admin")]
            public ActionResult CreateEmployee()
            {
                KnihovnaRoleDao knihovnaRoleDao = new KnihovnaRoleDao();
                IList<KnihovnaRole> zamestnanci = knihovnaRoleDao.GetAll();

                ViewBag.Zamestnanci = zamestnanci;

                return View();
            }

            [HttpPost]
            [Authorize(Roles = "admin")]
            public ActionResult Add(Tour tour, HttpPostedFileBase picture, int categoryId)
            {
                DestinationDao destinationDao = new DestinationDao();
                IList<Destination> destinations = destinationDao.GetAll();

                ViewBag.Categories = destinations;
            if (ModelState.IsValid)
                {
                    if (picture != null)
                    {
                        if (picture.ContentType == "image/jpeg" || picture.ContentType == "image/png")
                        {
                            Image image = Image.FromStream(picture.InputStream);

                            Guid guid = Guid.NewGuid();
                            string imageName = guid.ToString() + ".jpg";


                        Image smallImage = ImageHelper.ScaleImage(image, 1200, 1200);
                        Bitmap b = new Bitmap(smallImage);

                        b.Save(Server.MapPath("~/uploads/book/" + imageName), ImageFormat.Jpeg);

                        smallImage.Dispose();
                        b.Dispose();

                        tour.ImageName = imageName;
                        }
                    }
                    
                    Destination destination = destinationDao.GetById(categoryId);

                    tour.Destination = destination;

                    if (tour.CurrentCapacity == null)
                        tour.CurrentCapacity = 0;

                    if (tour.Priority == null)
                        tour.Priority = 0;

                    TourDao tourDao = new TourDao();
                    tourDao.Create(tour);

                    TempData["message-success"] = "Zajezd byl uspesne pridan";

                }
                else
            {
                return View("Create", tour);
            }

                return RedirectToAction("Index");
            }

        
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult AddWorker(KnihovnaUser knihovnaUser, int roleId)
        {
            if (ModelState.IsValid)
            {
                KnihovnaRoleDao knihovnaRoleDao = new KnihovnaRoleDao();
                KnihovnaRole knihovnaRole = knihovnaRoleDao.GetById(roleId);

                knihovnaUser.Role = knihovnaRole;

                KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
                knihovnaUserDao.Create(knihovnaUser);

                TempData["message-success"] = "Uzivatel byl uspesne pridan";
            }
            else
            {
                return View("CreateEmployee", knihovnaUser);
            }

            return RedirectToAction("Zamestnanci");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult AddCategory(Destination bookCategory)
        {
            if (ModelState.IsValid)
            {
                DestinationDao bookCategoryDao = new DestinationDao();
                bookCategoryDao.Create(bookCategory);

                TempData["message-success"] = "Destinace byla uspesne pridana";
                ViewBag.Categories = bookCategoryDao.GetAll();
            }
            else
            {
                return View("CreateCategory", bookCategory);
            }

            return RedirectToAction("Create");
        }

        [Authorize(Roles = "admin")]
            public ActionResult Edit(int id)
            {
                TourDao bookDao = new TourDao();
                DestinationDao bookCategoryDao = new DestinationDao();

                Tour b = bookDao.GetById(id);
                ViewBag.Categories = bookCategoryDao.GetAll();

                return View(b);
            }

            [Authorize(Roles = "admin")]
            public ActionResult EditWorker(int id)
            {
                KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
                KnihovnaUser knihovnaUser = knihovnaUserDao.GetById(id);

                KnihovnaRoleDao knihovnaRoleDao = new KnihovnaRoleDao();
                ViewBag.Zamestnanci = knihovnaRoleDao.GetAll();

                return View(knihovnaUser);
            }


        [Authorize(Roles = "admin")]
            [HttpPost]
            public ActionResult Update(Tour tour, HttpPostedFileBase picture, int categoryId)
            {
            try
            {
                TourDao bookDao = new TourDao();
                DestinationDao bookCategoryDao = new DestinationDao();

                Destination bookCategory = bookCategoryDao.GetById(categoryId);

                tour.Destination = bookCategory;

                if (picture != null)
                {
                    if (picture.ContentType == "image/jpeg" || picture.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(picture.InputStream);

                        Guid guid = Guid.NewGuid();
                        string imageName = guid.ToString() + ".jpg";

                        
                        //picture.SaveAs(Server.MapPath("~/uploads/tour/" + picture.FileName));

                        Image smallImage = ImageHelper.ScaleImage(image, 1200, 1200);
                        Bitmap b = new Bitmap(smallImage);

                        b.Save(Server.MapPath("~/uploads/book/" + imageName), ImageFormat.Jpeg);

                        smallImage.Dispose();
                        b.Dispose();
                        
                        //smazat stary soubor
                        System.IO.File.Delete(Server.MapPath("~/uploads/book/" + tour.ImageName));

                        tour.ImageName = imageName;
                    }
                }

                bookDao.Update(tour);

                TempData["message-success"] = "Zajezd " + tour.Name + " byl upraven";
            }
            catch (Exception)
            {
                throw;
            }
            return RedirectToAction("Index", "Tours");
        }

            [Authorize(Roles = "admin")]
            [HttpPost]
            public ActionResult UpdateWorker(KnihovnaUser knihovnaUser, int roleId)
            {
                try
                {
                    KnihovnaRoleDao knihovnaRoleDao = new KnihovnaRoleDao();
                    KnihovnaRole knihovnaRole = knihovnaRoleDao.GetById(roleId);
                    knihovnaUser.Role = knihovnaRole;

                    KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();

                    knihovnaUserDao.Update(knihovnaUser);

                    TempData["message-success"] = "Uspesne provedena zmena uzivatele";
                }
                catch (Exception)
                {
                    throw;
                }

                return RedirectToAction("Zamestnanci");
            }


        [Authorize(Roles = "admin")]
            public ActionResult Delete(int id)
            {
                try
                {
                    TourDao tourDao = new TourDao();
                    Tour tour = tourDao.GetById(id);

                    //System.IO.File.Delete(Server.MapPath("~/uploads/book/" + tour.ImageName));
                    
                    /*******POTREBA SMAZAT REZERVACE******/
                    ReservationDao reservationDao = new  ReservationDao();
                    IList<Reservation> reservations = reservationDao.GetAll();

                    foreach (Reservation reservation in reservations)
                    {
                        if(reservation.Tour.Id==id)
                            reservationDao.Delete(reservation);
                    }

                    tourDao.Delete(tour);

                    TempData["message-success"] = "Zajezd " + tour.Name + " byl smazan";
                }
                catch (Exception e)
                {
                    throw;
                }

                return RedirectToAction("Index");
            }

            [Authorize(Roles = "admin")]
            public ActionResult DeleteWorker(int id)
            {
                try
                {
                    KnihovnaUserDao knihovnaUserDao = new KnihovnaUserDao();
                    KnihovnaUser knihovnaUser = knihovnaUserDao.GetById(id);

                    knihovnaUserDao.Delete(knihovnaUser);

                    TempData["message-success"] = "Uzivatel " + knihovnaUser.Name + " " + knihovnaUser.Surname + " byl uspesne odstranen";
            }
                catch (Exception e)
                {
                    throw;
                }

                return RedirectToAction("Zamestnanci");

            }
    }
}