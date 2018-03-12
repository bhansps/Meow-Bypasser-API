using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using AngleSharp.Dom.Css;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Meow_Api.Controllers
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
            try
            {

                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].IsConverted) continue;
                    var index = i;
                    tasks.Add(Task.Run(() =>
                    {
                        convertHelperClass.Convert(list[index], 10, out var errorStatus, out var linkOut);
                        convertStatus.Add(index, errorStatus);
                        if (!convertStatus[index])
                        {
                            list[index] = linkOut;
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                //          Console.WriteLine(convertStatus.ToString()); // debug
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}\nMessage : {e.Message}");
            }
            return list;
        }
    }
}