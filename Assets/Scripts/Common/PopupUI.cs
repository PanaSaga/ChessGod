using UnityEngine;
using UnityEngine.Events;

// Controls a single popup's show/hide state. Never fills in popup content itself --
// content-filling scripts (AchievementPopulator, InGamePauseController, ...) subscribe to
// OnOpened/OnClosed instead, so this stays reusable across every popup in the project.
public class PopupUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        OnOpened?.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }
}
