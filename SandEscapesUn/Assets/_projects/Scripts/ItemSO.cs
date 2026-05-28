using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Item")]
public class ItemSO : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 99;
}