using System;
using Microsoft.EntityFrameworkCore;

namespace LongTaskApiExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<CancelJob> CancelJobs { get; set; }
    }
}
