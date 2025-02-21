using Terraria.ModLoader;
using WeaponFusion.Utils;

namespace WeaponFusion
{
	public class WeaponFusion : Mod
	{
		public override void Load()
		{
			WeaponFusionConfig.ConfigurationChanged += ReloadItemsStats;
		}

		public override void Unload()
		{
			WeaponFusionConfig.ConfigurationChanged -= ReloadItemsStats;
		}

		private void ReloadItemsStats()
		{
			foreach (Player player in Main.player)
			{
				if (player?.inventory == null) 
					continue;

				foreach (Item item in player.inventory)
				{
					if (MergeHelper.IsCompatible(item))
					{
						MergeHelper.SetLevel(item, MergeHelper.GetLevel(item));
						item.RebuildTooltip();
					}
				}
			}
		}
	}
}