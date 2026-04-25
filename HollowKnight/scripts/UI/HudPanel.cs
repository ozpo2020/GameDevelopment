using Godot;

public partial class HudPanel : Control
{
    private int _health;
    private int _maxHealth = 1;
    private int _focus;
    private int _maxFocus = 1;
    private string _weaponName = "SWORD";
    private bool _hasDash;
    private int _enemiesAlive;
    private bool _bossDefeated;

    public void SetStatus(int health, int maxHealth, int focus, int maxFocus, string weaponName, bool hasDash, int enemiesAlive, bool bossDefeated)
    {
        _health = health;
        _maxHealth = Mathf.Max(1, maxHealth);
        _focus = focus;
        _maxFocus = Mathf.Max(1, maxFocus);
        _weaponName = weaponName;
        _hasDash = hasDash;
        _enemiesAlive = enemiesAlive;
        _bossDefeated = bossDefeated;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, 320, 31), new Color(0.025f, 0.032f, 0.042f, 0.86f));
        DrawRect(new Rect2(0, 30, 320, 1), new Color(0.34f, 0.47f, 0.50f, 0.6f));

        for (var i = 0; i < _maxHealth; i++)
        {
            var x = 8 + i * 12;
            var filled = i < _health;
            var color = filled ? new Color(0.92f, 0.84f, 0.65f) : new Color(0.18f, 0.22f, 0.26f);
            DrawRect(new Rect2(x, 7, 9, 9), color);
            DrawRect(new Rect2(x + 2, 5, 5, 2), color.Lightened(0.16f));
            DrawRect(new Rect2(x, 16, 9, 1), new Color(0.02f, 0.02f, 0.025f, 0.9f));
        }

        var dashColor = _hasDash ? new Color(0.30f, 0.82f, 1.0f) : new Color(0.18f, 0.24f, 0.28f);
        DrawRect(new Rect2(70, 7, 9, 9), dashColor);
        DrawRect(new Rect2(73, 5, 3, 13), dashColor.Lightened(0.18f));

        DrawRect(new Rect2(88, 20, 72, 4), new Color(0.10f, 0.13f, 0.17f));
        var focusWidth = 72.0f * Mathf.Clamp(_focus / (float)_maxFocus, 0.0f, 1.0f);
        DrawRect(new Rect2(88, 20, focusWidth, 4), new Color(0.34f, 0.86f, 1.0f));
        for (var i = 0; i <= _maxFocus; i += 3)
        {
            var x = 88 + i * 72.0f / _maxFocus;
            DrawRect(new Rect2(x, 19, 1, 6), new Color(0.02f, 0.025f, 0.03f, 0.82f));
        }

        var markerColor = _bossDefeated
            ? new Color(0.55f, 0.95f, 0.68f)
            : _enemiesAlive > 0
                ? new Color(0.94f, 0.42f, 0.38f)
                : new Color(0.50f, 0.62f, 0.66f);
        DrawRect(new Rect2(296, 7, 10, 10), markerColor);
        DrawRect(new Rect2(298, 4, 6, 3), markerColor.Lightened(0.2f));

        DrawWeaponIcon(new Vector2(170, 7));
    }

    private void DrawWeaponIcon(Vector2 origin)
    {
        var active = _weaponName;
        var sword = active == "SWORD" ? new Color(0.86f, 0.92f, 0.88f) : new Color(0.22f, 0.27f, 0.30f);
        var axe = active == "AXE" ? new Color(0.58f, 0.92f, 0.78f) : new Color(0.22f, 0.27f, 0.30f);
        var hammer = active == "HAMMER" ? new Color(0.94f, 0.78f, 0.55f) : new Color(0.22f, 0.27f, 0.30f);

        DrawRect(new Rect2(origin, new Vector2(18, 2)), sword);
        DrawRect(new Rect2(origin + new Vector2(23, -2), new Vector2(12, 3)), axe);
        DrawRect(new Rect2(origin + new Vector2(31, -5), new Vector2(5, 9)), axe);
        DrawRect(new Rect2(origin + new Vector2(46, 1), new Vector2(16, 3)), hammer);
        DrawRect(new Rect2(origin + new Vector2(58, -4), new Vector2(8, 10)), hammer);
    }
}
