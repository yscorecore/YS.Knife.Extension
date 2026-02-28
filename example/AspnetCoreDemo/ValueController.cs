using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.Hosting.Web.Filters;

namespace AspnetCoreDemo
{
    [ApiController]
    [Route("[controller]")]
    public class ValueController : ControllerBase
    {
        [HttpPost]
        public Task Test([FromBody] Input req)
        {
            return Task.CompletedTask;
        }

        [HttpPost]
        [Route(nameof(TestOutput))]
        public Task<int> TestOutput([FromBody] Input req)
        {
            return Task.FromResult(0);
        }
        [HttpPost]
        [WrapCodeResultIgnore]
        [Route(nameof(TestOutputIgnore))]
        public Task<int> TestOutputIgnore([FromBody] Input req)
        {
            return Task.FromResult(0);
        }
    }
    public class Input
    {
        [Display(Name = "值1")]
        [System.ComponentModel.DataAnnotations.GreatThan(nameof(Value2))]
        public int Value1 { get; set; }
        [Display(Name = "值2")]
        public int Value2 { get; set; }
    }
}
