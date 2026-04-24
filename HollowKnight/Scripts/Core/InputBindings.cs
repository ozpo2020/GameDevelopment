using Godot;

public static class InputBindings
{
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";
    public const string MoveUp = "move_up";
    public const string MoveDown = "move_down";
    public const string Jump = "jump";
    public const string Attack = "attack";
    public const string Dash = "dash";
    public const string Interact = "interact";
    public const string WeaponNext = "weapon_next";
    public const string WeaponSword = "weapon_sword";
    public const string WeaponAxe = "weapon_axe";
    public const string WeaponHammer = "weapon_hammer";
    public const string Pause = "pause";

    public static void EnsureDefaults()
    {
        AddAction(MoveLeft, Key.A, Key.Left);
        AddAction(MoveRight, Key.D, Key.Right);
        AddAction(MoveUp, Key.W, Key.Up);
        AddAction(MoveDown, Key.S, Key.Down);
        AddAction(Jump, Key.Space, Key.K);
        AddAction(Attack, Key.J);
        AddMouseButton(Attack, MouseButton.Left);
        AddAction(Dash, Key.Shift, Key.L);
        AddAction(Interact, Key.E);
        AddAction(WeaponNext, Key.Q, Key.Tab);
        AddAction(WeaponSword, Key.Key1);
        AddAction(WeaponAxe, Key.Key2);
        AddAction(WeaponHammer, Key.Key3);
        AddAction(Pause, Key.Escape);

        AddJoyButton(Jump, JoyButton.A);
        AddJoyButton(Attack, JoyButton.X);
        AddJoyButton(Dash, JoyButton.RightShoulder);
        AddJoyButton(Interact, JoyButton.Y);
        AddJoyButton(WeaponNext, JoyButton.LeftShoulder);
        AddJoyButton(Pause, JoyButton.Start);
        AddJoyButton(MoveLeft, JoyButton.DpadLeft);
        AddJoyButton(MoveRight, JoyButton.DpadRight);
        AddJoyButton(MoveUp, JoyButton.DpadUp);
        AddJoyButton(MoveDown, JoyButton.DpadDown);
        AddJoyAxis(MoveLeft, JoyAxis.LeftX, -1.0f);
        AddJoyAxis(MoveRight, JoyAxis.LeftX, 1.0f);
        AddJoyAxis(MoveUp, JoyAxis.LeftY, -1.0f);
        AddJoyAxis(MoveDown, JoyAxis.LeftY, 1.0f);
    }

    private static void AddAction(string action, params Key[] keys)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);

        foreach (var key in keys)
        {
            var keyEvent = new InputEventKey { PhysicalKeycode = key };
            AddEvent(action, keyEvent);
        }
    }

    private static void AddMouseButton(string action, MouseButton button)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);

        var mouseEvent = new InputEventMouseButton { ButtonIndex = button };
        AddEvent(action, mouseEvent);
    }

    private static void AddJoyButton(string action, JoyButton button)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);

        AddEvent(action, new InputEventJoypadButton { ButtonIndex = button });
    }

    private static void AddJoyAxis(string action, JoyAxis axis, float axisValue)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action);

        AddEvent(action, new InputEventJoypadMotion { Axis = axis, AxisValue = axisValue });
    }

    private static void AddEvent(string action, InputEvent inputEvent)
    {
        if (!InputMap.ActionHasEvent(action, inputEvent))
            InputMap.ActionAddEvent(action, inputEvent);
    }
}
