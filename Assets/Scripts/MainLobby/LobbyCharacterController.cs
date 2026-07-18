using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCharacterController : MonoBehaviour
{
    [Serializable]
    public class LobbyLine
    {
        [TextArea] public string dialogue;
        [Tooltip("The standing sprite shown while this line is up.")]
        public Sprite sprite;
    }

    [Header("Character")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Button characterButton;

    [Header("Dialogue")]
    [SerializeField] private SlideUpPopup dialogueSlide;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private List<LobbyLine> lines = new();
    [SerializeField, Min(0.1f)] private float displayDuration = 5f;

    private Coroutine holdRoutine;

    private void Start()
    {
        if (characterButton == null) characterButton = GetComponent<Button>();
        if (characterButton != null) characterButton.onClick.AddListener(OnCharacterClicked);

        if (characterImage != null && defaultSprite != null) characterImage.sprite = defaultSprite;
    }

    // A new click always immediately swaps to a new line, even if one is already showing -
    // no need to wait for the previous line's hold/slide-out to finish first.
    private void OnCharacterClicked()
    {
        if (lines.Count == 0) return;

        LobbyLine line = lines[UnityEngine.Random.Range(0, lines.Count)];
        if (characterImage != null && line.sprite != null) characterImage.sprite = line.sprite;
        if (dialogueText != null) dialogueText.text = line.dialogue;

        dialogueSlide?.SlideIn();

        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(HoldThenHide());
    }

    private IEnumerator HoldThenHide()
    {
        yield return new WaitForSeconds(displayDuration);

        if (characterImage != null && defaultSprite != null) characterImage.sprite = defaultSprite;
        dialogueSlide?.SlideOut();
        holdRoutine = null;
    }
}
