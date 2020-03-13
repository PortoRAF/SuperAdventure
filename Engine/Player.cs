using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class Player : LivingCreature
	{
		public int Gold { get; set; }
		public int ExperiencePoints { get; set; }
		public int Level { get; set; }
		public List<InventoryItem> Inventory { get; set; }
		public List<PlayerQuest> Quests { get; set; }
		public Location CurrentLocation { get; set; }

		public Player(int currentHitPoints, int maximumHitPoints, int gold,
			int experiencePoints, int level) : base(currentHitPoints, maximumHitPoints)
		{
			Gold = gold;
			ExperiencePoints = experiencePoints;
			Level = level;

			Inventory = new List<InventoryItem>();
			Quests = new List<PlayerQuest>();
		}

		public bool HasRequiredItemToEnterThisLocation(Location location)
		{
			if (location.ItemRequiredToEnter == null)
			{
				// there is no required item to enter this location, so return true
				return true;
			}

			return Inventory.Exists(a => a.Details.ID == location.ItemRequiredToEnter.ID);
		}

		public bool HasThisQuest(Quest quest)
		{
			return Quests.Exists(a => a.Details.ID == quest.ID);
		}

		public bool CompletedThisQuest(Quest quest)
		{
			return Quests.Find(a => a.Details.ID == quest.ID).IsComplete;
		}

		public bool HasAllQuestCompletionItems(Quest quest)
		{
			bool hasAllQuestItems = true;
			foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
			{
				bool foundItemInInventory = false;
				foreach (InventoryItem playerItem in Inventory)
				{
					if (playerItem.Details.ID == questItem.Details.ID 
						&& playerItem.Quantity >= questItem.Quantity)
					{
						foundItemInInventory = true;
					}
				}
				hasAllQuestItems &= foundItemInInventory;
			}
			return hasAllQuestItems;
		}

		public void RemoveQuestCompletionItems(Quest quest)
		{
			foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
			{
				foreach (InventoryItem playerItem in Inventory)
				{
					if (playerItem.Details.ID == questItem.Details.ID)
					{
						playerItem.Quantity -= questItem.Quantity;
					}
				}
			}
		}

		public void AddQuestRewards(Quest quest) 
		{
			ExperiencePoints += quest.RewardExperiencePoints;
			Gold += quest.RewardGold;

			if (Inventory.Exists(x => x.Details.ID == quest.RewardItem.ID))
			{
				Inventory.Find(x => x.Details.ID == quest.RewardItem.ID).Quantity += 1;
			}
			else
			{
				Inventory.Add(new InventoryItem(quest.RewardItem, 1));
			}
		}

		public void MarkQuestCompleted(Quest quest)
		{
			Quests.Find(a => a.Details.ID == quest.ID).IsComplete = true;
		}
	}
}
