using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One selectable row in SaveSlotPickerPopup.
public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void Setup(string labelText, Action onClick)
    {
        if (label != null) label.text = labelText;
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
