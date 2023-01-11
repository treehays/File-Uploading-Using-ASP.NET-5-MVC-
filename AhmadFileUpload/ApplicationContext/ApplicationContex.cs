using AhmadFileUpload.Models;
using Microsoft.EntityFrameworkCore;

namespace AhmadFileUpload.ApplicationContext
{
    public class ApplicationContex : DbContext 
    {
        public ApplicationContex(DbContextOptions<ApplicationContex> options) : base(options)
        {

        }
        public DbSet<FileOffline> FileOfflines{ get; set; }
        public DbSet<FileOnline> FileOnlines { get; set; }
    }
}
