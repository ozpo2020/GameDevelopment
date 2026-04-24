using System.Collections.Generic;
using Godot;

public partial class Room : Node2D
{
    public string RoomId { get; private set; } = GameRoot.StartRoom;
    public Rect2 Bounds { get; private set; } = new(Vector2.Zero, new Vector2(640, 180));

    private readonly Dictionary<string, Vector2> _spawns = new();
    private GameRoot _root;
    private Color _accent = new(0.55f, 0.70f, 0.68f);

    public void Build(GameRoot root, string roomId)
    {
        _root = root;
        RoomId = roomId;
        Name = roomId;
        ZIndex = 0;
        _spawns.Clear();

        AddBaseSolids();

        switch (roomId)
        {
            case "Room_002_Combat":
                _accent = new Color(0.72f, 0.52f, 0.40f);
                BuildCombatRoom();
                break;
            case "Room_003_Flight":
                _accent = new Color(0.36f, 0.68f, 0.86f);
                BuildFlightRoom();
                break;
            case "Room_004_Boss":
                _accent = new Color(0.78f, 0.38f, 0.42f);
                BuildBossRoom();
                break;
            default:
                _accent = new Color(0.58f, 0.76f, 0.62f);
                BuildStartRoom();
                break;
        }

        QueueRedraw();
    }

    public Vector2 GetSpawn(string spawnId)
    {
        if (_spawns.TryGetValue(spawnId, out var spawn))
            return spawn;

        if (_spawns.TryGetValue("left", out spawn))
            return spawn;

        return _spawns.TryGetValue("checkpoint", out spawn) ? spawn : Vector2.Zero;
    }

    public void NotifyEnemyDefeated(string enemyId)
    {
        if (RoomId == "Room_002_Combat")
            QueueRedraw();
    }

    private void BuildStartRoom()
    {
        _spawns["checkpoint"] = new Vector2(56, 126);
        _spawns["left"] = new Vector2(56, 126);
        AddPlatform(new Rect2(122, 108, 72, 12), new Color(0.25f, 0.31f, 0.31f));
        AddCheckpoint(new Vector2(56, 126));
        AddTransition(new Vector2(628, 126), new Vector2(18, 48), "Room_002_Combat", "left");
    }

    private void BuildCombatRoom()
    {
        _spawns["left"] = new Vector2(34, 126);
        _spawns["right"] = new Vector2(596, 126);
        _spawns["checkpoint"] = new Vector2(34, 126);
        AddTransition(new Vector2(8, 126), new Vector2(18, 48), "Room_001_Start", "left");
        AddAbilityGate(new Rect2(610, 94, 20, 50), "dash", "Room_003_Flight", "left");
        AddPlatform(new Rect2(160, 112, 92, 12), new Color(0.28f, 0.35f, 0.36f));
        AddPlatform(new Rect2(360, 92, 86, 12), new Color(0.28f, 0.35f, 0.36f));
        AddGroundEnemy(new Vector2(260, 126), 210, 325);
        AddGroundEnemy(new Vector2(468, 126), 430, 560);
        AddRangedEnemy(new Vector2(404, 74));
    }

    private void BuildFlightRoom()
    {
        _spawns["left"] = new Vector2(34, 126);
        _spawns["right"] = new Vector2(596, 126);
        _spawns["checkpoint"] = new Vector2(34, 126);
        AddTransition(new Vector2(8, 126), new Vector2(18, 48), "Room_002_Combat", "right");
        AddTransition(new Vector2(628, 126), new Vector2(18, 48), "Room_004_Boss", "left");
        AddPlatform(new Rect2(120, 116, 74, 12), new Color(0.22f, 0.30f, 0.34f));
        AddPlatform(new Rect2(292, 96, 72, 12), new Color(0.22f, 0.30f, 0.34f));
        AddPlatform(new Rect2(464, 116, 74, 12), new Color(0.22f, 0.30f, 0.34f));
        AddFlyingEnemy(new Vector2(230, 72));
        AddFlyingEnemy(new Vector2(420, 62));
        AddRangedEnemy(new Vector2(506, 98));
    }

    private void BuildBossRoom()
    {
        _spawns["left"] = new Vector2(34, 126);
        _spawns["checkpoint"] = new Vector2(34, 126);
        AddTransition(new Vector2(8, 126), new Vector2(18, 48), "Room_003_Flight", "right");
        AddCheckpoint(new Vector2(70, 126));
        if (!_root.SaveManager.Current.BossDefeated)
            AddMiniBoss(new Vector2(420, 118));
    }

