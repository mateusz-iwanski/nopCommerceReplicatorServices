using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    public class KeyBindingDbContext : DbContext
    {
        private IServiceProvider _serviceProvider { get; set; }

        private string _connectionString { get; set; }  

        public DbSet<DataBindingEntity> DataBinding { get; set; }

        public KeyBindingDbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Change the table name
            modelBuilder.Entity<DataBindingEntity>().ToTable("DataBinding");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>("KeyBinding") ??
                    throw new CustomException($"In configuration DbConnectionStrings->KeyBinding not exists"); 

            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
