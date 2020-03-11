using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        // Create player and monster objects on class scope
        private Player _player;
        private Monster _currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            // player initial stats, items and location
            _player = new Player(10, 10, 20, 0, 1);
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));

            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RAT_TAIL), 5));

            // Assign labels text values to match player's attributes
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            if (newLocation.ItemRequiredToEnter != null)
            {
                bool hasKey = false;

                foreach (InventoryItem item in _player.Inventory)
                {
                    if (item.Details.Name == newLocation.ItemRequiredToEnter.Name)
                    {
                        hasKey = true;
                        break;
                    }
                }

                if (!hasKey)
                {
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name +
                        " to enter this location." + Environment.NewLine;
                    return;
                }
            }

            UpdateLocation(newLocation);

            RestoreHitPoints();

            ProcessQuest(newLocation.QuestAvailableHere);

            if (newLocation.MonsterLivingHere != null)
            {

            }
        }

        private void UpdateLocation(Location newLocation)
        {
            // Update location
            _player.CurrentLocation = newLocation;

            // Toggle buttons visibility
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;
        }

        private void RestoreHitPoints()
        {
            // Restore player's hit points
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            // Update hit points label
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
        }

        private void ProcessQuest(Quest quest)
        {
            if (quest != null)
            {
                if (!_player.Quests.Exists(a => a.Details.ID == quest.ID))
                {
                    _player.Quests.Add(new PlayerQuest(quest));

                    rtbMessages.Text += "You received the " + quest.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    
                    foreach (QuestCompletionItem item in quest.QuestCompletionItems)
                    {
                        if (item.Quantity == 1)
                        {
                            rtbMessages.Text += item.Quantity.ToString() + " " + item.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += item.Quantity.ToString() + " " + item.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;
                }
                else
                {
                    int index = _player.Quests.FindIndex(a => a.Details.Name == quest.Name);

                    if (!_player.Quests[index].IsComplete)
                    {
                        bool hasItems = true;
                        bool foundItem = false;

                        foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
                        {
                            foundItem = false;

                            foreach (InventoryItem playerItem in _player.Inventory)
                            {
                                if (playerItem.Details.Name == questItem.Details.Name && playerItem.Quantity >= questItem.Quantity)
                                {
                                    foundItem = true;
                                }
                            }

                            hasItems = hasItems & foundItem;
                        }

                        if (hasItems)
                        {
                            _player.Quests[index].IsComplete = true;
                            CompleteQuest(quest);                           
                        }
                    }
                }
            }
        }

        private void CompleteQuest(Quest quest)
        {
            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "You completed the " + quest.Name + " quest." + Environment.NewLine;

            foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
            {
                foreach (InventoryItem playerItem in _player.Inventory)
                {
                    if (playerItem.Details.Name == questItem.Details.Name)
                    {
                        playerItem.Quantity -= questItem.Quantity;
                    }
                }
            }

            rtbMessages.Text += "You receive: " + Environment.NewLine;
            rtbMessages.Text += quest.RewardExperiencePoints + " experience points" + Environment.NewLine;
            rtbMessages.Text += quest.RewardGold + " gold" + Environment.NewLine;
            rtbMessages.Text += quest.RewardItem.Name + Environment.NewLine;
            rtbMessages.Text += Environment.NewLine;

            _player.ExperiencePoints += quest.RewardExperiencePoints;
            _player.Gold += quest.RewardGold;

            if (_player.Inventory.Exists(x => x.Details.ID == quest.RewardItem.ID))
            {
                int i = _player.Inventory.FindIndex(x => x.Details.ID == quest.RewardItem.ID);
                _player.Inventory[i].Quantity += 1;
            }
            else
            {
                _player.Inventory.Add(new InventoryItem(quest.RewardItem, 1));
            }
        }
    }
}
