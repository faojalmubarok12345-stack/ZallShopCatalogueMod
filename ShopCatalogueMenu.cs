using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace ZallShopCatalogueMod;

internal sealed class ShopCatalogueMenu : IClickableMenu
{
    private readonly List<(string Id, string Name)> _shops;
    private readonly List<ClickableComponent> _buttons = new();
    private readonly ClickableTextureComponent _prev, _next, _close;
    private const int PerPage = 8;
    private int _page;

    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SeedShop"] = "Pierre's", ["Carpenter"] = "Robin's", ["Blacksmith"] = "Clint's",
        ["AnimalShop"] = "Marnie's", ["FishShop"] = "Willy's", ["Saloon"] = "Gus's Saloon",
        ["AdventureShop"] = "Guild", ["DesertTrade"] = "Desert Trader", ["Sandy"] = "Oasis",
        ["VolcanoShop"] = "Volcano Dwarf", ["Dwarf"] = "Mines Dwarf", ["Bookseller"] = "Bookseller",
        ["HatMouse"] = "Hat Mouse", ["Joja"] = "JojaMart", ["Sewer"] = "Krobus"
    };

    public ShopCatalogueMenu() : base(0, 0, Math.Min(Game1.uiViewport.Width - 64, 920), Math.Min(Game1.uiViewport.Height - 64, 600))
    {
        xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
        yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;

        var shopData = Game1.content.Load<Dictionary<string, ShopData>>("Data/Shops");
        _shops = shopData.Keys
            .OrderBy(k => Labels.ContainsKey(k) ? 0 : 1)
            .ThenBy(k => Labels.GetValueOrDefault(k, k))
            .Select(k => (k, Labels.GetValueOrDefault(k, k)))
            .ToList();

        _prev = new(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
        _next = new(new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
        _close = new(new Rectangle(xPositionOnScreen + width - 56, yPositionOnScreen - 16, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        BuildButtons();
    }

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_shops.Count / (double)PerPage));

    private void BuildButtons()
    {
        _buttons.Clear();
        var items = _shops.Skip(_page * PerPage).Take(PerPage).ToList();
        int padX = 32, gapX = 20, gapY = 14, btnW = (width - padX * 2 - gapX) / 2, btnH = 84;

        for (int i = 0; i < items.Count; i++)
        {
            int x = xPositionOnScreen + padX + (i % 2) * (btnW + gapX);
            int y = yPositionOnScreen + 88 + (i / 2) * (btnH + gapY);
            _buttons.Add(new ClickableComponent(new Rectangle(x, y, btnW, btnH), items[i].Id, items[i].Name));
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (_close.containsPoint(x, y) || !isWithinBounds(x, y)) { exitThisMenu(); return; }
        if (_page > 0 && _prev.containsPoint(x, y)) { _page--; BuildButtons(); return; }
        if (_page < TotalPages - 1 && _next.containsPoint(x, y)) { _page++; BuildButtons(); return; }

        foreach (var b in _buttons)
        {
            if (!b.containsPoint(x, y)) continue;
            exitThisMenu(false);
            if (!Utility.TryOpenShopMenu(b.name, Game1.currentLocation, true))
                Game1.addHUDMessage(new HUDMessage($"Gagal buka: {b.label}", HUDMessage.error_type));
            return;
        }
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

        string title = "Shop Catalogue";
        Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2(xPositionOnScreen + (width - Game1.dialogueFont.MeasureString(title).X) / 2f, yPositionOnScreen + 22), Game1.textColor);

        foreach (var btn in _buttons)
        {
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), btn.bounds.X, btn.bounds.Y, btn.bounds.Width, btn.bounds.Height, Color.White, drawShadow: false);
            Vector2 sz = Game1.smallFont.MeasureString(btn.label);
            Utility.drawTextWithShadow(b, btn.label, Game1.smallFont, new Vector2(btn.bounds.X + (btn.bounds.Width - sz.X) / 2f, btn.bounds.Y + (btn.bounds.Height - sz.Y) / 2f), Game1.textColor);
        }

        if (_page > 0) _prev.draw(b);
        if (_page < TotalPages - 1) _next.draw(b);
        string pg = $"{_page + 1}/{TotalPages}";
        Utility.drawTextWithShadow(b, pg, Game1.smallFont, new Vector2(xPositionOnScreen + (width - Game1.smallFont.MeasureString(pg).X) / 2f, yPositionOnScreen + height - 70), Game1.textColor);
        _close.draw(b);
        drawMouse(b);
    }
}
