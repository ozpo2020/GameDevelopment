using Godot;

public partial class HudController : CanvasLayer
{
    private HudPanel _panel;
    private Label _status;
    private Label _banner;
    private Label _roomTitle;
    private Label _hint;
    private Label _pause;
    private ColorRect _pauseShade;
    private ColorRect _majorShade;
    private Label _majorTitle;
    private Label _majorPrompt;
    private double _bannerTimer;
    private double _roomTitleTimer;

    public override void _Ready()
    {
        Layer = 15;

        _panel = new HudPanel
        {
            Position = Vector2.Zero,
            Size = new Vector2(320, 180),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        AddChild(_panel);

        _status = new Label
        {
            Position = new Vector2(86, 4),
            Size = new Vector2(226, 18)
        };
        _status.AddThemeFontSizeOverride("font_size", 9);
        _status.AddThemeColorOverride("font_color", new Color(0.90f, 0.94f, 0.98f));
        AddChild(_status);

        _roomTitle = new Label
        {
            Position = new Vector2(68, 38),
            Size = new Vector2(184, 24),
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false
        };
        _roomTitle.AddThemeFontSizeOverride("font_size", 15);
        _roomTitle.AddThemeColorOverride("font_color", new Color(0.92f, 0.86f, 0.66f));
        _roomTitle.AddThemeConstantOverride("outline_size", 4);
        _roomTitle.AddThemeColorOverride("font_outline_color", new Color(0.015f, 0.018f, 0.024f));
        AddChild(_roomTitle);

        _banner = new Label
        {
            Position = new Vector2(34, 62),
            Size = new Vector2(252, 32),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false
        };
        _banner.AddThemeFontSizeOverride("font_size", 13);
        _banner.AddThemeColorOverride("font_color", new Color(0.94f, 0.88f, 0.58f));
        _banner.AddThemeConstantOverride("outline_size", 4);
        _banner.AddThemeColorOverride("font_outline_color", new Color(0.02f, 0.025f, 0.03f));
        AddChild(_banner);

        _hint = new Label
        {
            Position = new Vector2(8, 164),
            Size = new Vector2(304, 13),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _hint.AddThemeFontSizeOverride("font_size", 8);
        _hint.AddThemeColorOverride("font_color", new Color(0.68f, 0.76f, 0.78f, 0.86f));
        AddChild(_hint);

        _pauseShade = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(320, 180),
            Color = new Color(0.02f, 0.025f, 0.035f, 0.68f),
            Visible = false
        };
        AddChild(_pauseShade);

        _majorShade = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(320, 180),
            Color = new Color(0.015f, 0.018f, 0.024f, 0.88f),
            Visible = false
        };
        AddChild(_majorShade);

        _majorTitle = new Label
        {
            Position = new Vector2(20, 48),
            Size = new Vector2(280, 36),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false
        };
        _majorTitle.AddThemeFontSizeOverride("font_size", 22);
        _majorTitle.AddThemeColorOverride("font_color", new Color(0.88f, 0.84f, 0.66f));
        _majorTitle.AddThemeConstantOverride("outline_size", 5);
        _majorTitle.AddThemeColorOverride("font_outline_color", new Color(0.0f, 0.0f, 0.0f));
        AddChild(_majorTitle);

        _majorPrompt = new Label
        {
            Position = new Vector2(28, 91),
            Size = new Vector2(264, 20),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false
        };
        _majorPrompt.AddThemeFontSizeOverride("font_size", 10);
        _majorPrompt.AddThemeColorOverride("font_color", new Color(0.72f, 0.82f, 0.84f));
        AddChild(_majorPrompt);

        _pause = new Label
        {
            Position = new Vector2(42, 58),
            Size = new Vector2(236, 64),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "PAUSED\nESC / START TO RESUME",
            Visible = false
        };
        _pause.AddThemeFontSizeOverride("font_size", 15);
        _pause.AddThemeColorOverride("font_color", new Color(0.90f, 0.96f, 1.0f));
        _pause.AddThemeConstantOverride("outline_size", 5);
        _pause.AddThemeColorOverride("font_outline_color", new Color(0.01f, 0.015f, 0.02f));
        AddChild(_pause);
    }

    public override void _Process(double delta)
    {
        if (_bannerTimer <= 0.0)
        {
            if (_roomTitleTimer > 0.0)
            {
                _roomTitleTimer -= delta;
                if (_roomTitleTimer <= 0.0)
                    _roomTitle.Visible = false;
            }
            return;
        }

        _bannerTimer -= delta;
        if (_bannerTimer <= 0.0)
            _banner.Visible = false;

        if (_roomTitleTimer > 0.0)
        {
            _roomTitleTimer -= delta;
            if (_roomTitleTimer <= 0.0)
                _roomTitle.Visible = false;
        }
    }

    public void SetStatus(string roomId, int health, int maxHealth, int focus, int maxFocus, string weaponName, bool hasDash, int enemiesAlive, bool bossDefeated)
    {
        var dash = hasDash ? "DASH" : "NO DASH";
        var objective = bossDefeated ? "CLEAR" : enemiesAlive > 0 ? $"FOES {enemiesAlive}" : "EXPLORE";
        _status.Text = $"{weaponName} | {dash} | SOUL {focus}/{maxFocus} | {objective}";
        _hint.Text = hasDash
            ? "1 SWORD  2 AXE  3 HAMMER/Q CYCLE   J ATTACK   E SOUL ART   SHIFT DASH"
            : "1 SWORD  2 AXE  3 HAMMER/Q CYCLE   J ATTACK   E SOUL ART";
        _panel.SetStatus(health, maxHealth, focus, maxFocus, weaponName, hasDash, enemiesAlive, bossDefeated);
    }

    public void ShowBanner(string text, float seconds = 2.0f)
    {
        _banner.Text = text;
        _banner.Visible = true;
        _bannerTimer = seconds;
    }

    public void SetPaused(bool paused)
    {
        _pauseShade.Visible = paused;
        _pause.Visible = paused;
    }

    public void ShowRoomTitle(string roomName)
    {
        _roomTitle.Text = roomName;
        _roomTitle.Visible = true;
        _roomTitleTimer = 1.45;
    }

    public void ShowTitle(string title, string prompt)
    {
        _majorTitle.Text = title;
        _majorPrompt.Text = prompt;
        _majorShade.Visible = true;
        _majorTitle.Visible = true;
        _majorPrompt.Visible = true;
    }

    public void HideMajorOverlay()
    {
        _majorShade.Visible = false;
        _majorTitle.Visible = false;
        _majorPrompt.Visible = false;
    }
}
