﻿/**
 * Author: Course Staff(Starter Code)
 *        Tingting Zhou 
 *        Andy Tran
 * Course: CS5530 2023 Spring
 * 
 */
using LibraryWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestProject1")]
namespace LibraryWebServer.Controllers
{
    /// <summary>
    /// This is the controller class for Library Web Server
    /// </summary>
    public class HomeController : Controller
    {

        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;

        private readonly ILogger<HomeController> _logger;

        protected Team28LibraryContext db = new Team28LibraryContext();

        public void UseLibraryContext(Team28LibraryContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin(string name, int cardnum)
        {

            bool loginSuccessful = false;

            var query = from p in db.Patrons
                        where p.Name == name && p.CardNum == cardnum
                        select p;

            loginSuccessful = query.Any();

            if (!loginSuccessful)
            {
                return Json(new { success = false });
            }
            else
            {
                user = name;
                card = cardnum;
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json(new { success = true });
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {
            var query = from t in db.Titles
                        join i in db.Inventory on t.Isbn equals i.Isbn into temp1
                        from t1 in temp1.DefaultIfEmpty()
                        join c in db.CheckedOut on t1.Serial equals c.Serial into temp2
                        from t2 in temp2.DefaultIfEmpty()
                        join p in db.Patrons on t2.CardNum equals p.CardNum into temp3
                        from t3 in temp3.DefaultIfEmpty()
                        select new
                        {
                            isbn = t.Isbn,
                            title = t.Title,
                            author = t.Author,
                            serial = t1 == null ? null : (uint?)t1.Serial,
                            name = t3 == null ? String.Empty : t3.Name
                        };
            return Json(query.ToArray());

        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            var query = from c in db.CheckedOut
                        where c.CardNum == card
                        join i in db.Inventory on c.Serial equals i.Serial
                        join t in db.Titles on i.Isbn equals t.Isbn
                        join p in db.Patrons on c.CardNum equals p.CardNum
                        select new
                        {
                            title = c == null ? String.Empty : t.Title,
                            author = c == null ? String.Empty : t.Author,
                            serial = c == null ? null : (uint?)c.Serial,
                            name = c == null ? String.Empty : p.Name

                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook(int serial)
        {
            CheckedOut checkout = new CheckedOut() { CardNum = (uint)card, Serial = (uint)serial };
            db.CheckedOut.Add(checkout);
            db.SaveChanges();

            return Json(new { success = true });

        }

        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook(int serial)
        {
            CheckedOut? rowToDelete = db.CheckedOut.FirstOrDefault(c => c.CardNum == card && c.Serial == serial);
            if (rowToDelete != null)
            {
                db.CheckedOut.Remove(rowToDelete);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }


        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/


        public IActionResult Index()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }


        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}