using ITHelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpDesk.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IActiveDirectoryService _adService;

        public UsersController(IActiveDirectoryService adService)
        {
            _adService = adService;
        }

        // GET: api/Users/Search?term=john
        [HttpGet("Search")]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Ok(new List<object>());
            }

            var users = _adService.SearchUsers(term);
            var results = users.Select(u => new
            {
                username = u.Username,
                displayName = u.DisplayName,
                email = u.Email,
                label = $"{u.DisplayName} ({u.Username})"
            }).Take(10).ToList();

            return Ok(results);
        }

        // GET: api/Users/SearchComputers?term=workstation
        [HttpGet("SearchComputers")]
        public IActionResult SearchComputers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Ok(new List<object>());
            }

            var computers = _adService.SearchComputers(term);
            var results = computers.Select(c => new
            {
                name = c,
                label = c
            }).Take(10).ToList();

            return Ok(results);
        }
    }
}
