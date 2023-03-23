
using Xunit;
using LibraryWebServer.Controllers;
using LibraryWebServer.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject
{
    private static ServiceProvider NewServiceProvider()
    {
        var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();
        return serviceProvider;
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Check_Login_Correct_Login()
        {

        }
    }
}