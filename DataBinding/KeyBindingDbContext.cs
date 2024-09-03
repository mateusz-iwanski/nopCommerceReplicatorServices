using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.DataBinding
{
    internal class KeyBindingDbContext : DbContext
    {
        private IServiceProvider _serviceProvider { get; set; }

        private string _connectionString { get; set; }  

        public DbSet<CustomerDataBinding> Customers { get; set; }

        public KeyBindingDbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>("KeyBinding");

            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
