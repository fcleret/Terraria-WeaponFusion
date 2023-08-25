using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace WeaponFusion.Graphics
{
    public class InventorySlotLevel : UIElement
    {
        private Item _item;
        private int _level;

        public InventorySlotLevel(Item item, int level)
        {
            _item = item;
            _level = level;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            DynamicSpriteFont mouseText = FontAssets.MouseText.Value;
            CalculatedStyle dimensions = GetInnerDimensions();
            Vector2 pos = new(dimensions.X + dimensions.Width + 4f, dimensions.Y + dimensions.Height - mouseText.MeasureString(_level.ToString()).Y - 4f);

            spriteBatch.DrawString(mouseText, _level.ToString(), pos, Color.White);
        }
    }
}
