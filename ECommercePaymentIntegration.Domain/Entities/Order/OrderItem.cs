using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Domain.Entities.Product;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
    public class OrderItem : ProductBase
    {
        public int Quantity { get; set; }
        public decimal TotalPrice => Price;
    }
}
