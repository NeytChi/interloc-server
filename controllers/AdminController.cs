using System;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Serilog.Core;

using common;

namespace controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        Context context;
        AdminModule module;
        Logger log;
        public AdminController(Context context)
        {
            this.log = new LoggerConfiguration()
                .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            this.context = context;
            this.module = new AdminModule(log, context);
        }
        [HttpPost]
        [ActionName("Auth")]
        public ActionResult<dynamic> AuthToken(AdminCache cache)
        {
            Admin admin = null;
            string message = string.Empty, authToken;
            if ((authToken = module.AuthToken(cache, ref admin, ref message)) != null)
                return new { success = true, data = new { auth_token = authToken }};
            return Error500(message);
        }
        [HttpPost]
        [Authorize]
        [ActionName("CreateQuestion")]
        public ActionResult<dynamic> CreateQuestion(QuestionCache cache)
        {
            Question question;
            string message = string.Empty;
            if (!string.IsNullOrEmpty(cache.question)) {
                question = new Question() {
                    question = HttpUtility.UrlDecode(cache.question),
                    created_at = DateTimeOffset.UtcNow,
                    deleted = false
                };
                context.questions.Add(question);
                context.SaveChanges();
                log.Information("Create new question, id -> " + question.question_id);
                return new { success = true, data = new { question = question }};
            }
            else
                message = "Question can't be empty.";
            return Error500(message);
        }
        [HttpPut]
        [Authorize]
        [ActionName("UpdateQuestion")]
        public ActionResult<dynamic> UpdateQuestion(QuestionCache cache)
        {
            Question question;
            string message = string.Empty;
            if (!string.IsNullOrEmpty(cache.question)) {
                if ((question = context.questions.Where(q => q.question_id == cache.question_id && !q.deleted).FirstOrDefault()) != null) {
                    question.question = HttpUtility.UrlDecode(cache.question);
                    context.questions.Update(question);
                    context.SaveChanges();
                    log.Information("Update question, id -> " + question.question_id);
                    return new { success = true, data = new { question = question }};
                }
                else
                    message = "Server can't define question by id.";
            }
            else
                message = "Question can't be empty.";
            return Error500(message);
        }
        [HttpDelete]
        [Authorize]
        [ActionName("DeleteQuestion")]
        public ActionResult<dynamic> DeleteQuestion(QuestionCache cache)
        {
            Question question;
            string message = string.Empty;
            if ((question = context.questions.Where(q => q.question_id == cache.question_id && !q.deleted).FirstOrDefault()) != null) {
                question.deleted = true;
                context.questions.Update(question);
                context.SaveChanges();
                log.Information("Delete question, id -> " + question.question_id);
                return new { success = true };
            }
            else
                message = "Server can't define question by id.";
            return Error500(message);
        }
        public dynamic Error500(string message)
        {
            if (Response != null)
                Response.StatusCode = 500;
            
            log.Warning(message + " IP -> " + HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new { success = false, message = message };
        }
    }
}
