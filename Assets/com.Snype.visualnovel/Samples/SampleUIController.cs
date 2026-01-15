using UnityEngine;
using UnityEngine.UI;

public class SampleUIController : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private GameObject OptionsButtons;
    [SerializeField] private GameObject TextUI;

    public void TransitionBackground(Object bg)
    {
        background.sprite = (Sprite)bg;
    }

    public void ToggleOptions()
    {
        OptionsButtons.SetActive(!OptionsButtons.activeSelf);
        TextUI.SetActive(!TextUI.activeSelf);
    }

    public void EnableOptions()
    {
        OptionsButtons.SetActive(true);
    }

    public void DisableOptions()
    {
        OptionsButtons.SetActive(false);
    }

    public void EnableDialogue()
    {
        TextUI.SetActive(true);
    }

    public void DisableDialogue()
    {
        TextUI.SetActive(false);
    }

}
