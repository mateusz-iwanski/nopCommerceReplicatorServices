﻿using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Services
{
    public interface ICustomerSourceData
    {
        CustomerCreatePLDto? GetCustomer(int customerId);
    }
}