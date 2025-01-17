using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace LeFauxMods.Sandwich.Utilities;

internal static class ModExtensions
{
    public static void ConsumeSandwich(this Farmer farmer, Item? sandwich)
    {
        if (sandwich is not SObject { QualifiedItemId: Constants.SandwichQualifiedId, heldObject.Value: Chest chest })
        {
            return;
        }

        foreach (var filling in chest.Items)
        {
            // Mostly copied from vanilla
            if (filling.HasContextTag("ginger_item"))
            {
                farmer.buffs.Remove("25");
            }

            foreach (var buff in filling.GetFoodOrDrinkBuffs())
            {
                farmer.applyBuff(buff);
            }

            switch (filling.QualifiedItemId)
            {
                case "(O)773":
                    farmer.health = farmer.maxHealth;
                    break;
                case "(O)351":
                    farmer.exhausted.Value = false;
                    break;
                case "(O)349":
                    farmer.Stamina = farmer.MaxStamina;
                    break;
            }

            farmer.ConsumeSandwich(filling);
        }
    }

    public static bool DrawSandwich(this SObject obj, SpriteBatch spriteBatch, int x, int y)
    {
        var layerDepth = 1f;
        return obj.DrawSandwich(spriteBatch, ref x, ref y, ref layerDepth, 0);
    }

    private static bool DrawSandwich(this SObject obj, SpriteBatch spriteBatch, ref int x, ref int y,
        ref float layerDepth, int level)
    {
        var chest = obj switch
        {
            { QualifiedItemId: Constants.SandwichQualifiedId, heldObject.Value: Chest sandwichChest } => sandwichChest,
            Chest tableChest => tableChest,
            _ => null
        };

        if (chest is null)
        {
            return false;
        }

        var bread = ItemRegistry.GetDataOrErrorItem(Constants.SandwichId);
        spriteBatch.Draw(
            bread.GetTexture(),
            new Rectangle(x, y, Game1.tileSize, 32),
            bread.GetSourceRect(),
            Color.White,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            layerDepth);

        chest.DrawFillings(spriteBatch, ref x, ref y, ref layerDepth, level);

        if (obj is Chest)
        {
            return true;
        }

        y -= chest.Items[^1] is { QualifiedItemId: Constants.SandwichQualifiedId } ? 6 : 8;
        spriteBatch.Draw(
            bread.GetTexture(),
            new Rectangle(x, y, Game1.tileSize, 32),
            bread.GetSourceRect(),
            Color.White,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            layerDepth + (float)Math.Pow(10, -level));

        return true;
    }

    private static void DrawFillings(this Chest chest, SpriteBatch spriteBatch, ref int x, ref int y,
        ref float layerDepth, int level)
    {
        foreach (var filling in chest.Items.OfType<SObject>())
        {
            y -= 6;
            layerDepth += 1f / chest.Items.Count * (float)Math.Pow(10, -level);
            if (filling.DrawSandwich(spriteBatch, ref x, ref y, ref layerDepth, level + 1))
            {
                y -= 2;
                continue;
            }

            var data = ItemRegistry.GetDataOrErrorItem(filling.ItemId);
            var rect = data.GetSourceRect();
            spriteBatch.Draw(
                data.GetTexture(),
                new Rectangle(x, y, Game1.tileSize, 32),
                rect,
                Color.White,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }
    }
}