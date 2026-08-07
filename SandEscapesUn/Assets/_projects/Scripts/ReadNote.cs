using UnityEngine;
using SandEscapes.Interaction;

public class ReadNote : MonoBehaviour, IInteractable, IInteractablePromptProvider
{
    public string prompt = "E - Читать";

    public void OnInteract(GameObject interactor)
    {
        NoteViewer.Instance.Open();
    }

    public string GetPromptText()
    {
        return prompt;
    }
}