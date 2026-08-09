using UnityEngine;

public class ReachPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<QuestTarget>().TriggerQuest();
        }
    }
}