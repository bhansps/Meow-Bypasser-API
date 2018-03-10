using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using AngleSharp.Dom.Css;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NaN_Api.Controllers
{
    [Route("[controller]")]
    public class BypassController : Controller
    {
        // GET api/values
        [HttpGet]
        public List<Link> Get()
        {
            return new List<Link>(new[]
            {
                new Link("/key/secret/", false)
            });
        }

        // POST api/values
        [HttpPost]
        public List<Link> Post([FromBody] List<Link> list)
        {
            List<Task> tasks = new List<Task>();
            var convertStatus = new Dictionary<int, bool>();
            var convertHelperClass = new Converter();
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].IsConverted)
                {
                    var index = i;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        convertHelperClass.Convert(list[index], 10, out var errorStatus, out var linkOut);
                        convertStatus.Add(index, errorStatus);
                        if (!convertStatus[index])
                        {
                            list[index] = linkOut;
                        }
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());
//          Console.WriteLine(convertStatus.ToString()); // debug
            return list;
        }
    }
}