    private void AddBaseSolids()
    {
        AddPlatform(new Rect2(0, 144, 640, 36), new Color(0.18f, 0.22f, 0.24f));
        AddPlatform(new Rect2(-16, 0, 16, 180), new Color(0.12f, 0.15f, 0.16f));
        AddPlatform(new Rect2(640, 0, 16, 180), new Color(0.12f, 0.15f, 0.16f));
        AddPlatform(new Rect2(0, -12, 640, 12), new Color(0.12f, 0.15f, 0.16f));
    }

    private void AddPlatform(Rect2 rect, Color color)
    {
        var block = new SolidBlock();
        AddChild(block);
        block.Configure(rect, color);
    }

    private void AddTransition(Vector2 position, Vector2 size, string targetRoom, string targetSpawn)
    {
        var transition = new RoomTransition();
        AddChild(transition);
        transition.Configure(_root, position, size, targetRoom, targetSpawn);
    }

    private void AddCheckpoint(Vector2 position)
    {
        var checkpoint = new Checkpoint();
        AddChild(checkpoint);
        checkpoint.Configure(_root, position);
    }

    private void AddAbilityGate(Rect2 rect, string abilityId, string targetRoom, string targetSpawn)
    {
        var gate = new AbilityGate();
        AddChild(gate);
        gate.Configure(_root, rect, abilityId, targetRoom, targetSpawn);
    }

    private void AddGroundEnemy(Vector2 position, float patrolLeft, float patrolRight)
    {
        var enemy = new GroundPatrolEnemy { GlobalPosition = position };
        AddChild(enemy);
        enemy.SetPatrol(patrolLeft, patrolRight);
        _root.RegisterEnemy(enemy);
    }

    private void AddFlyingEnemy(Vector2 position)
    {
        var enemy = new FlyingChaserEnemy { GlobalPosition = position };
        AddChild(enemy);
        _root.RegisterEnemy(enemy);
    }

    private void AddRangedEnemy(Vector2 position)
    {
        var enemy = new RangedSentinelEnemy { GlobalPosition = position };
        AddChild(enemy);
        _root.RegisterEnemy(enemy);
    }

    private void AddMiniBoss(Vector2 position)
    {
        var enemy = new MiniBoss01 { GlobalPosition = position };
        AddChild(enemy);
        _root.RegisterEnemy(enemy);
    }

    public override void _Draw()
    {
        var baseColor = new Color(0.045f, 0.054f, 0.064f);
        var upperColor = baseColor.Lerp(_accent, 0.08f);
        DrawRect(Bounds, baseColor);
        DrawRect(new Rect2(0, 0, 640, 82), upperColor);

        for (var y = 0; y < 144; y += 18)
        {
            var shade = new Color(0.018f, 0.022f, 0.028f, 0.18f + y / 900.0f);
            DrawRect(new Rect2(0, y, 640, 1), shade);
        }

        for (var x = 0; x < 640; x += 32)
        {
            var pillarColor = x % 64 == 0 ? new Color(0.070f, 0.084f, 0.090f) : new Color(0.058f, 0.070f, 0.078f);
            DrawRect(new Rect2(x + 8, 16, 12, 128), pillarColor.Lerp(_accent, 0.06f));
            DrawRect(new Rect2(x + 6, 18, 2, 124), new Color(0.02f, 0.025f, 0.03f, 0.38f));
        }

        for (var x = 26; x < 640; x += 96)
        {
            DrawCircle(new Vector2(x, 42), 6.0f, new Color(_accent.R, _accent.G, _accent.B, 0.10f));
            DrawRect(new Rect2(x - 2, 39, 4, 8), _accent.Lerp(Colors.White, 0.18f));
        }

        if (RoomId == "Room_001_Start")
        {
            DrawRect(new Rect2(34, 118, 42, 4), new Color(0.30f, 0.44f, 0.34f));
            DrawRect(new Rect2(42, 110, 3, 8), new Color(0.40f, 0.58f, 0.38f));
            DrawRect(new Rect2(58, 104, 3, 14), new Color(0.40f, 0.58f, 0.38f));
        }
        else if (RoomId == "Room_004_Boss")
        {
            DrawRect(new Rect2(374, 66, 92, 78), new Color(0.13f, 0.07f, 0.08f, 0.55f));
            DrawRect(new Rect2(384, 76, 72, 68), new Color(0.035f, 0.02f, 0.025f, 0.75f));
            DrawLine(new Vector2(384, 76), new Vector2(456, 144), _accent.Darkened(0.2f), 1.0f);
            DrawLine(new Vector2(456, 76), new Vector2(384, 144), _accent.Darkened(0.2f), 1.0f);
        }

        DrawLine(new Vector2(0, 143), new Vector2(640, 143), new Color(_accent.R, _accent.G, _accent.B, 0.40f), 1.0f);
    }
}
