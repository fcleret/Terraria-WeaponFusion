using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponFusion.Items;

namespace WeaponFusion.Utils
{
    public static class MergeHelper
	{
		#region Methods

		// Check if an item can be demerge
		public static bool CanDemerge(Item item)
		{
			return IsCompatible(item)
				&& item.stack == 1
				&& !item.favorited
				&& Main.keyState.PressingControl();
		}

		// Check if an item can be merge
		public static bool CanMerge(Item item)
		{
			return IsCompatible(item)
				&& item.stack == 1
				&& !IsMaxLevel(item)
				&& GetCombustibleItems(item, Main.CurrentPlayer, true).Count > 0
				&& !Main.keyState.PressingControl();
		}

		// Demerge all items from one
		public static void DoDemerge(Item item, Player player, int recursiveCount)
		{
			if (GetLevel(item) > 1)
			{
				Item demergedItem = new();
				demergedItem.SetDefaults(item.netID);

				SetLevel(item, GetLevel(item) - 1);
				SetLevel(demergedItem, GetLevel(item));

				if (recursiveCount > 0)
				{
					DoDemerge(item, player, recursiveCount - 1);
					DoDemerge(demergedItem, player, recursiveCount - 1);
				}

				// Drop demerged item
				player.DropItem(demergedItem.GetSource_FromThis(), player.position, ref demergedItem);
			}
		}

		// 
		public static void DoMerge(Item item, Player player, bool isMergeAll)
		{
			if (isMergeAll)
			{
				List<Item> items = GetCombustibleItems(item, player).OrderByDescending(GetLevel).ToList();
				items.Add(item);
				items.Reverse();

				bool hasMerged = true;
				while (hasMerged)
				{
					hasMerged = false;

					List<int> levels = items.Select(GetLevel).Where(e => e < WeaponFusionConfig.Current.MaxLevel).Distinct().ToList();
					foreach (int level in levels)
					{
						List<Item> levelItems = items.Where(e => GetLevel(e) == level).ToList();
						int count = levelItems.Count;

						while (count > 1)
						{
							Item mergeItem = levelItems[0];
							Item combustibleItem = levelItems[1];

							SetLevel(mergeItem, GetLevel(mergeItem) + 1);
							combustibleItem.TurnToAir();

							levelItems.Remove(mergeItem);
							levelItems.Remove(combustibleItem);
							items.Remove(combustibleItem);

							count -= 2;
							hasMerged = true;
						}
					}
				}
			}
			else if (GetCombustibleItems(item, player, true).FirstOrDefault() is Item combustibleItem)
			{
					SetLevel(item, GetLevel(item) + 1);
					combustibleItem.TurnToAir();
			}
		}

		// Return all same items to merge on it
		public static List<Item> GetCombustibleItems(Item item, Player player, bool checkSameLevel = false)
		{
			List<Item> items = new();
			foreach (Item combustibleItem in player.inventory)
			{
				if (!combustibleItem.Equals(player.HeldItem)
					&& !combustibleItem.Equals(item)
					&& combustibleItem.netID == item.netID
					&& !combustibleItem.favorited)
				{
					if (checkSameLevel)
					{
						if (GetLevel(item) == GetLevel(combustibleItem))
						{
							items.Add(combustibleItem);
							break;
						}
					}
					else
					{
						items.Add(combustibleItem);
					}
				}
			}
			return items;
		}

		// Return stats influenced by leveling
		public static List<EStatType> GetCompatibleStats(Item item)
		{
			List<EStatType> compatibleStats = new();
			if (item.damage > 0)
			{
				compatibleStats.Add(EStatType.Damage);
				if (item.DamageType.UseStandardCritCalcs)
					compatibleStats.Add(EStatType.Critical);
				if (item.knockBack > 0)
					compatibleStats.Add(EStatType.Knockback);
			}
			if (item.defense > 0)
				compatibleStats.Add(EStatType.Defense);

			return compatibleStats;
		}

		// Return item's level
		public static int GetLevel(Item item)
		{
			return item.GetGlobalItem<ModGlobalItem>().level;
		}

        // Get multiplier based on item's level
        public static float GetMultiplicative(Item item, float multiplier)
		{
			return 1 + (multiplier * (GetLevel(item) - 1));
		}

        // Return value based on item's level
        public static int GetValueByLevel(int initialValue, int level, float multiplier)
		{
			int returnValue = (int)(initialValue + ((level - 1) * initialValue * multiplier));
			return returnValue < 0 ? 0 : returnValue;
		}

		// Check if an item can have levels
		public static bool IsCompatible(Item item)
		{
			bool isBlacklisted = WeaponFusionConfig.Current.Blacklist.Any(e => e.Type == item.netID);
			return item.netID != ItemID.None
				&& !isBlacklisted
				// && item.stack == 1
				&& GetCompatibleStats(item).Count > 0;
		}

		// Check if item's level is maxed
		public static bool IsMaxLevel(Item item)
		{
			return GetLevel(item) >= WeaponFusionConfig.Current.MaxLevel;
		}

		// Set level for item
		public static void SetLevel(Item item, int value)
		{
			item.GetGlobalItem<ModGlobalItem>().level = value;

			// Summon is a special class
			if (item.DamageType == DamageClass.Summon)
				item.damage = (int)(item.OriginalDamage * GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage));

			// Defense need to be update here
			if (item.OriginalDefense > 0)
				item.defense = GetValueByLevel(item.OriginalDefense, value, WeaponFusionConfig.Current.MultDefense);

			// Update item's price
			item.shopCustomPrice = Math.Min(Math.Pow(2, value - 1) * (double)item.value, int.MaxValue);
		}

        #endregion
    }

    #region Enums

    public enum EStatType
    {
        Damage,
        Defense,
        Critical,
        Knockback
    }

    #endregion
}
