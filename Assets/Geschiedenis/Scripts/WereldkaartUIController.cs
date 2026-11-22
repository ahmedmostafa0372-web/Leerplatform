using UnityEngine;
using UnityEngine.UIElements;

// Alleen UI-voorbeeld: geen vraag- of databasekoppeling.
public class WereldkaartUIController : MonoBehaviour
{
    private readonly string[] regions = {
        "Frankrijk", "Duitsland", "Verenigd Koninkrijk", "Midden-Europa",
        "Voormalige Sovjet-Unie", "Griekenland", "Italië", "Noord-Afrika",
        "Centraal-Afrika", "Midden-Oosten", "Noord-Amerika", "Zuid-Amerika",
        "Spanje en Portugal", "India", "Azië en Oceanië"
    };

    private readonly string[] topics = {
        "Franse Revolutie", "Reformatie", "Industriële Revolutie", "Onderwerp nog te bepalen",
        "Russische Revolutie / Koude Oorlog", "Oude Grieken", "Romeinse Rijk", "Oude Egypte",
        "Europese kolonisatie", "Israël en Palestina", "Amerikaanse onafhankelijkheid",
        "Kolonisatie en onafhankelijkheid", "Ontdekkingsreizen", "Britse kolonisatie", "Zuidoost-Azië"
    };

    private int selectedRegion = -1;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var regionList = root.Q<ScrollView>("region-list");
        var regionTitle = root.Q<Label>("region-title");
        var regionDescription = root.Q<Label>("region-description");
        var startButton = root.Q<Button>("start-question");
        startButton.SetEnabled(false);

        for (int i = 0; i < regions.Length; i++)
        {
            int regionIndex = i;
            var button = new Button { text = (i + 1) + ".  " + regions[i] };
            button.AddToClassList("region-button");
            button.clicked += () =>
            {
                selectedRegion = regionIndex;
                foreach (var child in regionList.Children())
                    child.RemoveFromClassList("region-button-active");
                button.AddToClassList("region-button-active");
                regionTitle.text = regions[regionIndex];
                regionDescription.text = "Historisch onderwerp: " + topics[regionIndex];
                startButton.SetEnabled(true);
            };
            regionList.Add(button);
        }

        startButton.clicked += () =>
        {
            if (selectedRegion >= 0)
                Debug.Log("Start vraag voor regio: " + regions[selectedRegion] + " (koppeling volgt)");
        };
    }
}
