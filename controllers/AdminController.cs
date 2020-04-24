using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
        public ActionResult<dynamic> CreateQuestion()
        {
            return null;
        }
        [HttpPut]
        [Authorize]
        public ActionResult<dynamic> UpdateQuestion()
        {
            return null;
        }
        [HttpDelete]
        [Authorize]
        public ActionResult<dynamic> DeleteQuestion()
        {
            return null;
        }
    }
}
