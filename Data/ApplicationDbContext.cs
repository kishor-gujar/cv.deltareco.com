using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using cv.deltareco.com.Models;

namespace cv.deltareco.com.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, AppRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

     public DbSet<MyUser>MyUsers { get; set; }  
        public DbSet<CandidateCV> CandidateCVs { get; set; }
        public DbSet<CandidateProfile> CandidateProfiles { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

    }
}
