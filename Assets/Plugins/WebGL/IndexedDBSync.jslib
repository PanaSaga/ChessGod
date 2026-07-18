// Flushes Application.persistentDataPath's in-memory virtual filesystem out to the browser's
// IndexedDB. File.WriteAllText alone only writes into memory in a WebGL build - without this call,
// everything written there is lost the moment the page is closed or reloaded.
// Unity's Plugins/WebGL/ folder is automatically excluded from every non-WebGL build target.
mergeInto(LibraryManager.library, {
  SyncFileSystem: function () {
    FS.syncfs(false, function (err) {
      if (err) {
        console.error("IndexedDB syncfs failed: " + err);
      }
    });
  }
});
