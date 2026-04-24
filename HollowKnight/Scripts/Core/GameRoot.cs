using Godot;

public partial class GameRoot : Node2D
{
    public const string StartRoom = "Room_001_Start";

    public PlayerController Player { get; private set; }
    public HudController Hud { get; private set; }
    public SaveManager SaveManager { get; private set; }

    private Camera2D _camera;
    private Room _currentRoom;
    private DebugOverlay _debugOverlay;
    private int _enemiesAlive;
    private string _currentRoomId = StartRoom;
    private bool _paused;
    private bool _titleActive;
    private bool _clearActive;
    private double _shakeTimer;
    private float _shakeStrength;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _rng.Randomize();
        InputBindings.EnsureDefaults();
        AddChild(new SignalBus());
        AddChild(new SceneLoader());

        SaveManager = new SaveManager();
        AddChild(SaveManager);
        SaveManager.Load();

        Player = new PlayerController();
        AddChild(Player);
        Player.PlayerDied += OnPlayerDied;
        Player.HealthChanged += OnPlayerHealthChanged;
        Player.AttackLanded += OnPlayerAttackLanded;
        Player.PlayerHurt += OnPlayerHurt;
        Player.Abilities.HasDash = SaveManager.Current.HasDash;

        _camera = new Camera2D { Enabled = true, PositionSmoothingEnabled = true, PositionSmoothingSpeed = 8.0f };
        AddChild(_camera);
        _camera.MakeCurrent();

        Hud = new HudController();
        Hud.ProcessMode = ProcessModeEnum.Always;
        AddChild(Hud);

        _debugOverlay = new DebugOverlay();
        AddChild(_debugOverlay);
        _debugOverlay.Watch(Player);

