using UnityEngine;
using UnityEngine.Events;

// Click-detection scaffold for the lobby's center character. What actually happens on
// click (dialogue/expression/sound) is undecided -- OnCharacterClicked is left unbound
// until content is confirmed. Requires a Button component on the same object (or an Image
// with "Raycast Target" on) so clicks are detected.
[RequireComponent(typeof(UnityEngine.UI.Button))]
public class LobbyCharacterInteraction : MonoBehaviour
{
    public UnityEvent OnCharacterClicked;

    private void Awake()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnCharacterClicked?.Invoke());
    }
}
