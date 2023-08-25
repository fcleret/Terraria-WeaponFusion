using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using WeaponFusion.Items;

namespace WeaponFusion.Graphics
{
    public class ModUI : ModSystem
    {
        /*
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "Mod: WeaponFusionInventoryLevel",
                    delegate
                    {
                        DrawInventoryLevel(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private static void DrawInventoryLevel(SpriteBatch spriteBatch)
        {
            Player player = Main.CurrentPlayer;
            Item[] inventory = player.inventory;

            for (int i = 0; i < inventory.Length; i++)
            {
                Item item = inventory[i];

                if (item.IsAir)
                    continue;

                if (item.TryGetGlobalItem<ModGlobalItem>(out var globalItem)
                    && ModGlobalItem.CanHaveLevel(item))
                {
                    Rectangle slotTexture = TextureAssets.InventoryBack.Value.Bounds;
                    int level = globalItem.level;

                    InventorySlotLevel inventorySlotLevel = new(item, level);
                    inventorySlotLevel.Left.Pixels = 20 + slotTexture.Width * Main.inventoryScale * (i % 10);
                    inventorySlotLevel.Top.Pixels = 20 + slotTexture.Height * Main.inventoryScale * (float)Math.Ceiling(i / 10f);

                    inventorySlotLevel.Recalculate();
                    inventorySlotLevel.Draw(spriteBatch);
                }
            }
        }
        */
    }
}
