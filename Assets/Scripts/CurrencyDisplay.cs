using UnityEngine;
using TMPro;

public class CurrencyDisplay : MonoBehaviour
{
    // Array of TMP Text objects to display the coin count
    public TextMeshProUGUI[] coinTexts;
    public TextMeshProUGUI highestTrophyText;  // TMP Text to display highest trophy
    public TextMeshProUGUI totalTrophyText;    // TMP Text to display total trophies

    void Update()
    {
        // Update the currency display when the scene starts
        UpdateCurrencyDisplay();
    }

    // Call this method to update the currency display on the screen
    public void UpdateCurrencyDisplay()
    {
        // Get the current coin count from CurrencyManager
        int coinCount = CurrencyManager.Instance.GetCoinCount();

        // Update all the TextMeshPro objects in the array for coins
        foreach (TextMeshProUGUI coinText in coinTexts)
        {
            coinText.text = coinCount.ToString();  // Display the current coin count
        }

        // Retrieve the highest trophy value from PlayerPrefs
        int highestTrophy = PlayerPrefs.GetInt("HighestTrophy", 0);

        // Update the TMP text for highest trophy
        if (highestTrophyText != null)
        {
            highestTrophyText.text = "Highest Trophies: " + highestTrophy;
        }

        // Retrieve the total trophy value from PlayerPrefs
        int totalTrophy = PlayerPrefs.GetInt("TotalTrophy", 0);

        // Update the TMP text for total trophies
        if (totalTrophyText != null)
        {
            totalTrophyText.text = "Total Trophies: " + totalTrophy;
        }
    }
}
