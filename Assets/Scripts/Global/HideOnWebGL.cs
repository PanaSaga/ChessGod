using UnityEngine;

// Put this on anything that shouldn't appear in a WebGL build - e.g. an Exit/Quit button, since
// a web page can't force-close its own browser tab the way Application.Quit() does on desktop.
public class HideOnWebGL : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
