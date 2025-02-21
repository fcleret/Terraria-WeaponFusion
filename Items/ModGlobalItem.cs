using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WeaponFusion.Utils;

namespace WeaponFusion.Items
{
    public class ModGlobalItem : GlobalItem
	{
		#region Fields

		private const string NsLocalization = "Mods.WeaponFusion.GlobalItem";
		private const int UnmergeRecursivityMax = 4;
		public int level = 1;

		#endregion

		#region Properties

		public override bool InstancePerEntity => true;

		#endregion

		#region Methods

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (MergeHelper.IsCompatible(item))
			{
				bool isShifting = Main.keyState.PressingShift();
				int level = MergeHelper.GetLevel(item);

				string strLevelMax = MergeHelper.IsMaxLevel(item) ? Language.GetTextValue($"{NsLocalization}.TooltipsLevelMax") : string.Empty;
				string strDetail = level > 1 && !isShifting ? Language.GetTextValue($"{NsLocalization}.TooltipsDetail") : string.Empty;
				string strLevel = Language.GetTextValue($"{NsLocalization}.TooltipsLevel", level, strLevelMax, strDetail);

				// Add item's level
				TooltipLine line = new(Mod, string.Empty, strLevel) { OverrideColor = Color.Cyan };
				tooltips.Add(line);

				if (level > 1 && isShifting)
				{
                    foreach (EStatType statType in MergeHelper.GetCompatibleStats(item))
					{
						string strMultiplier = Language.GetTextValue($"{NsLocalization}.TooltipsMult{statType}", GetMultiplicativeByStat(item, statType));
						
						// Add item's stats modifiers
						TooltipLine damage = new(Mod, string.Empty, strMultiplier) { OverrideColor = Color.Cyan };
						tooltips.Add(damage);
					}
				}
			}
		}

		// Check if item can be merge or unmerge (on Right-Click)
		public override bool CanRightClick(Item item)
		{
			return MergeHelper.CanMerge(item) || MergeHelper.CanDemerge(item) || base.CanRightClick(item);
		}

		public override bool ConsumeItem(Item item, Player player)
		{
			return !MergeHelper.IsCompatible(item) && base.ConsumeItem(item, player);
		}

		public override void RightClick(Item item, Player player)
		{
			bool isControl = Main.keyState.PressingControl();
			bool isShifting = Main.keyState.PressingShift();

			if (isControl)
				MergeHelper.DoDemerge(item, player, isShifting ? UnmergeRecursivityMax : 0);
			else
				MergeHelper.DoMerge(item, player, isShifting);
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
		{
			StatModifier modifier = new(1f, MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage));
			if (item.DamageType != DamageClass.Summon)
				damage = damage.CombineWith(modifier);

			if (Main.keyState.PressingShift())
				item.RebuildTooltip();
		}

		public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
		{
			crit *= MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultCrit);
		}

		public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
		{
			StatModifier modifier = new(1f, MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultKnockback));
			knockback = knockback.CombineWith(modifier);
		}

		public override void PostReforge(Item item)
		{
			if (item.DamageType == DamageClass.Summon || item.OriginalDefense > 0)
				MergeHelper.SetLevel(item, MergeHelper.GetLevel(item));
		}

		public override GlobalItem NewInstance(Item target)
		{
			return base.NewInstance(target);
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			if (MergeHelper.IsCompatible(item) && tag.ContainsKey(nameof(level)))
				MergeHelper.SetLevel(item, tag.Get<int>(nameof(level)));
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (MergeHelper.IsCompatible(item))
				tag.Set(nameof(level), MergeHelper.GetLevel(item));
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			if (MergeHelper.IsCompatible(item))
				MergeHelper.SetLevel(item, reader.ReadInt32());
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			if (MergeHelper.IsCompatible(item))
				writer.Write(MergeHelper.GetLevel(item));
		}

		private static string GetMultiplicativeByStat(Item item, EStatType stat)
		{
			return stat switch
			{
				EStatType.Damage => ((MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultDamage) * 100) - 100).ToString("N0"),
				EStatType.Defense => ((MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultDefense) * 100) - 100).ToString("N0"),
				EStatType.Critical => ((MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultCrit) * 100) - 100).ToString("N0"),
				EStatType.Knockback => ((MergeHelper.GetMultiplicative(item, WeaponFusionConfig.Current.MultKnockback) * 100) - 100).ToString("N0"),
				_ => string.Empty
			};
		}

		#endregion
	}
}
