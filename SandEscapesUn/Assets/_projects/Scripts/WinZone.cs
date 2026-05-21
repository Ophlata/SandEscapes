using UnityEngine;

public class WinZone : MonoBehaviour
{
    public GameObject winPanel;

    private bool won = false;

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (won) return;

        if (other.CompareTag("Player"))
        {
            won = true;

            Debug.Log("YOU WIN");

            if (winPanel != null)
                winPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 0f;
        }
    }
}