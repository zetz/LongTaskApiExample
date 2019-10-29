using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using LongTaskApiExample.Data;
using LongTaskApiExample.Services;
using LongTaskApiExample.Providers;

namespace LongTaskApiExample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly LongTaskApiProvider _longTaskJobProvider;

        public IReadOnlyList<Message> Messages { get; private set; }


        public IndexModel(
            AppDbContext db,
            LongTaskApiProvider longTaskJobProvider
            )
        {
            _db = db;
            _longTaskJobProvider = longTaskJobProvider;
        }

        public async Task OnGetAsync()
        {
            Messages = await _db.Messages.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostAddTaskAsync(string msg, int loop, double delay)
        {
            await _longTaskJobProvider.RegistJob(msg, loop, delay);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAbortAsync(string id)
        {
            await _longTaskJobProvider.CancelJob(id);
            return RedirectToPage();
        }
    }
}
