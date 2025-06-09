using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLMS.Api.Models;
using OLMS.Api.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IBookService bookService,
            IEmailService emailService,
            IConfiguration config,
            ILogger<AdminController> logger)
        {
            _bookService = bookService;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("report")]
        public async Task<IActionResult> Report()
        {
            try
            {
                var report = await _bookService.GenerateReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { message = "An error occurred while generating the report." });
            }
        }
    }
}
