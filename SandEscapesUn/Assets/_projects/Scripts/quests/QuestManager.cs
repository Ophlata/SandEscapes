using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [System.Serializable]
    public class Quest
    {
        public string questName;

        public QuestType questType;

        public string targetID;

        public int requiredAmount = 1;

        [HideInInspector]
        public int currentAmount;

        [HideInInspector]
        public bool completed;
    }

    public enum QuestType
    {
        Inspect,
        Collect,
        Drink,
        ReachPoint,
        KillEnemy,
        Interact
    }

    public Quest[] quests;

    public int currentQuestIndex;

    [Header("UI")]
    public TextMeshProUGUI questText;
    public TextMeshProUGUI progressText;
    public GameObject completedMark;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip completeSound;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddProgress(string id)
    {
        if (currentQuestIndex >= quests.Length)
            return;

        Quest quest = quests[currentQuestIndex];

        if (quest.completed)
            return;

        if (quest.targetID != id)
            return;

        quest.currentAmount++;

        if (quest.currentAmount >= quest.requiredAmount)
        {
            CompleteQuest();
        }

        UpdateUI();
    }

    // Для квестов взаимодействия
    public void Interact(string id)
    {
        AddProgress(id);
    }

    void CompleteQuest()
    {
        Quest quest = quests[currentQuestIndex];

        quest.completed = true;

        Debug.Log("Квест выполнен: " + quest.questName);

        if (audioSource != null && completeSound != null)
            audioSource.PlayOneShot(completeSound);

        if (completedMark != null)
        {
            completedMark.SetActive(true);
            Invoke(nameof(HideMark), 2f);
        }

        currentQuestIndex++;

        Invoke(nameof(UpdateUI), 0.5f);
    }

    void HideMark()
    {
        if (completedMark != null)
            completedMark.SetActive(false);
    }

    void UpdateUI()
    {
        if (currentQuestIndex >= quests.Length)
        {
            questText.text = "Все задания выполнены";
            progressText.text = "";
            return;
        }

        Quest quest = quests[currentQuestIndex];

        questText.text = quest.questName;
        progressText.text = quest.currentAmount + " / " + quest.requiredAmount;
    }
}