using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Api.Services;

namespace MiniBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BackgroundJobController : ControllerBase
    {
        private readonly IBackgroundJobService _jobService;

        public BackgroundJobController(IBackgroundJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost("generate-statement")]
        public IActionResult GenerateStatement([FromQuery] string accountNumber, [FromQuery] int month, [FromQuery] int year)
        {
            var jobId = _jobService.GenerateMonthlyStatement(accountNumber, month, year);
            return Ok(new { JobId = jobId, Message = "Statement generation started" });
        }

        [HttpPost("send-bulk-emails")]
        public IActionResult SendBulkEmails([FromBody] BulkEmailRequest request)
        {
            var jobId = _jobService.SendBulkEmailsToUsers(request.Emails, request.Subject, request.Body);
            return Ok(new { JobId = jobId, Message = "Bulk email job started" });
        }

        [HttpPost("schedule-daily-reports")]
        public IActionResult ScheduleDailyReports()
        {
            _jobService.ScheduleDailyReports();
            return Ok(new { Message = "Daily reports scheduled" });
        }
    }

    public class BulkEmailRequest
    {
        public List<string> Emails { get; set; } = new();
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}