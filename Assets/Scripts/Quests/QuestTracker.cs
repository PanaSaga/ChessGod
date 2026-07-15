using UnityEngine;

// Subscribes to GameSessionObserver (never to GameManager directly) and turns its signals
// into quest judgments. Actual judgment conditions are left as TODO on purpose --
// content isn't finalized yet, only the wiring/helper is. See CLAUDE.md's achievement
// table for the reference conditions this will eventually implement.
[RequireComponent(typeof(GameSessionObserver))]
public class QuestTracker : MonoBehaviour
{
    [Header("달성 시 Raise할 이벤트 채널 (없어도 동작함)")]
    [SerializeField] private QuestUnlockedEvent questUnlockedEvent;

    private GameSessionObserver observer;

    private void Awake()
    {
        observer = GetComponent<GameSessionObserver>();
    }

    private void OnEnable()
    {
        observer.OnEnemyDefeated += HandleEnemyDefeated;
        observer.OnMultiKill += HandleMultiKill;
        observer.OnStageCleared += HandleStageCleared;
        observer.OnScoreChanged += HandleScoreChanged;
        observer.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        observer.OnEnemyDefeated -= HandleEnemyDefeated;
        observer.OnMultiKill -= HandleMultiKill;
        observer.OnStageCleared -= HandleStageCleared;
        observer.OnScoreChanged -= HandleScoreChanged;
        observer.OnGameOver -= HandleGameOver;
    }

    // "체크메이트"(King 처치), "하극상"(Knight로 Queen 처치) 등에서 사용 예정
    private void HandleEnemyDefeated(ChessPieceType pieceType)
    {
        // TODO(콘텐츠 연결 후 구현)
    }

    // "포크"에서 사용 예정
    private void HandleMultiKill(int killCount)
    {
        // TODO(콘텐츠 연결 후 구현)
    }

    // "제노사이드"의 "2스테이지 이후" 판정에 사용 예정
    private void HandleStageCleared(int newStage)
    {
        // TODO(콘텐츠 연결 후 구현)
    }

    // "레이팅 마스터"(점수 2000점 이상)에서 사용 예정
    private void HandleScoreChanged(int currentScore)
    {
        // TODO(콘텐츠 연결 후 구현)
    }

    // "탁월수" 등 최종 결산이 필요한 업적이 있다면 여기서 처리
    // NOTE(시스템 확인 필요): 실제 GameManager.SettlementRoutine()은 플레이어 hp<=0이면
    // ResolvePlayerAttack() 없이 GameOver로 바로 빠지므로, "탁월수"(동귀어진) 조건이
    // 이 순서상 발생할 수 없다. 관찰자 패턴으로도 우회 불가 -- 판정 자체가 게임 로직
    // 내부에만 존재하는 순간이라 관찰 대상이 없다. 시스템 담당자 확인 필요.
    private void HandleGameOver(int finalScore)
    {
        // TODO(콘텐츠 연결 후 구현)
    }

    // 업적 진행도를 amount만큼 증가시키고, 목표치 도달 시 달성 처리 + 이벤트 Raise하는 공용 헬퍼.
    private void IncrementQuestProgress(string questId, int amount = 1)
    {
        if (GlobalManager.DataProvider == null) return;

        var quests = GlobalManager.DataProvider.GetAllQuests();
        var target = quests.Find(q => q.id == questId);
        if (target == null)
        {
            Debug.LogWarning($"[QuestTracker] '{questId}' 업적을 찾을 수 없습니다.");
            return;
        }
        if (target.isCompleted) return;

        int newProgress = Mathf.Min(target.currentProgress + amount, target.targetProgress);
        bool nowCompleted = newProgress >= target.targetProgress;

        GlobalManager.DataProvider.SaveQuestProgress(questId, newProgress, nowCompleted);

        if (nowCompleted && questUnlockedEvent != null)
        {
            target.currentProgress = newProgress;
            target.isCompleted = true;
            questUnlockedEvent.Raise(target);
        }
    }
}
