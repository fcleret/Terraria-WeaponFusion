using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
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
			if (CheckCompatibility(item) != CompatibleType.None)
			{
				TooltipLine line = new(Mod, "", $"Level: {level}")
				{
					OverrideColor = Color.Cyan
				};
				tooltips.Add(line);
			}
		}

		public override bool CanRightClick(Item item)
		{
			return CheckCompatibility(item) != CompatibleType.None || base.CanRightClick(item);
		}

		public override bool ConsumeItem(Item item, Player player)
		{
			return CheckCompatibility(item) == CompatibleType.None && base.ConsumeItem(item, player);
		}

		public override void RightClick(Item item, Player player)
        {
            bool isUnderMaxLevel = GetLevel(item) < (int)GetConfigValue(nameof(WeaponFusionConfig.MaxLevel));
            if (isUnderMaxLevel && GetCombustibleItem(item, player) is Item combustibleItem)
			{
				SetLevel(item, GetLevel(item) + 1);
				combustibleItem.TurnToAir();
			}
			else
			{
				base.RightClick(item, player);
			}
		}

		public override void PostReforge(Item item)
		{
			SetLevel(item, GetLevel(item));
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			SetLevel(item, tag.Get<int>(nameof(level)));
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			tag.Set(nameof(level), GetLevel(item));
		}

		#endregion

		#region Custom Methods

		public static void SetLevel(Item item, int value)
		{
			Item defaultItem = new();
			defaultItem.SetDefaults(item.netID);

			item.GetGlobalItem<ModGlobalItem>().level = value;

			switch (CheckCompatibility(item))
			{
				case CompatibleType.Both:
					item.damage = GetValueByLevel(defaultItem.damage, value, (float)GetConfigValue(nameof(WeaponFusionConfig.MultDamage)));
					item.defense = GetValueByLevel(defaultItem.defense, value, (float)GetConfigValue(nameof(WeaponFusionConfig.MultDefense)));
					break;
				case CompatibleType.Damage:
					item.damage = GetValueByLevel(defaultItem.damage, value, (float)GetConfigValue(nameof(WeaponFusionConfig.MultDamage)));
					break;
				case CompatibleType.Defense:
					item.defense = GetValueByLevel(defaultItem.defense, value, (float)GetConfigValue(nameof(WeaponFusionConfig.MultDefense)));
					break;
				default:
					break;
			}
		}

		public static int GetLevel(Item item)
		{
			return item.GetGlobalItem<ModGlobalItem>().level;
		}

		private static object GetConfigValue(string valueName)
		{
			return WeaponFusionConfig.Current.GetType().GetField(valueName).GetValue(WeaponFusionConfig.Current);
		}

		private static int GetValueByLevel(int initialValue, int level, float multiplier)
		{
			int returnValue = (int)(initialValue + ((level - 1) * initialValue * multiplier));
			return returnValue < 0 ? 0 : returnValue;
		}

		private static Item GetCombustibleItem(Item item, Player player)
		{
			if (CheckCompatibility(item) != CompatibleType.None)
			{
				foreach (Item itemSlot in player.inventory)
				{
					if (!itemSlot.Equals(player.HeldItem)
						&& !itemSlot.Equals(item)
						&& itemSlot.netID == item.netID
						&& GetLevel(itemSlot) == GetLevel(item))
					{
						return itemSlot;
					}
				}
			}
			return null;
		}

		private static CompatibleType CheckCompatibility(Item item)
		{
			Item defaultItem = new();
			defaultItem.SetDefaults(item.netID);

			CompatibleType type = CompatibleType.None;

			// IsCandidateForReforge ?

			bool isBlacklisted = ((List<ItemDefinition>)GetConfigValue(nameof(WeaponFusionConfig.Blacklist))).Any(e => e.Type == item.netID);

            if (!isBlacklisted && item.stack == 1)
            {
                if (defaultItem.damage > 0 && defaultItem.IsCandidateForReforge)
				{
					type = CompatibleType.Damage;
				}
				if (defaultItem.defense > 0)
				{
					type = type == CompatibleType.Damage
						? CompatibleType.Both : CompatibleType.Defense;
				}
			}

			return type;
		}

		#endregion
	}

	public enum CompatibleType
	{
		None,
		Both,
		Damage,
		Defense
	}
}
