using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        // Create player and monster objects on class scope
        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            InitializeComponent();

            // player initial stats, items and location
            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }

            // Assign labels text values to match player's attributes
            lblHitPoints.DataBindings.Add("Text", _player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", _player, "Gold");
            lblExperience.DataBindings.Add("Text", _player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", _player, "Level");
            
            MoveTo(_player.CurrentLocation);
            
            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            DisplayWeaponAndPotionListsInUI(_player.CurrentLocation.MonsterLivingHere != null);
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = _player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = _player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });
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

        private void btnTrade_Click(object sender, EventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(_player);
            tradingScreen.StartPosition = FormStartPosition.CenterParent;
            tradingScreen.ShowDialog(this);
        }

        private void MoveTo(Location newLocation)
        {
            if (!_player.HasRequiredItemToEnter(newLocation))
            {
                rtbMessages.AppendText("You must have a " + newLocation.ItemRequiredToEnter.Name +
                    " to enter this location." + Environment.NewLine);
                rtbMessages.AppendText(Environment.NewLine);

                return;
            }

            UpdateLocation(newLocation);

            _player.RestoreHitPoints();

            btnTrade.Visible = newLocation.VendorWorkingHere != null;

            if (newLocation.HasAQuest)
            {
                ProcessQuest(newLocation.QuestAvailableHere);
            }

            if (newLocation.HasAMonster)
            {
                ProcessMonster(newLocation.MonsterLivingHere);                
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

            UpdateWeaponListInUI();
            UpdatePotionListInUI();
        }

        private void ProcessMonster(Monster monsterLivingHere)
        {
            // load monster stats from Monsters list
            Monster sampleMonster = World.MonsterByID(monsterLivingHere.ID);

            // create new monster from loaded stats
            _currentMonster = new Monster(sampleMonster.ID, sampleMonster.Name, sampleMonster.MaximumDamage,
                sampleMonster.RewardExperiencePoints, sampleMonster.RewardGold, sampleMonster.CurrentHitPoints,
                sampleMonster.MaximumHitPoints);
            foreach (LootItem lootItem in sampleMonster.LootTable)
            {
                _currentMonster.LootTable.Add(lootItem);
            }

            rtbMessages.AppendText("You see a " + _currentMonster.Name + Environment.NewLine);
            rtbMessages.AppendText(Environment.NewLine);

            DisplayWeaponAndPotionListsInUI(true);
        }

        private void DisplayWeaponAndPotionListsInUI(bool isVisible)
        {
            if (isVisible)
            {
                lblSelectAction.Visible = UpdateWeaponListInUI();
                lblSelectAction.Visible |= UpdatePotionListInUI();
            }
            else
            {
                lblSelectAction.Visible = false;
                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
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
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged;
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";
                cboWeapons.Visible = true;
                btnUseWeapon.Visible = true;

                if (_player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = _player.CurrentWeapon;
                }
                else
                {
                    cboWeapons.SelectedItem = 0;
                }

                return true;
            }
            else
            {
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
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
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
                cboPotions.Visible = true;
                btnUsePotion.Visible = true;
                return true;
            }
            else
            {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
                return false;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // get current selected weapon from combo box and calculate damage
            // subtract inflicted damage to monster's hit points
            _currentMonster.CurrentHitPoints -= ((Weapon)cboWeapons.SelectedItem).GetWeaponDamage();

            rtbMessages.AppendText("You hit " + _currentMonster.Name + " for " + 
                ((Weapon)cboWeapons.SelectedItem).GetWeaponDamage() + " hit points." + Environment.NewLine + Environment.NewLine);

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
            _player.AddMonsterRewards(_currentMonster, GetMonsterLoot());

            rtbMessages.AppendText("You killed " + _currentMonster.Name + "!" + Environment.NewLine + Environment.NewLine);
            rtbMessages.AppendText("You loot: " + Environment.NewLine);
            rtbMessages.AppendText(_currentMonster.RewardExperiencePoints + " experience points" + Environment.NewLine);
            rtbMessages.AppendText(_currentMonster.RewardGold + " gold" + Environment.NewLine);

            foreach (Item items in GetMonsterLoot())
            {
                rtbMessages.AppendText("1 " + items.Name + Environment.NewLine);
            }
            rtbMessages.AppendText(Environment.NewLine);

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
            _player.CurrentHitPoints -= _currentMonster.GetMonsterDamage();

            rtbMessages.AppendText(_currentMonster.Name + " did " + _currentMonster.GetMonsterDamage().ToString() +
                " points of damage." + Environment.NewLine + Environment.NewLine);

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

            _player.CurrentHitPoints += ((HealingPotion)cboPotions.SelectedItem).AmountToHeal;
            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.RemovePotion((HealingPotion)cboPotions.SelectedItem);

            UpdatePotionListInUI();

            DamageToPlayer();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXMLString());
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            WorldMap mapScreen = new WorldMap();
            mapScreen.StartPosition = FormStartPosition.CenterParent;
            mapScreen.ShowDialog(this);
        }
    }
}
