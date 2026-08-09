using UnityEngine;

public class QuestTarget : MonoBehaviour
{
    public string targetID;

    public bool destroyAfterUse;

    public void TriggerQuest()
    {
        QuestManager.Instance.AddProgress(targetID);

        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }
}