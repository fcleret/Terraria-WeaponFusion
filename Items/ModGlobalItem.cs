using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponFusion.Items
{
    public class ModGlobalItem : GlobalItem
	{
		private const string nsLocalization = "Mods.WeaponFusion.GlobalItem";
		private const int MaxRecursive = 5; // x! = 120
		public int level = 1;

		public override bool InstancePerEntity => true;

		#region Override Methods

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (CanHaveLevel(item))
			{
				string textLevelMax = IsMaxLevel(item) ? Language.GetTextValue($"{nsLocalization}.TooltipsLevelMax") : string.Empty;
                string textDetail = !Main.keyState.PressingShift() && GetLevel(item) > 1 ? Language.GetTextValue($"{nsLocalization}.TooltipsDetail") : string.Empty;

                TooltipLine line = new(Mod, "", Language.GetTextValue($"{nsLocalization}.TooltipsLevel", level, textLevelMax, textDetail))
				{
					OverrideColor = Color.Cyan
				};
				tooltips.Add(line);

				if (Main.keyState.PressingShift() && GetLevel(item) > 1)
				{
					foreach (EStatType statType in GetCompatibleStats(item))
					{
                        TooltipLine damage = new(Mod, "", Language.GetTextValue($"{nsLocalization}.TooltipsMult{statType}", GetMultiplactiveByStat(item, statType)))
						{
							OverrideColor = Color.Cyan
						};
                        tooltips.Add(damage);
                    }
                }
			}
		}

		public override bool CanRightClick(Item item)
		{
			return CanMerge(item) || CanUnmerge(item) || base.CanRightClick(item);
		}

		public override bool ConsumeItem(Item item, Player player)
        {
            return !CanHaveLevel(item) && base.ConsumeItem(item, player);
		}

		public override void RightClick(Item item, Player player)
        {
            if (Main.keyState.PressingControl())
			{
				UnmergeItem(item, player, Main.keyState.PressingShift() ? MaxRecursive : 0);
			}
			else
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
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            StatModifier modifier = new(1f, GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage));
			if (item.DamageType != DamageClass.Summon)
			{
                damage = damage.CombineWith(modifier);
            }

            if (Main.keyState.PressingShift())
				item.RebuildTooltip();
        }

		public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            crit *= GetMultiplicative(item, WeaponFusionConfig.Current.MultCrit);
        }

		public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            StatModifier modifier = new(1f, GetMultiplicative(item, WeaponFusionConfig.Current.MultKnockback));
            knockback = knockback.CombineWith(modifier);
        }

		public override void PostReforge(Item item)
		{
			if (item.DamageType == DamageClass.Summon || item.OriginalDefense > 0)
            {
                SetLevel(item, GetLevel(item));
            }

        }

		public override GlobalItem NewInstance(Item target)
        {
			GlobalItem globalItem = base.NewInstance(target);
            WeaponFusionConfig.ConfigurationChanged += () => OnConfigurationChanged(target);

            return globalItem;
		}

		public override void LoadData(Item item, TagCompound tag)
        {
            if (CanHaveLevel(item))
			{
                SetLevel(item, tag.Get<int>(nameof(level)));
                WeaponFusionConfig.ConfigurationChanged += () => OnConfigurationChanged(item);
            }
        }

		public override void SaveData(Item item, TagCompound tag)
		{
			if (CanHaveLevel(item))
				tag.Set(nameof(level), GetLevel(item));
            WeaponFusionConfig.ConfigurationChanged -= () => OnConfigurationChanged(item);
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

        public static bool CanHaveLevel(Item item)
        {
            bool isBlacklisted = WeaponFusionConfig.Current.Blacklist.Any(e => e.Type == item.netID);
            return item.netID != ItemID.None
                && !isBlacklisted
                && item.stack < 2
                && GetCompatibleStats(item).Count > 0;
        }

        public static void SetLevel(Item item, int value)
		{
			item.GetGlobalItem<ModGlobalItem>().level = value;

            if (item.DamageType == DamageClass.Summon)
            {
				item.damage = (int)(item.OriginalDamage * GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage));
            }

            double sellPrice = Math.Pow(2, value - 1) * item.value;
            item.shopCustomPrice = sellPrice > int.MaxValue ? int.MaxValue : (int)sellPrice;

			if (item.OriginalDefense > 0)
                item.defense = GetValueByLevel(item.OriginalDefense, value, WeaponFusionConfig.Current.MultDefense);
        }

		public static int GetLevel(Item item)
		{
			return item.GetGlobalItem<ModGlobalItem>().level;
        }

		private static void OnConfigurationChanged(Item item)
        {
            if (CanHaveLevel(item))
                SetLevel(item, GetLevel(item));
        }

        private static int GetValueByLevel(int initialValue, int level, float multiplier)
        {
            int returnValue = (int)(initialValue + ((level - 1) * initialValue * multiplier));
            return returnValue < 0 ? 0 : returnValue;
        }

        private static float GetMultiplicative(Item item, float multiplier)
        {
            return 1 + (multiplier * (GetLevel(item) - 1));
        }

        private static bool CanUnmerge(Item item)
        {
            return CanHaveLevel(item)
                && item.stack == 1
                && Main.keyState.PressingControl()
				&& !item.favorited;
        }

        private static bool CanMerge(Item item)
		{
            return CanHaveLevel(item)
				&& item.stack == 1
				&& !IsMaxLevel(item)
                && !Main.keyState.PressingControl()
                && (Main.keyState.PressingShift() || HasAnyInInventory(item));
		}

        private static List<EStatType> GetCompatibleStats(Item item)
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

		private static void UnmergeItem(Item item, Player player, int recursiveCount)
        {
            if (GetLevel(item) > 1)
			{
				Item unmergeItem = new();
                unmergeItem.SetDefaults(item.netID);

				SetLevel(item, GetLevel(item) - 1);
				SetLevel(unmergeItem, GetLevel(item));

				if (recursiveCount > 0)
                {
                    UnmergeItem(item, player, recursiveCount - 1);
					UnmergeItem(unmergeItem, player, recursiveCount - 1);
                }

                player.DropItem(unmergeItem.GetSource_FromThis(), player.position, ref unmergeItem);
            }
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

        private static string GetMultiplactiveByStat(Item item, EStatType stat)
        {
            return stat switch
            {
                EStatType.Damage => ((GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage) * 100) - 100).ToString("N0"),
                EStatType.Defense => ((GetMultiplicative(item, WeaponFusionConfig.Current.MultDefense) * 100) - 100).ToString("N0"),
                EStatType.Critical => ((GetMultiplicative(item, WeaponFusionConfig.Current.MultCrit) * 100) - 100).ToString("N0"),
                EStatType.Knockback => ((GetMultiplicative(item, WeaponFusionConfig.Current.MultKnockback) * 100) - 100).ToString("N0"),
                _ => string.Empty
            };
        }

        #endregion

        private enum EStatType
		{
			Damage,
			Defense,
			Critical,
			Knockback
		}
	}
}
