using UnityEngine;

public class GameplayStatsManager : MonoBehaviour
{
    public static GameplayStatsManager instance { get; private set; }

    public int ZombiesKilled { get; private set; }
    public int DamageTaken { get; private set; }
    public int LifeHealed { get; private set; }

    private static GameplayStatsManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            GameplayStatsManager existing = FindFirstObjectByType<GameplayStatsManager>();
            if (existing != null)
                return existing;

            GameObject managerObject = new GameObject("GameplayStatsManager");
            return managerObject.AddComponent<GameplayStatsManager>();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void ResetStatsForNewRun()
    {
        Instance.ResetStats();
    }

    public static void RegisterZombieKilled()
    {
        Instance.ZombiesKilled++;
    }

    public static void RegisterDamageTaken(int amount)
    {
        if (amount <= 0)
            return;

        Instance.DamageTaken += amount;
    }

    public static void RegisterLifeHealed(int amount)
    {
        if (amount <= 0)
            return;

        Instance.LifeHealed += amount;
    }

    public static string BuildVictoryScoreText()
    {
        GameplayStatsManager stats = Instance;

        return "SCORE TABLE\n\n"
            + $"Zombies killed: {stats.ZombiesKilled}\n"
            + $"Damage taken: {stats.DamageTaken}\n"
            + $"Life healed: {stats.LifeHealed}";
    }

    private void ResetStats()
    {
        ZombiesKilled = 0;
        DamageTaken = 0;
        LifeHealed = 0;
    }
}
