using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nopCommerceReplicatorServices.Actions;
using nopCommerceReplicatorServices.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.CommandOptions
{
    public class ExternalProductDisplayOptions
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ExternalProductDisplayOptions(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }
        public async Task ShowProductAsync(string serviceToReplicate, int shProductIdOption, bool showDetailsOption)
        {
            var productService = _configuration.GetSection("Service").GetSection(serviceToReplicate).GetValue<string>("Product") ??
                    throw new CustomException($"In configuration Service->{serviceToReplicate}->Product not exists"); 

            using (var scope = _serviceProvider.CreateScope())
            {
                IProductSourceData productDataSourceService = scope.ServiceProvider.GetRequiredService<Func<string, IProductSourceData>>()(productService);

                var productDto = await productDataSourceService.GetByIdAsync(shProductIdOption);

                if (productDto != null)
                    Console.WriteLine($"Response: {productDto.ToString()}");
            }
        }

    }
}
