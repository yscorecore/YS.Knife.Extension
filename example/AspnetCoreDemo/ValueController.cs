using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
