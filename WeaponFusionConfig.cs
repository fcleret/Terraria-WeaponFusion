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
        public override ConfigScope Mode => ConfigScope.ServerSide;

        public static WeaponFusionConfig Current => ModContent.GetInstance<WeaponFusionConfig>();

        [Header("Weapon Fusion")]

        [Label("Damage multiplier")]
        [DefaultValue(0.5f)]
        [Range(0.0f, 1.0f)]
        [ReloadRequired]
        public float MultDamage;

        [Label("Defense multiplier")]
        [DefaultValue(0.5f)]
        [Range(0.0f, 1.0f)]
        [ReloadRequired]
        public float MultDefense;

        [Label("Override prefix")]
        [Tooltip("Damage and defense bonus from prefix are overrides. Not configurable for now. [WIP]")]
        [JsonProperty]
        public bool OverridePrefix { get; } = true;

        [Label("Blacklist")]
        public List<ItemDefinition> Blacklist = new()
        {
            { new ItemDefinition(24) }, // Wooden Sword
            { new ItemDefinition(284) }, // Wooden Boomerang
            { new ItemDefinition(55) } // Enchanted Boomerang
        };
    }
}
