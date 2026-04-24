using Godot;

public partial class SaveManager : Node
{
    private const string SavePath = "user://mossbound.cfg";

    public SaveData Current { get; private set; } = new();

    public void Load()
    {
        var config = new ConfigFile();
        var error = config.Load(SavePath);
        if (error != Error.Ok)
            return;

        Current.CheckpointRoom = (string)config.GetValue("save", "checkpoint_room", Current.CheckpointRoom);
        Current.CheckpointPosition = (Vector2)config.GetValue("save", "checkpoint_position", Current.CheckpointPosition);
        Current.HasDash = (bool)config.GetValue("save", "has_dash", Current.HasDash);
        Current.BossDefeated = (bool)config.GetValue("save", "boss_defeated", Current.BossDefeated);
    }

    public void Save()
    {
        var config = new ConfigFile();
        config.SetValue("save", "checkpoint_room", Current.CheckpointRoom);
        config.SetValue("save", "checkpoint_position", Current.CheckpointPosition);
        config.SetValue("save", "has_dash", Current.HasDash);
        config.SetValue("save", "boss_defeated", Current.BossDefeated);
        config.Save(SavePath);
    }

    public void SetCheckpoint(string roomId, Vector2 position)
    {
        Current.CheckpointRoom = roomId;
        Current.CheckpointPosition = position;
        Save();
    }

    public void SetDashUnlocked(bool unlocked)
    {
        Current.HasDash = unlocked;
        Save();
    }

    public void SetBossDefeated(bool defeated)
    {
        Current.BossDefeated = defeated;
        Save();
    }

    public void ResetRun()
    {
        Current = new SaveData();
        Save();
    }
}
