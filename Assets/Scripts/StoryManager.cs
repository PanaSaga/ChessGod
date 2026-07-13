using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 스탠딩, 배경, 이름, 대사를 데이터 기반으로 순차 출력하는 골격 스크립트.
/// 프롤로그 컷씬과 갤러리 재열람(컷씬 다시보기) 양쪽에서 재사용 가능하도록 설계.
///
/// 붙이는 위치: 별도의 StoryUI 오브젝트 (MainScene 또는 공용 오버레이 캔버스)
/// 지금은 최소 기능만 구현되어 있으며, 타이핑 효과/선택지 분기는 추후 보강 필요.
/// </summary>
public class StoryManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string characterName;
        public string dialogueText;
        public Sprite background;
        public Sprite characterStanding;
    }

    [Header("출력용 UI 참조")]
    [SerializeField] private GameObject storyRoot; // 스토리 UI 전체를 켜고 끄는 최상위 오브젝트
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    private List<DialogueLine> currentLines;
    private int currentIndex;

    private void Awake()
    {
        if (storyRoot != null) storyRoot.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextLine);
    }

    // 프롤로그 재생 (LobbyManager에서 호출)
    public void PlayPrologue()
    {
        // TODO(게임팀/기획): 실제 프롤로그 대사 데이터를 여기에 연결
        List<DialogueLine> prologueLines = new List<DialogueLine>
        {
            new DialogueLine { characterName = "???", dialogueText = "(임시) 프롤로그 대사 자리" }
        };

        Play(prologueLines);
    }

    // 갤러리 재열람 등 외부에서 임의의 대사 목록을 재생할 때 사용
    public void Play(List<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0) return;

        currentLines = lines;
        currentIndex = 0;

        if (storyRoot != null) storyRoot.SetActive(true);
        ShowLine(currentIndex);
    }

    private void ShowNextLine()
    {
        currentIndex++;

        if (currentLines == null || currentIndex >= currentLines.Count)
        {
            EndStory();
            return;
        }

        ShowLine(currentIndex);
    }

    private void ShowLine(int index)
    {
        DialogueLine line = currentLines[index];

        if (nameText != null) nameText.text = line.characterName;
        if (dialogueText != null) dialogueText.text = line.dialogueText;
        if (backgroundImage != null && line.background != null) backgroundImage.sprite = line.background;
        if (characterImage != null && line.characterStanding != null) characterImage.sprite = line.characterStanding;
    }

    private void EndStory()
    {
        if (storyRoot != null) storyRoot.SetActive(false);

        // 프롤로그였다면 시청 여부를 저장
        if (GlobalManager.DataProvider != null)
        {
            GlobalManager.DataProvider.SetPrologueSeen();
        }
    }
}
