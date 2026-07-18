using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Center
{
    [Authorize(Roles = "Center")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;

        public DashboardModel(AppDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }
    }
}
