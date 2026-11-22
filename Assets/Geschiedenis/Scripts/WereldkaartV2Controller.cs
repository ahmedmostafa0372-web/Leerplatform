using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

// UI Toolkit wereldkaart. Alle landen in de kaart zijn gekoppeld aan precies 1 van 15 regio's.
// De kleurcode in RegionMask.png is het regionummer (rood kanaal 1 t/m 15).
[RequireComponent(typeof(UIDocument))]
public class WereldkaartV2Controller : MonoBehaviour
{
    private readonly string[] names = {
        "", "Frankrijk", "Duitsland", "Verenigd Koninkrijk en Noord-Europa",
        "Midden-Europa", "Voormalige Sovjet-Unie", "Griekenland en de Balkan",
        "Italië", "Noord-Afrika", "Afrika", "Midden-Oosten",
        "Noord-Amerika", "Zuid-Amerika", "Spanje en Portugal",
        "India en omgeving", "Oost- en Zuidoost-Azië en Oceanië"
    };

    // Dit zijn voorlopige onderwerpen, later te vervangen door jullie echte vragen.
    private readonly string[] topics = {
        "", "De Franse Revolutie", "De Reformatie", "De Industriële Revolutie",
        "Geschiedenis van Midden-Europa", "De Russische Revolutie",
        "De Griekse oudheid", "Het Romeinse Rijk", "Het Oude Egypte",
        "Kolonisatie van Afrika", "Israël en Palestina", "De Amerikaanse onafhankelijkheid",
        "Kolonisatie en onafhankelijkheid", "Ontdekkingsreizen", "Brits-Indië",
        "Kolonisatie en dekolonisatie in Azië"
    };

    private VisualElement mapSurface, mapFrame, highlight, popup;
    private Label popupName, popupTopic, progressValue, selfValue;
    private VisualElement progressFill, selfFill;
    private Texture2D regionMask;
    private Texture2D[] overlays = new Texture2D[16];
    private readonly HashSet<int> conquered = new HashSet<int>();
    private int hoveredRegion = 0;
    private float mapWidth;

    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        StyleSheet style = Resources.Load<StyleSheet>("WorldMap/WorldMapStyle");
        if (style != null) root.styleSheets.Add(style);
        else Debug.LogError("WorldMapStyle.uss ontbreekt in Resources/WorldMap.");

        mapFrame = root.Q<VisualElement>("map-frame");
        mapSurface = root.Q<VisualElement>("map-surface");
        VisualElement mapBase = root.Q<VisualElement>("map-base");
        highlight = root.Q<VisualElement>("map-highlight");
        popup = root.Q<VisualElement>("region-popup");
        popupName = root.Q<Label>("popup-name");
        popupTopic = root.Q<Label>("popup-topic");
        progressValue = root.Q<Label>("progress-value");
        progressFill = root.Q<VisualElement>("progress-fill");
        selfValue = root.Q<Label>("leader-self-value");
        selfFill = root.Q<VisualElement>("leader-self-fill");

        Texture2D baseTexture = Resources.Load<Texture2D>("WorldMap/WorldBase");
        regionMask = Resources.Load<Texture2D>("WorldMap/RegionMask");
        if (baseTexture == null || regionMask == null)
        {
            Debug.LogError("WorldBase of RegionMask ontbreekt. Controleer de Resources/WorldMap map.");
            return;
        }
        mapBase.style.backgroundImage = new StyleBackground(baseTexture);
        for (int i = 1; i <= 15; i++)
            overlays[i] = Resources.Load<Texture2D>("WorldMap/Region" + i.ToString("00"));

        // Behoud de verhouding van de geografische kaart, zodat de hit-test klopt.
        mapSurface.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = mapSurface.resolvedStyle.width;
            if (width > 0 && Mathf.Abs(width - mapWidth) > 0.5f)
            {
                mapWidth = width;
                mapSurface.style.height = width * (float)baseTexture.height / baseTexture.width;
            }
        });

        mapSurface.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        mapFrame.RegisterCallback<PointerLeaveEvent>(_ => HidePopup());
        popup.RegisterCallback<PointerEnterEvent>(_ => { });
        root.Q<Button>("start-question").clicked += StartQuestion;
        popup.style.display = DisplayStyle.None;
        UpdateProgress();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (regionMask == null) return;
        Vector2 pos = evt.localPosition;
        float width = mapSurface.resolvedStyle.width;
        float height = mapSurface.resolvedStyle.height;
        if (width <= 0 || height <= 0) return;
        float u = pos.x / width;
        float v = pos.y / height;
        if (u < 0 || u >= 1 || v < 0 || v >= 1) { HidePopup(); return; }
        // PNG is top-down; Unity's Texture2D.GetPixel starts bottom-left.
        int x = Mathf.Clamp(Mathf.FloorToInt(u * regionMask.width), 0, regionMask.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((1f - v) * regionMask.height), 0, regionMask.height - 1);
        int id = regionMask.GetPixel(x, y).r > 0 ? Mathf.RoundToInt(regionMask.GetPixel(x, y).r * 255f) : 0;
        if (id < 1 || id > 15) { HidePopup(); return; }
        if (id != hoveredRegion)
        {
            hoveredRegion = id;
            highlight.style.backgroundImage = new StyleBackground(overlays[id]);
            popupName.text = names[id];
            popupTopic.text = "▤  " + topics[id];
            popup.Q<Button>("start-question").text = conquered.Contains(id) ? "Opnieuw oefenen  →" : "Beantwoord de vraag  →";
            popup.style.display = DisplayStyle.Flex;
        }
        // Pop-up volgt de aanwijzer en blijft binnen het kaartkader.
        float px = Mathf.Clamp(pos.x + 20f, 10f, Mathf.Max(10f, width - 275f));
        float py = Mathf.Clamp(pos.y + 16f, 10f, Mathf.Max(10f, height - 155f));
        popup.style.left = px;
        popup.style.top = py;
    }

    private void HidePopup()
    {
        hoveredRegion = 0;
        if (popup != null) popup.style.display = DisplayStyle.None;
        if (highlight != null) highlight.style.backgroundImage = StyleKeyword.None;
    }

    private void StartQuestion()
    {
        if (hoveredRegion == 0) return;
        // Koppelpunt voor jullie eigen vraagdata en voortgangssysteem.
        Debug.Log("Geselecteerde regio: " + names[hoveredRegion] + " (id=" + hoveredRegion + ")");
        if (Application.CanStreamedLevelBeLoaded("Geschiedenis"))
            SceneManager.LoadScene("Geschiedenis");
        else
            Debug.LogWarning("Voeg de scene 'Geschiedenis' toe aan File > Build Profiles > Scene List om hem te openen.");
    }

    // Later door Wouts voortgangssysteem aanroepen na een voldoende van Falco's LLM.
    public void SetConquered(int regionId)
    {
        if (regionId < 1 || regionId > 15) return;
        conquered.Add(regionId);
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        // Tijdelijke voortgang op basis van aantal regio's; vervang later door landoppervlakte.
        float pct = conquered.Count / 15f * 100f;
        string label = Mathf.RoundToInt(pct) + "%";
        progressValue.text = label;
        selfValue.text = label;
        progressFill.style.width = Length.Percent(pct);
        selfFill.style.width = Length.Percent(pct);
    }
}
