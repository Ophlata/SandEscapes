using UnityEngine;

public class InspectObject : MonoBehaviour
{
    private bool used;

    private void OnTriggerStay(Collider other)
    {
        if (used)
            return;

        if (other.CompareTag("Player"))
        {
            used = true;

            Debug.Log("Квест выполнен");

            GetComponent<QuestTarget>().TriggerQuest();
        }
    }
}