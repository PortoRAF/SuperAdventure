﻿using System;
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
			foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
			{
				if (!Inventory.Exists(a => a.Details.ID == questItem.Details.ID 
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

		public void AddMonsterRewards(Monster monster, List<Item> loot)
		{
			ExperiencePoints += monster.RewardExperiencePoints;
			Gold += monster.RewardGold;

			foreach (Item rewardItems in loot)
			if (Inventory.Exists(x => x.Details.ID == rewardItems.ID))
			{
				Inventory.Find(x => x.Details.ID == rewardItems.ID).Quantity += 1;
			}
			else
			{
				Inventory.Add(new InventoryItem(rewardItems, 1));
			}
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
	}
}