        LoadRoom(SaveManager.Current.CheckpointRoom, "checkpoint");
        Player.RespawnAt(SaveManager.Current.CheckpointPosition);
        ShowTitle();
    }

    public override void _Process(double delta)
    {
        if (_titleActive)
        {
            if (WantsConfirm())
                BeginFromTitle();
            return;
        }

        if (_clearActive)
        {
            if (WantsConfirm())
                StartFreshRun();
            return;
        }

        if (Input.IsActionJustPressed(InputBindings.Pause))
            SetPaused(!_paused);

        if (Player != null)
            UpdateCamera(delta);

        Hud.SetStatus(
            _currentRoomId,
            Player.Health.CurrentHealth,
            Player.Health.MaxHealth,
            Player.Focus,
            Player.MaxFocus,
            Player.WeaponName,
            Player.Abilities.HasDash,
            _enemiesAlive,
            SaveManager.Current.BossDefeated
        );
    }

    public void LoadRoom(string roomId, string spawnId)
    {
        _currentRoom?.QueueFree();
        _enemiesAlive = 0;
        _currentRoomId = roomId;

        _currentRoom = new Room();
        AddChild(_currentRoom);
        MoveChild(_currentRoom, 0);
        _currentRoom.Build(this, roomId);

        var spawn = _currentRoom.GetSpawn(spawnId);
        if (spawn != Vector2.Zero)
            Player.GlobalPosition = spawn;

        CameraBounds.ApplyTo(_camera, _currentRoom.Bounds);
        _debugOverlay?.SetRoom(roomId);
        Hud.ShowRoomTitle(FormatRoomName(roomId));
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.RoomChanged, roomId);
    }

    public void TransitionTo(string roomId, string spawnId)
    {
        LoadRoom(roomId, spawnId);
    }

    public void ActivateCheckpoint(Vector2 position)
    {
        SaveManager.SetCheckpoint(_currentRoomId, position);
        Hud.ShowBanner("CHECKPOINT LIT");
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.CheckpointActivated, _currentRoomId, position);
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        _enemiesAlive++;
        enemy.Player = Player;
        enemy.Defeated += OnEnemyDefeated;
    }

    public void GrantAbility(string abilityId)
    {
        if (abilityId != "dash" || Player.Abilities.HasDash)
            return;

        Player.Abilities.HasDash = true;
        SaveManager.SetDashUnlocked(true);
        Hud.ShowBanner("ABILITY UNLOCKED: DASH");
        ShakeCamera(3.0f, 0.18f);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.AbilityUnlocked, abilityId);
    }

    private void OnEnemyDefeated(string enemyId)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        _currentRoom?.NotifyEnemyDefeated(enemyId);

        if (_currentRoomId == "Room_002_Combat" && _enemiesAlive == 0)
            GrantAbility("dash");

        if (enemyId == "MiniBoss_01")
        {
            SaveManager.SetBossDefeated(true);
            ShakeCamera(5.0f, 0.35f);
            CompleteGame();
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.BossDefeated);
        }
    }

    private void OnPlayerHealthChanged(int current, int maximum)
    {
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.PlayerHealthChanged, current, maximum);
    }

    private void OnPlayerAttackLanded(Vector2 position)
    {
        ShakeCamera(2.0f, 0.08f);
    }

    private void OnPlayerHurt(Vector2 position)
    {
        ShakeCamera(4.0f, 0.16f);
    }

    private void OnPlayerDied()
    {
        Hud.ShowBanner("YOU FELL - RETURNING TO CHECKPOINT");
        LoadRoom(SaveManager.Current.CheckpointRoom, "checkpoint");
        Player.RespawnAt(SaveManager.Current.CheckpointPosition);
    }

    private void SetPaused(bool paused)
    {
        _paused = paused;
        GetTree().Paused = paused;
        Hud.SetPaused(paused);
    }

    private void ShowTitle()
    {
        _titleActive = true;
        _clearActive = false;
        _paused = false;
        GetTree().Paused = true;
        Hud.SetPaused(false);
        Hud.ShowTitle("MOSSBOUND", "PRESS ATTACK / JUMP TO START");
    }

    private void BeginFromTitle()
    {
        _titleActive = false;
        GetTree().Paused = false;
        Hud.HideMajorOverlay();
        Hud.ShowBanner("WAKE, LITTLE KNIGHT");
    }

    private void CompleteGame()
    {
        _clearActive = true;
        _paused = false;
        GetTree().Paused = true;
        Hud.SetPaused(false);
        Hud.ShowTitle("GAME CLEAR", "PRESS ATTACK / JUMP TO START AGAIN");
    }

    private void StartFreshRun()
    {
        _clearActive = false;
        SaveManager.ResetRun();
        Player.Abilities.HasDash = false;
        LoadRoom(StartRoom, "checkpoint");
        Player.RespawnAt(SaveManager.Current.CheckpointPosition);
        Hud.HideMajorOverlay();
        GetTree().Paused = false;
        Hud.ShowBanner("NEW RUN");
    }

    private static bool WantsConfirm()
    {
        return Input.IsActionJustPressed(InputBindings.Attack)
            || Input.IsActionJustPressed(InputBindings.Jump)
            || Input.IsActionJustPressed(InputBindings.Interact)
            || Input.IsKeyPressed(Key.Enter);
    }

    private void UpdateCamera(double delta)
    {
        var target = Player.GlobalPosition + new Vector2(Player.Facing * 14.0f, -8.0f);
        _shakeTimer = Mathf.Max(0.0, _shakeTimer - delta);

        if (_shakeTimer > 0.0)
        {
            var falloff = (float)(_shakeTimer / 0.35);
            target += new Vector2(
                _rng.RandfRange(-_shakeStrength, _shakeStrength),
                _rng.RandfRange(-_shakeStrength, _shakeStrength)
            ) * Mathf.Clamp(falloff, 0.0f, 1.0f);
        }
        else
        {
            _shakeStrength = 0.0f;
        }

        _camera.GlobalPosition = target.Round();
    }

    private void ShakeCamera(float strength, float duration)
    {
        _shakeStrength = Mathf.Max(_shakeStrength, strength);
        _shakeTimer = Mathf.Max(_shakeTimer, duration);
    }

    private static string FormatRoomName(string roomId)
    {
        return roomId switch
        {
            "Room_001_Start" => "MOSS WAKE",
            "Room_002_Combat" => "OLD CROSSING",
            "Room_003_Flight" => "LANTERN SHAFT",
            "Room_004_Boss" => "SEALED CHAMBER",
            _ => roomId
        };
    }
}
