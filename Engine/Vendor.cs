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

            InventoryItem item = Inventory.SingleOrDefault(a => a.Details.ID == itemToAdd.ID);

            if (item == null)
            {                
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            } 
            else
            {
                item.Quantity += quantity;
            }

            OnPropertyChanged("Inventory");
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return;
            }

            InventoryItem item = Inventory.SingleOrDefault(a => a.Details.ID == itemToRemove.ID);
            
            if (item == null)
            {
                return;
            }
            else
            {
                item.Quantity -= quantity;

                if (item.Quantity <= 0)
                {
                    Inventory.Remove(item);
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
