using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    internal class KeyBindingDbContext : DbContext
    {
        private string _connectionString { get; set; }  

        public DbSet<CustomerDataBinding> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>("KeyBinding");

            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
