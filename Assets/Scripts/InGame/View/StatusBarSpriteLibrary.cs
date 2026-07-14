using UnityEngine;

// Put one instance in the scene and assign the six UI sprites once.
public class StatusBarSpriteLibrary : MonoBehaviour
{
    [Header("HP")]
    [Tooltip("Filled heart: UI_HP_Heart")]
    public Sprite hpHeart;
    [Tooltip("Empty heart: UI_HP_Empty")]
    public Sprite hpEmpty;

    [Header("Buff (Up)")]
    public Sprite upBar;
    public Sprite upLine;

    [Header("Transform (Tr)")]
    public Sprite trBar;
    public Sprite trLine;
}
