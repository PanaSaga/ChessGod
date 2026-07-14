using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 업적 달성 시 화면 우측 하단에 순차적으로 뜨는 토스트 알림을 관리합니다.
/// AchievementUnlockedEvent(데이터를 실어 나르는 이벤트 채널)를 AchievementEventListener로
/// 구독하고, Response에 이 스크립트의 Enqueue(AchievementData)를 연결해서 사용합니다.
///
/// 붙이는 위치: 공용 오버레이 캔버스 하위 "AchievementToastManager"
///   (같은 오브젝트에 AchievementEventListener.cs도 함께 부착)
///
/// [자원 경합 방지] 토스트 처리 중에는 DataManager.IsUpdating을 확인해
/// AchievementPopulator와 동시에 리스트를 갱신하지 않도록 합니다.
/// </summary>
public class AchievementToastManager : MonoBehaviour
{
    [Header("토스트 프리팹 (Title/Description Text 포함)")]
    [SerializeField] private GameObject toastPrefab;

    [Header("토스트가 생성될 부모 (화면 우측 하단 Anchor)")]
    [SerializeField] private Transform toastRoot;

    [Header("토스트 표시 시간(초)")]
    [SerializeField] private float displayDuration = 3f;

    [Header("페이드 시간(초)")]
    [SerializeField] private float fadeDuration = 0.3f;

    private readonly Queue<AchievementData> queue = new Queue<AchievementData>();
    private bool isShowing;

    // AchievementEventListener의 Response에 연결 (파라미터 1개 함수라 인스펙터 드롭다운에 정상 표시됨)
    public void Enqueue(AchievementData data)
    {
        queue.Enqueue(data);

        if (!isShowing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            AchievementData data = queue.Dequeue();
            yield return ShowToast(data);
        }

        isShowing = false;
    }

    private IEnumerator ShowToast(AchievementData data)
    {
        GameObject toast = Instantiate(toastPrefab, toastRoot);
        CanvasGroup canvasGroup = toast.GetComponent<CanvasGroup>();

        // 텍스트 채우기 (프리팹 구조에 맞게 이름 매칭, 필요 시 전용 SlotUI 스크립트로 교체 가능)
        TMP_Text[] texts = toast.GetComponentsInChildren<TMP_Text>();
        foreach (var t in texts)
        {
            if (t.name == "Description") t.text = data.title;
        }

        // 페이드 인
        if (canvasGroup != null)
        {
            yield return Fade(canvasGroup, 0f, 1f);
        }

        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃
        if (canvasGroup != null)
        {
            yield return Fade(canvasGroup, 1f, 0f);
        }

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
