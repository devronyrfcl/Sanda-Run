using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    // Singleton instance
    public static CurrencyManager Instance;

    // Public variables for start coin and current coin
    public int startCoinValue = 1000; // Default starting coin value
    public int currentCoinValue; // Current coin value, this will be modified during the game

    // PlayerPrefs key for currency
    private const string CoinKey = "CoinCount";

    void Awake()
    {
        // Ensure singleton instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep this object alive between scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize current coin value with the starting value
        InitializeCoins();
    }

    // Initialize current coin value, either from PlayerPrefs or using the start coin value
    private void InitializeCoins()
    {
        // Load saved coin data, if it exists
        currentCoinValue = PlayerPrefs.GetInt(CoinKey, startCoinValue); // Default to startCoinValue if no saved data
    }

    // Save coin count to PlayerPrefs
    private void SaveCoinData()
    {
        PlayerPrefs.SetInt(CoinKey, currentCoinValue);
        PlayerPrefs.Save();  // Ensure data is saved immediately
    }

    // Get the current coin count
    public int GetCoinCount()
    {
        return currentCoinValue;
    }

    // Set the current coin count (use with caution as this overrides the value)
    public void SetCoinCount(int newCoinCount)
    {
        currentCoinValue = newCoinCount;
        SaveCoinData();
    }

    // Add coins to the current coin count
    public void AddCoins(int amount)
    {
        currentCoinValue += amount;
        SaveCoinData();
    }

    // Subtract coins from the current coin count
    public void SubtractCoins(int amount)
    {
        currentCoinValue = Mathf.Max(0, currentCoinValue - amount);  // Prevent negative coins
        SaveCoinData();
    }

    // Reset coins back to the start value (useful for game reset)
    public void ResetCoins()
    {
        currentCoinValue = startCoinValue;
        SaveCoinData();
    }
}
