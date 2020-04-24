using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpPost]
        public ActionResult<dynamic> AuthToken()
        {
            return null;
        }
        [HttpPost]
        public ActionResult<dynamic> CreateQuestion()
        {
            return null;
        }
        [HttpPut]
        public ActionResult<dynamic> UpdateQuestion()
        {
            return null;
        }
        [HttpDelete]
        public ActionResult<dynamic> DeleteQuestion()
        {
            return null;
        }
    }
}
