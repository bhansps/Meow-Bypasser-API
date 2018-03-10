using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NaN_Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public List<Link> Index()
        {
            return new List<Link>(new[]
            {
                new Link("http://A.Rabbit.Is", true),
                new Link("https://Hiding.Behind.The/Bush/", false),
                new Link("http://Where.Is.The", true),
                new Link("/../../secret/path", false),
            });
        }
    }
}