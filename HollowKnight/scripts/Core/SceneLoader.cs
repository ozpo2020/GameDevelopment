using Godot;

public partial class SceneLoader : Node
{
    public void ChangeScene(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"Scene not found: {path}");
            return;
        }

        GetTree().ChangeSceneToFile(path);
    }
}

