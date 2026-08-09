using UnityEngine;

public class ItemHighlight : MonoBehaviour
{
    
    [SerializeField] Renderer[] renderers;
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] float emissionIntensity = 2f;

    MaterialPropertyBlock block;

    void Awake()
    {
        block = new MaterialPropertyBlock();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetHighlight(bool enabled)
    {
        foreach (var rend in renderers)
        {
            rend.GetPropertyBlock(block);

            if (enabled)
            {
                block.SetColor("_EmissionColor",
                    highlightColor * emissionIntensity);
            }
            else
            {
                block.SetColor("_EmissionColor", Color.black);
            }

            rend.SetPropertyBlock(block);
        }
    }
}   
