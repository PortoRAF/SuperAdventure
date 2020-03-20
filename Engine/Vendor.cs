using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Vendor : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BindingList<InventoryItem> Inventory { get; private set; }

        public Vendor(string name)
        {
            Name = name;
            Inventory = new BindingList<InventoryItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return;
            }

            InventoryItem ii = Inventory.SingleOrDefault(a => a.Details.ID == itemToAdd.ID);

            if (ii == null)
            {                
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            } 
            else
            {
                ii.Quantity += quantity;
            }

            OnPropertyChanged("Inventory");
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return;
            }

            InventoryItem ii = Inventory.SingleOrDefault(a => a.Details.ID == itemToRemove.ID);
            
            if (ii == null)
            {
                return;
            }
            else
            {
                ii.Quantity -= quantity;

                if (ii.Quantity <= 0)
                {
                    Inventory.Remove(ii);
                }
            }

            OnPropertyChanged("Inventory");
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
