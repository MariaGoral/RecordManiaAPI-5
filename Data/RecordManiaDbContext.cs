using Microsoft.EntityFrameworkCore;
using RecordManiaAPI.Models;

namespace RecordManiaAPI.Data
{
    public class RecordManiaDbContext : DbContext
    {
        public RecordManiaDbContext(DbContextOptions<RecordManiaDbContext> options) : base(options) { }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
        public DbSet<Record> Records => Set<Record>();
    }
}