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

            // Assign labels text values to match player's attributes
            UpdatePlayerAttributesInUI();
            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            DisplayWeaponAndPotionListsInUI(false);
        }

        private void UpdatePlayerAttributesInUI()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            if (_player.Inventory.Count != 0) 
            { 
                foreach (InventoryItem item in _player.Inventory)
                {
                    dgvInventory.Rows.Add(new[] { item.Details.Name, item.Quantity.ToString() });
                }
            }
        }
        
        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Quest";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            if (_player.Quests.Count != 0)
            {
                foreach (PlayerQuest quest in _player.Quests)
                {
                    dgvQuests.Rows.Add(new[] { quest.Details.Name, quest.IsComplete?"Yes":"No" });
                }
            }
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
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.AppendText("You must have a " + newLocation.ItemRequiredToEnter.Name +
                    " to enter this location." + Environment.NewLine);
                rtbMessages.AppendText(Environment.NewLine);

                return;
            }

            UpdateLocation(newLocation);

            RestoreHitPoints();

            if (newLocation.QuestAvailableHere != null)
            {
                ProcessQuest(newLocation.QuestAvailableHere);
            }

            if (newLocation.MonsterLivingHere != null)
            {
                _currentMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                rtbMessages.AppendText("You see a " + _currentMonster.Name + Environment.NewLine);
                rtbMessages.AppendText(Environment.NewLine);

                DisplayWeaponAndPotionListsInUI(true);
            }
            else 
            {
                DisplayWeaponAndPotionListsInUI(false);
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
            rtbLocation.AppendText(newLocation.Description + Environment.NewLine);
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
            // add quest if player doesn't have it yet
            if (!_player.HasThisQuest(quest))
            {
                _player.Quests.Add(new PlayerQuest(quest));

                rtbMessages.AppendText("You received the " + quest.Name + " quest." + Environment.NewLine);
                rtbMessages.AppendText("To complete it, return with:" + Environment.NewLine);

                foreach (QuestCompletionItem item in quest.QuestCompletionItems)
                {
                    if (item.Quantity == 1)
                    {
                        rtbMessages.AppendText(item.Quantity.ToString() + " " + item.Details.Name + Environment.NewLine);
                    }
                    else
                    {
                        rtbMessages.AppendText(item.Quantity.ToString() + " " + item.Details.NamePlural + Environment.NewLine);
                    }
                }
                rtbMessages.AppendText(Environment.NewLine);
            }
            // otherwise check if player meets quest's completion requirements
            else
            {
                if (!_player.CompletedThisQuest(quest)
                    && _player.HasAllQuestCompletionItems(quest)) 
                {                    
                    CompleteQuest(quest);
                }
            }
            UpdateQuestListInUI();
        }

        private void CompleteQuest(Quest quest)
        {
            rtbMessages.AppendText("You completed the " + quest.Name + " quest." + Environment.NewLine);
            rtbMessages.AppendText("You receive: " + Environment.NewLine);
            rtbMessages.AppendText(quest.RewardExperiencePoints + " experience points" + Environment.NewLine);
            rtbMessages.AppendText(quest.RewardGold + " gold" + Environment.NewLine);
            rtbMessages.AppendText("1 " + quest.RewardItem.Name + Environment.NewLine);
            rtbMessages.AppendText(Environment.NewLine);

            _player.RemoveQuestCompletionItems(quest);
            _player.AddQuestRewards(quest);
            _player.MarkQuestCompleted(quest);

            UpdatePlayerAttributesInUI();
            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();
        }

        private void DisplayWeaponAndPotionListsInUI(bool isVisible)
        {
            if (!isVisible)
            {
                lblSelectAction.Visible = false;
                cboWeapons.Visible      = false;
                cboPotions.Visible      = false;
                btnUseWeapon.Visible    = false;
                btnUsePotion.Visible    = false;
            }
            else
            {
                lblSelectAction.Visible = UpdateWeaponListInUI();
                lblSelectAction.Visible |= UpdatePotionListInUI();
            }
        }

        private bool UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();
            foreach (InventoryItem item in _player.Inventory)
            {
                if (item.Details is Weapon && item.Quantity > 0)
                {
                    weapons.Add((Weapon)item.Details);
                }
            }
            if (weapons.Count != 0)
            {
                cboWeapons.DataSource       = weapons;
                cboWeapons.DisplayMember    = "Name";
                cboWeapons.ValueMember      = "ID";
                cboWeapons.SelectedIndex    = 0;
                cboWeapons.Visible          = true;
                btnUseWeapon.Visible        = true;
                return true;
            }
            else
            {
                cboWeapons.Visible      = false;
                btnUseWeapon.Visible    = false;
                return false;
            }
        }

        private bool UpdatePotionListInUI()
        {
            List<Item> healingPotions = new List<Item>();
            foreach (InventoryItem item in _player.Inventory)
            {
                if (item.Details.ID == World.ITEM_ID_HEALING_POTION && item.Quantity > 0)
                {
                    healingPotions.Add(item.Details);
                }
            }
            if (healingPotions.Count != 0)
            {
                cboPotions.DataSource       = healingPotions;
                cboPotions.DisplayMember    = "Name";
                cboPotions.ValueMember      = "ID";
                cboPotions.SelectedIndex    = 0;
                cboPotions.Visible          = true;
                btnUsePotion.Visible        = true;
                return true;
            }
            else
            {
                cboPotions.Visible      = false;
                btnUsePotion.Visible    = false;
                return false;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // get current selected weapon from combo box
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // calculate damage
            int damageToMonster = currentWeapon.GetWeaponDamage();

            // subtract inflicted damage to monster's hit points
            _currentMonster.CurrentHitPoints -= damageToMonster;

            rtbMessages.AppendText("You hit " + _currentMonster.Name + " for " +
                damageToMonster + " hit points." + Environment.NewLine + Environment.NewLine);

            if (_currentMonster.CurrentHitPoints <= 0)
            {
                MonsterDefeated();
            }
            else
            {
                DamageToPlayer();
            }
        }

        private void MonsterDefeated()
        {
            List<Item> lootedItems = GetMonsterLoot();
            _player.AddMonsterRewards(_currentMonster, lootedItems);
            
            rtbMessages.AppendText("You killed " + _currentMonster.Name + "!" + Environment.NewLine + Environment.NewLine);
            rtbMessages.AppendText("You loot: " + Environment.NewLine);
            rtbMessages.AppendText(_currentMonster.RewardExperiencePoints + " experience points" + Environment.NewLine);
            rtbMessages.AppendText(_currentMonster.RewardGold + " gold" + Environment.NewLine);

            foreach (Item items in lootedItems)
            {
                rtbMessages.AppendText("1 " + items.Name + Environment.NewLine);
            }
            rtbMessages.AppendText(Environment.NewLine);

            UpdatePlayerAttributesInUI();
            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();

            // move player to current location to restore hit points and create a new monster
            MoveTo(_player.CurrentLocation);
        }

        private List<Item> GetMonsterLoot()
        {
            List<Item> loot = new List<Item>();
            foreach (LootItem lootItems in _currentMonster.LootTable)
            {
                if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItems.DropPercentage)
                {
                    loot.Add(lootItems.Details);
                }
            }

            // if loot returned empty, add the default loot item
            if (loot.Count == 0)
            {
                foreach (LootItem lootItems in _currentMonster.LootTable)
                {
                    if (lootItems.IsDefaultItem)
                    {
                        loot.Add(lootItems.Details);
                    }
                }
            }

            return loot;
        }

        private void DamageToPlayer()
        {
            int damageToPlayer = _currentMonster.GetMonsterDamage();
            _player.CurrentHitPoints -= damageToPlayer;

            rtbMessages.AppendText(_currentMonster.Name + " did " + damageToPlayer.ToString() +
                " points of damage." + Environment.NewLine + Environment.NewLine);

            UpdatePlayerAttributesInUI();

            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.AppendText("You were defeated by " + _currentMonster.Name + "!" + Environment.NewLine + Environment.NewLine);
                rtbMessages.AppendText("Villagers found you unconscious and took you to your house." + Environment.NewLine + Environment.NewLine);

                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            rtbMessages.AppendText("You use 1 healing potion." + Environment.NewLine + Environment.NewLine);
            HealingPotion potion = (HealingPotion) cboPotions.SelectedItem;

            _player.CurrentHitPoints += potion.AmountToHeal;
            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.RemovePotion(potion);

            UpdatePlayerAttributesInUI();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();

            DamageToPlayer();
        }
    }
}
