using System.Runtime.InteropServices;

// On WebGL, Application.persistentDataPath writes only live in an in-memory virtual filesystem
// until explicitly flushed to the browser's IndexedDB - call Sync() right after any File.Write.
// No-op on every other platform (and in the Editor), so this is always safe to call.
public static class WebGLStorageSync
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncFileSystem();

    public static void Sync() => SyncFileSystem();
#else
    public static void Sync() { }
#endif
}
