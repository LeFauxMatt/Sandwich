using HarmonyLib;
using LeFauxMods.Common.Utilities;
using LeFauxMods.Sandwich.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace LeFauxMods.Sandwich.Services;

/// <summary>Encapsulates mod patches.</summary>
internal static class ModPatches
{
    private static readonly Harmony Harmony = new(Constants.ModId);

    public static void Apply()
    {
        try
        {
            Log.Info("Applying patches");

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Farmer), nameof(Farmer.doneEating)),
                postfix: new HarmonyMethod(typeof(ModPatches), nameof(Farmer_doneEating_postfix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canStackWith)),
                postfix: new HarmonyMethod(typeof(ModPatches), nameof(Item_canStackWith_postfix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                postfix: new HarmonyMethod(typeof(ModPatches), nameof(Object_draw_postfix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float)]),
                new HarmonyMethod(typeof(ModPatches), nameof(Object_draw_prefix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawWhenHeld)),
                new HarmonyMethod(typeof(ModPatches), nameof(Object_drawWhenHeld_prefix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)),
                new HarmonyMethod(typeof(ModPatches), nameof(Object_drawInMenu_prefix)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.maximumStackSize)),
                postfix: new HarmonyMethod(typeof(ModPatches), nameof(Object_maximumStackSize_postfix)));
        }
        catch (Exception)
        {
            Log.WarnOnce("Failed to apply patches");
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static void Farmer_doneEating_postfix(Farmer __instance) =>
        __instance.ConsumeSandwich(__instance.itemToEat);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result)
    {
        if (__instance is SObject { QualifiedItemId: Constants.SandwichQualifiedId, heldObject.Value: Chest })
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static void Object_draw_postfix(SObject __instance, SpriteBatch spriteBatch, int x, int y)
    {
        var position = Game1.GlobalToLocal(Game1.viewport, Game1.tileSize * new Vector2(x, y - 0.25f));

        switch (__instance)
        {
            case { QualifiedItemId: Constants.TableQualifiedId, heldObject.Value: Chest chest }:
                chest.DrawSandwich(spriteBatch, (int)position.X, (int)position.Y);
                return;

            case
            {
                QualifiedItemId: Constants.TableQualifiedId,
                heldObject.Value:
                { QualifiedItemId: Constants.SandwichQualifiedId, heldObject.Value: Chest } sandwich
            }:
                sandwich.DrawSandwich(spriteBatch, (int)position.X, (int)position.Y);
                return;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile) =>
        !__instance.DrawSandwich(spriteBatch, xNonTile, yNonTile);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static bool Object_drawInMenu_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 location) =>
        !__instance.DrawSandwich(spriteBatch, (int)location.X, (int)location.Y + 32);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static bool
        Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition) =>
        !__instance.DrawSandwich(spriteBatch, (int)objectPosition.X, (int)objectPosition.Y + 32);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    private static void Object_maximumStackSize_postfix(SObject __instance, ref int __result) =>
        __result = __result > 1 && __instance is
            { QualifiedItemId: Constants.SandwichQualifiedId, heldObject.Value: Chest }
            ? 1
            : __result;
}