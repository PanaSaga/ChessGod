using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Shows quest-unlock toasts sequentially bottom-right. QuestEventListener's
// Response should be wired to Enqueue(QuestData).
//
// Placement: under GlobalManager (DontDestroyOnLoad), not inside InGame.unity -- unlike
// GameSessionObserver/QuestTracker, this has no dependency on GameManager.Instance,
// and keeping it persistent means a toast raised right before a GameOver scene transition
// isn't cut off mid-fade. QuestUnlockedEvent is a ScriptableObject asset, so it can
// still be raised from InGame regardless of where the listener lives.
public class QuestToastManager : MonoBehaviour
{
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Transform toastRoot;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.3f;

    private readonly Queue<QuestData> queue = new Queue<QuestData>();
    private bool isShowing;

    public void Enqueue(QuestData data)
    {
        queue.Enqueue(data);
        if (!isShowing) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;
        while (queue.Count > 0)
            yield return ShowToast(queue.Dequeue());
        isShowing = false;
    }

    private IEnumerator ShowToast(QuestData data)
    {
        GameObject toast = Instantiate(toastPrefab, toastRoot);
        CanvasGroup canvasGroup = toast.GetComponent<CanvasGroup>();

        TMP_Text[] texts = toast.GetComponentsInChildren<TMP_Text>();
        foreach (var t in texts)
            if (t.name == "Description") t.text = data.title;

        if (canvasGroup != null) yield return Fade(canvasGroup, 0f, 1f);
        yield return new WaitForSeconds(displayDuration);
        if (canvasGroup != null) yield return Fade(canvasGroup, 1f, 0f);

        Destroy(toast);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}
