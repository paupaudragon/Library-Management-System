/**
 * Author: Course Staff(setup code)
 *        Tingting Zhou 
 * Course: CS5530 2023 Spring
 * 
 */
using LibraryWebServer.Controllers;
using LibraryWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1
{
    /// <summary>
    /// This is the unit test class for Library Web Server
    /// </summary>
    public class UnitTest1
    {
        // Uncomment this once you have scaffolded your library, 
        // then replace instances of 'X' below with your team number

        Team28LibraryContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<Team28LibraryContext>()
            .UseInMemoryDatabase("LibraryControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new Team28LibraryContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Populate Titles
            Titles t = new Titles();
            t.Author = "Fake author";
            t.Title = "Fake title";
            t.Isbn = "123-5678901234";
            db.Titles.Add(t);
            db.SaveChanges();

            t.Author = "Fake author2";
            t.Title = "Fake title2";
            t.Isbn = "123-5678901235";
            db.Titles.Add(t);
            db.SaveChanges();

            // Populate Inventory 
            Inventory i = new Inventory();
            i.Isbn = "123-5678901234";
            i.Serial = 1001;
            db.Inventory.Add(i);
            db.SaveChanges();

            i.Isbn = "123-5678901234";
            i.Serial = 1002;
            db.Inventory.Add(i);
            db.SaveChanges();

            // Populate Patrons
            Patrons p = new Patrons();
            p.CardNum = 1;
            p.Name = "Joe";
            db.Patrons.Add(p);
            db.SaveChanges();

            p.CardNum = 2;
            p.Name = "Ann";
            db.Patrons.Add(p);
            db.SaveChanges();

            // Populate CheckedOut
            CheckedOut c = new CheckedOut();
            c.CardNum = 1;
            c.Serial = 1001;
            db.CheckedOut.Add(c);
            db.SaveChanges();


            return db;
        }

        [Fact]
        public void AllTitles()
        {
            HomeController c = new HomeController(null);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            var allTitlesResult = c.AllTitles() as JsonResult;

            dynamic x = allTitlesResult.Value;

            Assert.Equal(3, x.Length);
            Assert.Equal("123-5678901234", x[0].isbn);
            Assert.Equal("123-5678901234", x[1].isbn);
            Assert.Equal("123-5678901235", x[2].isbn);

        }

        [Fact]
        public void ListMyBooks_Empty()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Ann", 2);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            var allBooksOfAnn = c.ListMyBooks() as JsonResult;

            dynamic x = allBooksOfAnn.Value;

            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void ListMyBooks_NonEmpty()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Joe", 1);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            var allBooksOfJoe = c.ListMyBooks() as JsonResult;

            dynamic x = allBooksOfJoe.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("Joe", x[0].name);
            Assert.Equal((uint)1001, x[0].serial);
        }

        [Fact]
        public void CheckOutBooks_Empty()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Ann", 2);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.CheckOutBook(1002);
            var allBooksOfAnn = c.ListMyBooks() as JsonResult;
            dynamic x = allBooksOfAnn.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal((uint)1002, x[0].serial);
        }

        [Fact]
        public void CheckOutBooks_NonEmpty()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Joe", 1);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.CheckOutBook(1002);
            var allBooksOfJoe = c.ListMyBooks() as JsonResult;
            dynamic x = allBooksOfJoe.Value;

            Assert.Equal(2, x.Length);
            Assert.Equal((uint)1001, x[0].serial);
            Assert.Equal((uint)1002, x[1].serial);
        }

        [Fact]
        public void ReturnBook_NonEmpty()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Joe", 1);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.ReturnBook(1001);
            var allBooksOfJoe = c.ListMyBooks() as JsonResult;
            dynamic x = allBooksOfJoe.Value;

            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void ReturnBook_Illegal()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Ann", 2);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.ReturnBook(1001);
            var allBooksOfAnn = c.ListMyBooks() as JsonResult;
            dynamic x = allBooksOfAnn.Value;

            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void ReturnBook_NonExisting()
        {
            HomeController c = new HomeController(null);
            c.CheckLogin("Ann", 2);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.ReturnBook(1009);
            var allBooksOfAnn = c.ListMyBooks() as JsonResult;
            dynamic x = allBooksOfAnn.Value;

            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void Incorrect_UserName()
        {
            HomeController c = new HomeController(null);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            var login = c.CheckLogin("ann", 2) as JsonResult;
            dynamic x = login.Value;

            Assert.Equal(false, x.success);
        }

        [Fact]
        public void Incorrect_CardNum()
        {
            HomeController c = new HomeController(null);

            Team28LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            var login = c.CheckLogin("Joe", 2) as JsonResult;
            dynamic x = login.Value;

            Assert.Equal(false, x.success);
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }
    }
}