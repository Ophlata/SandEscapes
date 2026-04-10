namespace SandEscapes.Interaction
{
    /// <summary>
    /// Optional prompt text for the interaction hint UI (e.g. "E - Pick up").
    /// </summary>
    public interface IInteractablePromptProvider
    {
        string GetPromptText();
    }
}
