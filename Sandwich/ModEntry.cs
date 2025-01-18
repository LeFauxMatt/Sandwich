using LeFauxMods.Common.Utilities;
using LeFauxMods.Sandwich.Services;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Objects;

namespace LeFauxMods.Sandwich;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(helper.Translation);
        Log.Init(this.Monitor);
        ModPatches.Apply();

        // Events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree ||
            !e.Button.IsUseToolButton() ||
            !Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out var target) ||
            target is not { QualifiedItemId: Constants.TableQualifiedId })
        {
            return;
        }

        // Make sandwich
        if (target.heldObject.Value is null)
        {
            if (Game1.player.ActiveObject is not { QualifiedItemId: "(O)216" })
            {
                return;
            }

            this.Helper.Input.Suppress(e.Button);
            target.heldObject.Value = new Chest(true, Constants.SandwichId)
            {
                displayNameFormat = I18n.Sandwich_Name()
            };

            target.MinutesUntilReady = int.MaxValue;
            Game1.player.reduceActiveItemByOne();
            return;
        }

        // Collect sandwich
        if (target.heldObject.Value is { QualifiedItemId: Constants.SandwichQualifiedId })
        {
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                if (Game1.player.Items[i] is not null)
                {
                    continue;
                }

                this.Helper.Input.Suppress(e.Button);
                Game1.player.Items[i] = target.heldObject.Value;
                target.heldObject.Value = null;
                return;
            }

            return;
        }

        if (target.heldObject.Value is not Chest chest || Game1.player.ActiveObject?.Edibility is null or -300)
        {
            return;
        }

        this.Helper.Input.Suppress(e.Button);

        // End sandwich
        if (Game1.player.ActiveObject.QualifiedItemId == "(O)216")
        {
            var sandwich = ItemRegistry.Create<SObject>(Constants.SandwichId);
            sandwich.displayNameFormat =
                string.Join(' ', chest.Items.Select(static item => item.DisplayName)) + " Sandwich";
            sandwich.heldObject.Value = chest;
            sandwich.Edibility = chest.Items.OfType<SObject>().Sum(static item => item.Edibility);
            target.heldObject.Value = sandwich;
            Game1.player.reduceActiveItemByOne();
            return;
        }

        // Add toppings
        var topping = (SObject)Game1.player.ActiveObject.getOne();
        topping.heldObject.Value = Game1.player.ActiveObject.heldObject.Value;
        chest.Items.Add(topping);
        Game1.player.reduceActiveItemByOne();
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                asset.AsDictionary<string, BigCraftableData>().Data.Add(
                    Constants.TableId,
                    new BigCraftableData
                    {
                        Name = "Sandwich Prep Table",
                        DisplayName = I18n.SandwichPrepTable_Name(),
                        Description = I18n.SandwichPrepTable_Description(),
                        CanBePlacedOutdoors = true,
                        CanBePlacedIndoors = true,
                        Texture = this.Helper.ModContent.GetInternalAssetName("assets/table.png").BaseName,
                        Price = 500
                    });
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
        {
            e.Edit(static asset =>
            {
                asset.AsDictionary<string, MachineData>().Data.Add(
                    $"(BC){Constants.TableId}",
                    new MachineData
                    {
                        HasInput = true,
                        HasOutput = true,
                        AllowLoadWhenFull = true,
                        PreventTimePass = [MachineTimeBlockers.Always],
                        WobbleWhileWorking = false
                    });
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(static asset =>
            {
                asset.AsDictionary<string, ObjectData>().Data.Add(
                    Constants.SandwichId,
                    new ObjectData
                    {
                        Name = "Sandwich",
                        DisplayName = I18n.Sandwich_Name(),
                        Description = I18n.Sandwich_Description(),
                        Type = "Cooking",
                        Category = -7,
                        Texture = "Maps/springobjects",
                        Edibility = -299,
                        SpriteIndex = 217
                    });
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(static asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;
                if (!data.TryGetValue("Saloon", out var shopData))
                {
                    return;
                }

                shopData.Items.Add(new ShopItemData
                {
                    Id = Constants.TableQualifiedId, ItemId = Constants.TableQualifiedId
                });
            });
        }
    }
}