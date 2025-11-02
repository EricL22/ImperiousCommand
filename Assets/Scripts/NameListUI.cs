using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;


[System.Serializable]
public class NameListSelectionData
{
    public string SelectedListId;
    public bool DynastyFirst;
}

public class NameListUI : MonoBehaviour
{
    public TextMeshProUGUI nameListTitle;
    public TextMeshProUGUI characterNamesDisplay;
    public TextMeshProUGUI shipNamesDisplay;
    public Toggle dynastyToggle;
    public TMP_InputField shipPrefixInput;

    public Button leftButton;
    public Button rightButton;
    public Button backButton;

    private List<string> availableLists;
    private int currentIndex = 0;

    private void Start()
    {
        availableLists = NameListLoader.instance.GetAvailableLists();

        if (availableLists.Count == 0)
        {
            Debug.LogError("No name lists available!");
            return;
        }

        UpdateUI();
        SaveSelection();

        leftButton.onClick.AddListener(PreviousNameList);
        rightButton.onClick.AddListener(NextNameList);
        backButton.onClick.AddListener(SaveSelection);
        shipPrefixInput.onValueChanged.AddListener(delegate { UpdateUI(); });
    }

    private void UpdateUI()
    {
        if (availableLists.Count == 0) return;

        string selectedId = availableLists[currentIndex];
        NameList selectedList = NameListLoader.instance.GetNameList(selectedId);

        nameListTitle.text = selectedList.DisplayName;
        dynastyToggle.isOn = selectedList.DynastyFirst;

        // Generate Character Names
        List<string> firstNames = selectedList.FirstNames;
        List<string> lastNames = selectedList.LastNames;
        characterNamesDisplay.text = GenerateSequentialNames(firstNames, lastNames, dynastyToggle.isOn);

        // Generate Ship Names
        // Apply ship prefix
        string prefix = shipPrefixInput.text.Trim();
        shipNamesDisplay.text = GenerateShipNames(selectedList.ShipNames, prefix);

        // Enable/Disable buttons based on position
        leftButton.interactable = currentIndex > 0;
        rightButton.interactable = currentIndex < availableLists.Count - 1;
    }

    private string GenerateSequentialNames(List<string> first, List<string> last, bool dynastyFirst)
    {
        int count = Mathf.Min(first.Count, last.Count);
        List<string> names = new List<string>();

        for (int i = 0; i < count; i++)
        {
            string name = dynastyFirst ? $"{last[i]} {first[i]}" : $"{first[i]} {last[i]}";
            names.Add(name);
        }

        return string.Join("\n", names);
    }

    private string GenerateShipNames(List<string> shipNames, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return string.Join("\n", shipNames);

        return string.Join("\n", shipNames.Select(name => prefix + " " + name));
    }

    private void PreviousNameList()
    {
        currentIndex--;
        UpdateUI();
    }

    private void NextNameList()
    {
        currentIndex++;
        UpdateUI();
    }

    private void SaveSelection()
    {
        if (availableLists.Count == 0) return;

        string selectedId = availableLists[currentIndex];
        PlayerData.instance.countries[0].nameListId = selectedId;
        PlayerData.instance.countries[0].shipPrefix = shipPrefixInput.text.Trim();
    }
}
