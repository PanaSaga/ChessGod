using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Plays a sequence of dialogue lines (character name/text/background/standing) one at a
// time. Shared by the prologue and gallery cutscene replay. Actual dialogue content comes
// from IGameDataProvider.GetStoryLines(), which currently returns an empty list (Phase 5
// content not written yet) -- this script only owns the playback plumbing.
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

    [SerializeField] private GameObject storyRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    private List<DialogueLine> currentLines;
    private int currentIndex;
    private string currentStoryId;

    private void Awake()
    {
        if (storyRoot != null) storyRoot.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextLine);
    }

    public void PlayPrologue() => PlayStoryById("prologue");

    public void PlayStoryById(string storyId)
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[StoryManager] DataProvider가 아직 연결되지 않았습니다.");
            return;
        }

        List<StoryLineData> rawLines = GlobalManager.DataProvider.GetStoryLines(storyId);
        if (rawLines == null || rawLines.Count == 0)
        {
            Debug.LogWarning($"[StoryManager] '{storyId}' 스토리에 대사가 없습니다 (콘텐츠 미확정).");
            return;
        }

        List<DialogueLine> lines = new List<DialogueLine>();
        foreach (var raw in rawLines)
        {
            lines.Add(new DialogueLine
            {
                characterName = raw.characterName,
                dialogueText = raw.dialogueText,
                background = string.IsNullOrEmpty(raw.backgroundPath) ? null : Resources.Load<Sprite>(raw.backgroundPath),
                characterStanding = string.IsNullOrEmpty(raw.standingPath) ? null : Resources.Load<Sprite>(raw.standingPath)
            });
        }

        Play(lines, storyId);
    }

    // storyId is optional -- only "prologue" marks HasSeenPrologue() on completion.
    // Gallery replays call this with a null/empty id so they don't affect that flag.
    public void Play(List<DialogueLine> lines, string storyId = null)
    {
        if (lines == null || lines.Count == 0) return;

        currentLines = lines;
        currentIndex = 0;
        currentStoryId = storyId;

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

        if (currentStoryId == "prologue" && GlobalManager.DataProvider != null)
            GlobalManager.DataProvider.SetPrologueSeen();
    }
}
