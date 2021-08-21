
using System.Collections.Generic;
using System.Linq;

namespace Basket.API.Entities
{
    public class ShoppingCart
    {
         public string UserName { get; set; }
         public List<ShoppingCartItem> Items { get; set; } 
         public decimal TotalPrice => Items.Select(p=> p.Quantity * p.Price).Sum();

         public ShoppingCart(string userName)
         {
             UserName = userName;
             Items = new List<ShoppingCartItem>();
         }
    }
}