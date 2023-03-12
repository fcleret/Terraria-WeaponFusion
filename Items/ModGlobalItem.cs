using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponFusion.Items
{
    public class ModGlobalItem : GlobalItem
	{
		public override bool InstancePerEntity => true;
        public int level = 1;

		#region Override Methods

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (CanHaveLevel(item))
			{
				TooltipLine line = new(Mod, "", $"Level: {level}{(IsMaxLevel(item) ? " [Max]" : string.Empty)}")
				{
					OverrideColor = Color.Cyan
				};
				tooltips.Add(line);
			}
		}

		public override bool CanRightClick(Item item)
		{
            return (CanMerge(item) && (Main.keyState.PressingShift() || HasAnyInInventory(item)))
                || base.CanRightClick(item);
		}

		public override bool ConsumeItem(Item item, Player player)
		{
			return !CanHaveLevel(item) && base.ConsumeItem(item, player);
		}

		public override void RightClick(Item item, Player player)
        {
            if (Main.keyState.PressingShift())
            {
                CombineAllItem(item, player);
            }
            else if (GetCombustibleItems(item, player, true).FirstOrDefault() is Item combustibleItem)
            {
                SetLevel(item, GetLevel(item) + 1);
                combustibleItem.TurnToAir();
            }
        }

        public override void PostReforge(Item item)
		{
            if (CanHaveLevel(item))
			    SetLevel(item, GetLevel(item));
		}

		public override void LoadData(Item item, TagCompound tag)
        {
            if (CanHaveLevel(item))
                SetLevel(item, tag.Get<int>(nameof(level)));
		}

		public override void SaveData(Item item, TagCompound tag)
        {
            if (CanHaveLevel(item))
                tag.Set(nameof(level), GetLevel(item));
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            if (CanHaveLevel(item))
                SetLevel(item, reader.ReadInt32());
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            if (CanHaveLevel(item))
                writer.Write(GetLevel(item));
        }

        #endregion

        #region Custom Methods

        public static void SetLevel(Item item, int value)
		{
            /*
			 * To use in next patch
			 * 
            StatModifier modifier = new(1, WeaponFusionConfig.Current.MultDamage);
			modifier.ApplyTo(defaultItem.damage);
			*/

            item.GetGlobalItem<ModGlobalItem>().level = value;
			foreach (EStatType compatibleStats in GetCompatibleStats(item))
            {
                switch (compatibleStats)
                {
                    case EStatType.Damage:
                        item.damage = GetValueByLevel(item.OriginalDamage, value, WeaponFusionConfig.Current.MultDamage);
                        break;
                    case EStatType.Defense:
                        item.defense = GetValueByLevel(item.OriginalDefense, value, WeaponFusionConfig.Current.MultDefense);
                        break;
                }
            }
        }

        public static int GetLevel(Item item)
		{
			return item.GetGlobalItem<ModGlobalItem>().level;
		}

		private static int GetValueByLevel(int initialValue, int level, float multiplier)
		{
			int returnValue = (int)(initialValue + ((level - 1) * initialValue * multiplier));
			return returnValue < 0 ? 0 : returnValue;
		}

        private static bool CanMerge(Item item)
        {
            return CanHaveLevel(item) && item.stack == 1 && !IsMaxLevel(item);
        }

        private static bool CanHaveLevel(Item item)
        {
            bool isBlacklisted = WeaponFusionConfig.Current.Blacklist.Any(e => e.Type == item.netID);
            return item.IsCandidateForReforge && !isBlacklisted && GetCompatibleStats(item).Count > 0;
        }

		private static List<EStatType> GetCompatibleStats(Item item)
		{
            List<EStatType> compatibleStats = new();
			if (item.OriginalDamage > 0)
				compatibleStats.Add(EStatType.Damage);
            if (item.OriginalDefense > 0)
                compatibleStats.Add(EStatType.Defense);

			return compatibleStats;
        }

		private static bool HasAnyInInventory(Item item)
		{
			return GetCombustibleItems(item, Main.CurrentPlayer, true).Count > 0;
		}

        private static List<Item> GetCombustibleItems(Item item, Player player, bool checkSameLevel = false)
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

        private static void CombineAllItem(Item item, Player player)
        {
            List<Item> items = GetCombustibleItems(item, player).OrderByDescending(GetLevel).ToList();
            items.Add(item);
            items.Reverse();

            bool hasCombined = true;
            while (hasCombined)
            {
                hasCombined = false;

                List<int> levels = items.Select(GetLevel).Where(e => e < WeaponFusionConfig.Current.MaxLevel).Distinct().ToList();
                foreach (int level in levels)
                {
                    List<Item> levelItems = items.Where(e => GetLevel(e) == level).ToList();
                    int count = levelItems.Count;

                    while (count > 1)
                    {
                        Item mergeItem = levelItems[0];
                        Item conbustibleItem = levelItems[1];

                        SetLevel(mergeItem, GetLevel(mergeItem) + 1);
                        conbustibleItem.TurnToAir();

                        levelItems.Remove(mergeItem);
                        levelItems.Remove(conbustibleItem);
                        items.Remove(conbustibleItem);

                        count -= 2;
                        hasCombined = true;
                    }
                }
            }
        }

        private static bool IsMaxLevel(Item item)
		{
			return GetLevel(item) >= WeaponFusionConfig.Current.MaxLevel;
		}

        #endregion

		private enum EStatType
		{
			Damage,
			Defense
		}
    }
}
