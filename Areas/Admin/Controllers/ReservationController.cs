using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication6.Areas.Admin.Controllers
{
    public class ReservationController : Controller
    {
        ///Reservation Section - Detail of Tour///
        [ChildActionOnly]
        public ActionResult ReservationSection(int id)
        {
            TourDao tDao = new TourDao();
            IList<Tour> t = new List<Tour>();
            t.Add(tDao.GetById(id));

            ViewBag.Tour = t;

            ReservationDao reservationDao = new ReservationDao();
            Reservation reservation = reservationDao.GetById(id);

            return View(reservation);
        }

        [ChildActionOnly]
        public ActionResult RezervaceDetailSide(int id)
        {
            TourDao bookDao = new TourDao();
            Tour b = bookDao.GetById(id);

            return View(b);
        }

        public class RandomGenerator
        {
            // Generate a random number between two numbers    
            public int RandomNumber(int min, int max)
            {
                Random random = new Random();
                return random.Next(min, max);
            }

            // Generate a random string with a given size    
            public string RandomString(int size, bool lowerCase)
            {
                StringBuilder builder = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }
                if (lowerCase)
                    return builder.ToString().ToLower();
                return builder.ToString();
            }

            // Generate a random password    
            public string RandomPassword()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(RandomString(4, true));
                builder.Append(RandomNumber(1000, 9999));
                builder.Append(RandomString(2, false));
                return builder.ToString();
            }
        }

        [HttpPost]
        public ActionResult AddReservation(Reservation reservation,int tourId)
        {
            ReservationDao reservationDao = new ReservationDao();

            RandomGenerator generator = new RandomGenerator();
            string validationNumber = generator.RandomPassword();

            reservation.ValidationNumber = validationNumber;
            reservation.Status = "nezaplaceno";
            
            reservation.KnihovnaUser = new KnihovnaUserDao().GetByLogin(User.Identity.Name);

            if (ModelState.IsValid)
            {
                TourDao tourDao = new TourDao();
                Tour tour = tourDao.GetById(tourId);

                reservation.Tour = tour;
                int result = DateTime.Compare(DateTime.Now, tour.StartDate);
                

                if (tour.CurrentCapacity < tour.Capacity && result < 0)
                {
                    reservationDao.Create(reservation);

                    tour.CurrentCapacity++;

                    tourDao.Update(tour);
                    reservationDao.Update(reservation);

                    TempData["message-success"] = "Vase rezervace byla uspesne odeslana, prosim ulozte si nasledujici kod: " + validationNumber;
                }
                else
                {
                    TempData["message-danger"] = "Je nam lito ale neni mozne provest rezervaci";
                }
            }
            else
            {
                return RedirectToAction("Detail","Tours", new { id = tourId });
            }

            return RedirectToAction("Detail","Tours", new { id = tourId }); //presmerovani
        }

        [Authorize(Roles = "admin")]
        public ActionResult ChangeStatus(int id,Boolean zrusit,Boolean vratit)
        {
            try
            {
                ReservationDao reservationDao = new ReservationDao();
                Reservation reservation = reservationDao.GetById(id);

                TourDao tourDao = new TourDao();
                Tour tour = tourDao.GetById(reservation.Tour.Id);

                if (zrusit)
                {
                    if (vratit)
                    {
                        int result = DateTime.Compare(DateTime.Now, tour.StartDate);


                        if (tour.CurrentCapacity < tour.Capacity && result < 0)
                        {
                            reservation.Status = "nezaplaceno";
                            tour.CurrentCapacity++;
                            tourDao.Update(tour);

                            reservationDao.Update(reservation);

                            TempData["message-success"] = "Status rezervace " + reservation.ValidationNumber + " byl zmenen";
                        }
                        else
                        {
                            TempData["message-danger"] = "Je nam lito ale neni mozne vratit rezervaci. Nedostatek mist nebo je zajezd uz zahajen.";
                        }
                    }
                    else
                    {
                        reservation.Status = "zruseno";
                        tour.CurrentCapacity--;
                        tourDao.Update(tour);

                        reservationDao.Update(reservation);

                        TempData["message-success"] = "Status rezervace " + reservation.ValidationNumber + " byl zmenen";
                    }
                }
                else
                {
                    if (reservation.Status == "nezaplaceno")
                    {
                        reservation.Status = "zaplaceno";
                    }
                    else
                    {
                        reservation.Status = "nezaplaceno";
                    }
                    reservationDao.Update(reservation);

                    TempData["message-success"] = "Status rezervace " + reservation.ValidationNumber + " byl zmenen";
                }
                

                
            }
            catch (Exception e)
            {
                throw;
            }
            return RedirectToAction("Rezervace","Tours");
        }

        [Authorize(Roles = "admin")]
        public ActionResult DeleteReservation(int id)
        {
            try
            {
                ReservationDao reservationDao = new ReservationDao();
                Reservation reservation = reservationDao.GetById(id);

                TourDao tourDao = new TourDao();
                Tour tour = tourDao.GetById(reservation.Tour.Id);

                tour.CurrentCapacity--;
                tourDao.Update(tour);

                reservationDao.Delete(reservation);

                TempData["message-success"] = "Rezervace " + reservation.ValidationNumber + " byla uspesne smazana z databazi";
            }
            catch (Exception e)
            {
                throw;
            }
            return RedirectToAction("Rezervace", "Tours");
        }
    }
}