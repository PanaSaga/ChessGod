using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Reusable "정말 하시겠습니까?" yes/no confirmation, gating destructive actions like
// progress reset. Not persistent -- one instance per scene that needs it.
public class ConfirmDialogManager : MonoBehaviour
{
    public static ConfirmDialogManager Instance { get; private set; }

    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onConfirmed;

    private void Awake()
    {
        Instance = this;

        if (dialogRoot != null) dialogRoot.SetActive(false);
        if (yesButton != null) yesButton.onClick.AddListener(OnYesClicked);
        if (noButton != null) noButton.onClick.AddListener(OnNoClicked);
    }

    public void Show(string message, Action onConfirm)
    {
        onConfirmed = onConfirm;
        if (messageText != null) messageText.text = message;
        if (dialogRoot != null) dialogRoot.SetActive(true);
    }

    private void OnYesClicked()
    {
        if (dialogRoot != null) dialogRoot.SetActive(false);
        onConfirmed?.Invoke();
        onConfirmed = null;
    }

    private void OnNoClicked()
    {
        if (dialogRoot != null) dialogRoot.SetActive(false);
        onConfirmed = null;
    }
}
