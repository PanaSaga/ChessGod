// Single source of truth for scene names used across multiple scripts (previously each script
// had its own separately-typed default, e.g. "MainLobby", risking a silent mismatch if a scene
// ever gets renamed in Build Settings without updating every copy).
public static class SceneNames
{
    public const string GameStart = "00_GameStart";
    public const string MainLobby = "01_MainLobby";
    public const string InGame = "02_InGame";
    public const string Story = "03_Story";
    public const string Gallery = "04_Gallery";
}
