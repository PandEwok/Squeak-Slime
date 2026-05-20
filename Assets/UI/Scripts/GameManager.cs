using UnityEngine;

public class GameManager : MonoBehaviour
{
    // The "Singleton" instance that other scripts can see
    //public static GameManager Instance { get; private set; }

    [Header("Global Progression State")]
    public int currentFloor = 1;
    public int currentStage = 1;
    private const int maxStages = 6;
    private const int maxFloors = 4;

    public static GameManager Instance
    {
        get
        {
            // If a script asks for the GameManager but it doesn't exist yet...
            if (_instance == null)
            {
                // Create a brand new invisible GameObject and attach the GameManager to it automatically
                GameObject go = new GameObject("Runtime_GameManager");
                _instance = go.AddComponent<GameManager>();
            }
            return _instance;
        }
    }
    private static GameManager _instance;

    /*    void Awake()
        {
            // Setup the Singleton and make sure it doesn't get destroyed between scenes
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }*/

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Call this whenever the player beats a combat stage!
    public void AdvanceStage()
    {
        currentStage++;

        if (currentStage > maxStages)
        {
            AdvanceFloor();
        }

        Debug.Log($"Advanced! Now on Floor {currentFloor}, Stage {currentStage}");
    }

    private void AdvanceFloor()
    {
        currentStage = 1;
        currentFloor++;

        if (currentFloor > maxFloors)
        {
            Debug.Log("Game Cleared! You beat the final floor!");
            // Handle victory screen or reset game here
        }
    }
}