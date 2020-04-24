using System;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using Serilog;
using Serilog.Core;

namespace common
{
    [Route("/[action]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        Context context;
        Logger log;
        public QuestionController(Context context)
        {
            this.log = new LoggerConfiguration()
                .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            this.context = context;
        }
        [HttpGet]
        [ActionName("Questions")]
        public ActionResult<dynamic> Questions()
        {
            log.Information("Get all questions");
            return new { success = true, data = new { questions = context.questions.Where(q => !q.deleted).ToArray() } };
        }
        [HttpGet("{id}")]
        [ActionName("QuestionById")]
        public ActionResult<dynamic> QuestionById(int id)
        {
            Question question;
            string message = string.Empty;

            if ((question = context.questions.Where(q => q.question_id == id && !q.deleted).FirstOrDefault()) != null) {
                log.Information("Get question by id -> " + id);
                return new { success= true, data = new { question = question } };
            }
            else 
                message = "Server can't define question by id.";
            return Error500(message);
        }
        [HttpGet("{id}")]
        [ActionName("AnswersByQuestion")]
        public ActionResult<dynamic> Answers(int id)
        {
            log.Information("Get answers on question, id -> " + id);
            return new { success = true, data = new { answers = context.answers.Where(a => a.question_id == id && !a.deleted).ToArray() }};
        }
        [HttpPost]
        [ActionName("SetAnswer")]
        public ActionResult<dynamic> SetAnswer(QuestionCache cache)
        {
            Answer answer;
            Question question;
            string message = string.Empty;
            if (!string.IsNullOrEmpty(cache.answer)) {
                if ((question = context.questions.Where(q => q.question_id == cache.question_id && !q.deleted).FirstOrDefault()) != null) {
                    answer = new Answer() {
                        answer = HttpUtility.UrlDecode(cache.answer),
                        created_at = DateTimeOffset.UtcNow,
                        deleted = false
                    };
                    context.answers.Add(answer);
                    context.SaveChanges();
                    log.Information("Create new answer, id -> " + answer.answer_id);
                    return new { success = true, data = new { answer = answer} };
                }
                else
                    message = "Server can't define question by id.";
            }
            else
                message = "Answer can't be empty.";
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
