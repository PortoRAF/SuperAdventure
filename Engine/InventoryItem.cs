using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class InventoryItem
    {
        public Item Details { get; set; }
        public int Quantity { get; set; }
        public int ItemID
        {
            get { return Details.ID; }
        }
        public string Description
        {
            get { return Details.Name; }
        }
        public double Price
        {
            get { return Details.Price; }
        }

        public InventoryItem(Item details, int quantity)
        {
            Details = details;
            Quantity = quantity;
        }
    }
}
