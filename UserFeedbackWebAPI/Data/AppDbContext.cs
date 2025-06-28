using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserFeedbackWebAPI.Models;

namespace UserFeedbackWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<FeedBack> Feedbacks { get; set; }
    }
}
