using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
	public class Player : LivingCreature
	{
		private int _gold;
		private int _experiencePoints;
		public int Gold
		{
			get { return _gold; }
			set
			{
				_gold = value;
				OnPropertyChanged("Gold");
			}
		}
		public int ExperiencePoints
		{
			get { return _experiencePoints; }
			private set
			{
				_experiencePoints = value;
				OnPropertyChanged("ExperiencePoints");
				OnPropertyChanged("Level");
			}
		}
		public int Level { get; set; }
		public BindingList<InventoryItem> Inventory { get; set; }
		public BindingList<PlayerQuest> Quests { get; set; }
		public Location CurrentLocation { get; set; }
		public Weapon CurrentWeapon { get; set; }

		private Player(int currentHitPoints, int maximumHitPoints, int gold,
			int experiencePoints, int level) : base(currentHitPoints, maximumHitPoints)
		{
			Gold = gold;
			ExperiencePoints = experiencePoints;
			Level = level;

			Inventory = new BindingList<InventoryItem>();
			Quests = new BindingList<PlayerQuest>();
		}

		public static Player CreateDefaultPlayer()
		{
			Player player = new Player(10, 10, 20, 0, 1);
			player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
			player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_CLUB), 1));
			player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

			return player;
		}

		public static Player CreatePlayerFromXmlString(string xmlPlayerData)
		{
			try
			{
				XmlDocument playerData = new XmlDocument();

				playerData.LoadXml(xmlPlayerData);

				int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
				int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
				int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
				int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);
				int level = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Level").InnerText);

				Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints, level);

				int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
				player.CurrentLocation = World.LocationByID(currentLocationID);

				if(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
				{
					int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
					player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
				}

				foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
				{
					int id = Convert.ToInt32(node.Attributes["ID"].Value);
					int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

					player.Inventory.Add(new InventoryItem(World.ItemByID(id), quantity));
				}

				foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
				{
					int id = Convert.ToInt32(node.Attributes["ID"].Value);
					bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

					PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
					playerQuest.IsCompleted = isCompleted;

					player.Quests.Add(playerQuest);					
				}

				return player;
			}
			catch
			{
				// if there's an error loading xml file, create default player
				return CreateDefaultPlayer();
			}
		}

		public void RestoreHitPoints()
		{
			CurrentHitPoints = MaximumHitPoints;
		}

		public bool HasRequiredItemToEnterThisLocation(Location location)
		{
			if (location.ItemRequiredToEnter == null)
			{
				// there is no required item to enter this location, so return true
				return true;
			}

			return Inventory.Any(a => a.Details.ID == location.ItemRequiredToEnter.ID);
		}

		public bool HasThisQuest(Quest quest)
		{
			return Quests.Where(a => a.Details.ID == quest.ID).SingleOrDefault() != null;
		}

		public bool CompletedThisQuest(Quest quest)
		{
			return Quests.SingleOrDefault(a => a.Details.ID == quest.ID).IsCompleted;
		}

		public bool HasAllQuestCompletionItems(Quest quest)
		{
			foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
			{
				if (!Inventory.Any(a => a.Details.ID == questItem.Details.ID 
					&& a.Quantity >= questItem.Quantity))
				{
					return false;
				}
					
			}
			return true;
		}

		public void RemoveQuestCompletionItems(Quest quest)
		{
			List<int> markItemForDeletion = new List<int>();
			foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)  // (int i = 0; i < quest.QuestCompletionItems.Count; i++)
			{
				InventoryItem item = Inventory.SingleOrDefault(a => a.Details.ID == questItem.Details.ID);

				if (item != null)
				{
					item.Quantity -= questItem.Quantity;

					if (item.Quantity == 0)
					{
						Inventory.Remove(item);
					}
				}
			}
		}

		public void AddQuestRewards(Quest quest) 
		{
			AddExperiencePoints(quest.RewardExperiencePoints);
			Gold += quest.RewardGold;

			if (Inventory.Any(x => x.Details.ID == quest.RewardItem.ID))
			{
				Inventory.SingleOrDefault(x => x.Details.ID == quest.RewardItem.ID).Quantity += 1;
			}
			else
			{
				Inventory.Add(new InventoryItem(quest.RewardItem, 1));
			}
		}

		public void MarkQuestCompleted(Quest quest)
		{
			Quests.SingleOrDefault(a => a.Details.ID == quest.ID).IsCompleted = true;
		}

		public void AddMonsterRewards(Monster monster, List<Item> loot)
		{
			AddExperiencePoints(monster.RewardExperiencePoints);
			Gold += monster.RewardGold;

			foreach (Item rewardItems in loot)
			if (Inventory.Any(x => x.Details.ID == rewardItems.ID))
			{
				Inventory.SingleOrDefault(x => x.Details.ID == rewardItems.ID).Quantity += 1;
			}
			else
			{
				Inventory.Add(new InventoryItem(rewardItems, 1));
			}
		}

		public void AddExperiencePoints(int experiencePointsToAdd)
		{
			ExperiencePoints += experiencePointsToAdd;
			Level = (ExperiencePoints / 100) + 1;
			MaximumHitPoints = Level * 10;
		}

		public void RemovePotion(HealingPotion potion)
		{
			List<int> markItemForDeletion = new List<int>();
			for (int i = 0; i < Inventory.Count; i++)
			{
				if (Inventory[i].Details.ID == potion.ID)
				{
					Inventory[i].Quantity -= 1;
					if (Inventory[i].Quantity == 0)
					{
						markItemForDeletion.Add(i);
					}
				}
			}
			for (int i = markItemForDeletion.Count - 1; i >= 0; i--)
			{
				Inventory.RemoveAt(markItemForDeletion[i]);
			}
		}

		public string ToXMLString()
		{
			XmlDocument playerData = new XmlDocument();

			// create top-level node
			XmlNode player = playerData.CreateElement("Player");
			playerData.AppendChild(player);

			// create stats node
			XmlNode stats = playerData.CreateElement("Stats");
			player.AppendChild(stats);

			XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
			currentHitPoints.AppendChild(playerData.CreateTextNode(CurrentHitPoints.ToString()));
			stats.AppendChild(currentHitPoints);

			XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
			maximumHitPoints.AppendChild(playerData.CreateTextNode(MaximumHitPoints.ToString()));
			stats.AppendChild(maximumHitPoints);
			
			XmlNode gold = playerData.CreateElement("Gold");
			gold.AppendChild(playerData.CreateTextNode(Gold.ToString()));
			stats.AppendChild(gold);

			XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
			experiencePoints.AppendChild(playerData.CreateTextNode(Gold.ToString()));
			stats.AppendChild(experiencePoints);

			XmlNode playerLevel = playerData.CreateElement("Level");
			playerLevel.AppendChild(playerData.CreateTextNode(Level.ToString()));
			stats.AppendChild(playerLevel);

			XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
			currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.ID.ToString()));
			stats.AppendChild(currentLocation);

			if (CurrentWeapon != null)
			{
				XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
				currentWeapon.AppendChild(playerData.CreateTextNode(CurrentWeapon.ID.ToString()));
				stats.AppendChild(currentWeapon);
			}

			// create inventory node
			XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
			player.AppendChild(inventoryItems);

			foreach (InventoryItem item in Inventory)
			{
				XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

				XmlAttribute idAttribute = playerData.CreateAttribute("ID");
				idAttribute.Value = item.Details.ID.ToString();
				inventoryItem.Attributes.Append(idAttribute);

				XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
				quantityAttribute.Value = item.Quantity.ToString();
				inventoryItem.Attributes.Append(quantityAttribute);

				inventoryItems.AppendChild(inventoryItem);
			}

			// create player's quests node
			XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
			player.AppendChild(playerQuests);

			foreach (PlayerQuest quest in Quests)
			{
				XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

				XmlAttribute idAttribute = playerData.CreateAttribute("ID");
				idAttribute.Value = quest.Details.ID.ToString();
				playerQuest.Attributes.Append(idAttribute);

				XmlAttribute isCompletedAttibute = playerData.CreateAttribute("IsCompleted");
				isCompletedAttibute.Value = quest.IsCompleted.ToString();
				playerQuest.Attributes.Append(isCompletedAttibute);

				playerQuests.AppendChild(playerQuest);
			}

			return playerData.InnerXml;
		}
	}
}
