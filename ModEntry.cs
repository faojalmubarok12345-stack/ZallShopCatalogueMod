using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace ZallShopCatalogueMod;

public class ModEntry : Mod
{
    private const string ItemId = "zall.shopcatalogue_Catalog";
    private Texture2D? _icon;

    public override void Entry(IModHelper helper)
    {
        _icon = helper.ModContent.Load<Texture2D>("assets/item.png");
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Mods/zall.shopcatalogue/item"))
            e.LoadFrom(() => _icon!, AssetLoadPriority.Medium);

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ObjectData>().Data;
                if (!data.ContainsKey(ItemId))
                {
                    data[ItemId] = new ObjectData
                    {
                        Name = "ShopCatalogue",
                        DisplayName = "Universal Shop Catalogue",
                        Description = "Buka berbagai toko kapan saja.",
                        Type = "Basic",
                        Category = 0,
                        Price = 2500,
                        Texture = "Mods/zall.shopcatalogue/item",
                        SpriteIndex = 0
                    };
                }
            });
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.activeClickableMenu != null) return;
        if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()) return;
        if (Game1.player.ActiveObject?.QualifiedItemId != $"(O){ItemId}") return;

        if (e.Button.IsUseToolButton())
            Helper.Input.Suppress(e.Button);

        Game1.activeClickableMenu = new ShopCatalogueMenu();
    }
}
