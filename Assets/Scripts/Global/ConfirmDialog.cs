using System;
using UnityEngine;
using UnityEngine.UI;

// Generic Yes/No confirmation popup. The message text is authored directly on the panel in the
// Editor; call Show() with just the callbacks from anywhere.
public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onYes;
    private Action onNo;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnYesPressed);
        if (noButton != null) noButton.onClick.AddListener(OnNoPressed);
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Show(Action onYes, Action onNo = null)
    {
        this.onYes = onYes;
        this.onNo = onNo;
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Cancel() => OnNoPressed();

    private void OnYesPressed()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        onYes?.Invoke();
    }

    private void OnNoPressed()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        onNo?.Invoke();
    }
}
