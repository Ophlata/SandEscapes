using UnityEngine;

public class DrinkWater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<QuestTarget>().TriggerQuest();
        }
    }
}