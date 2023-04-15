using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WeaponFusion
{
	public class WeaponFusionConfig : ModConfig
	{
		private const string nsLocalization = "Mods.WeaponFusion.Config";

		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static WeaponFusionConfig Current => ModContent.GetInstance<WeaponFusionConfig>();

		#region General

		[Header($"${nsLocalization}.GeneralHeader")]

		[Label($"${nsLocalization}.LevelCap")]
		[DefaultValue(999)]
		[Range(0, 999)]
		public int MaxLevel;

		[Label($"${nsLocalization}.Blacklist")]
		public List<ItemDefinition> Blacklist = new()
		{
			{ new ItemDefinition(24) },		// Wooden sword
			{ new ItemDefinition(284) },	// Wooden boomerang
			{ new ItemDefinition(55) }		// Enchanted boomerang
		};

		#endregion

		#region Multipliers

		[Header($"${nsLocalization}.MultipliersHeader")]

        [Label($"${nsLocalization}.MultiplierDefense")]
        [DefaultValue(0.5f)]
        [Range(0.0f, 1.0f)]
        public float MultDefense;

        [Label($"${nsLocalization}.MultiplierDamage")]
		[DefaultValue(0.5f)]
		[Range(0.0f, 1.0f)]
		public float MultDamage;

        [Label($"${nsLocalization}.MultiplierCrit")]
        [DefaultValue(0.1f)]
        [Range(0.0f, 1.0f)]
        public float MultCrit;

        [Label($"${nsLocalization}.MultiplierKnockback")]
        [DefaultValue(0.01f)]
        [Range(0.0f, 1.0f)]
        public float MultKnockback;

		#endregion

		#region WIP

		[Header($"$Mods.WeaponFusion.Global.WorkInProgress")]

		[Label($"${nsLocalization}.MultiplayerCommands")]
		[DefaultValue(false)]
		public bool EnabledCommands;

		#endregion

		public static event ConfigurationChangedEventHandler ConfigurationChanged;
		public delegate void ConfigurationChangedEventHandler();

		public override void OnChanged()
		{
			ConfigurationChanged?.Invoke();
		}
	}
}
