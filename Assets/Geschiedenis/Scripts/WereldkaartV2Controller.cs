using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class WereldkaartV2Controller : MonoBehaviour
{
    List<int> conquered = HistoryProgress.completedQuestions;

    Label progressValue;
    Label selfValue;
    VisualElement progressFill;
    VisualElement selfFill;

    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        string[] names = {
            "",
            "Frankrijk",
            "Duitsland",
            "Verenigd Koninkrijk en Noord-Europa",
            "Midden-Europa",
            "Voormalige Sovjet-Unie",
            "Griekenland en de Balkan",
            "Italië",
            "Noord-Afrika",
            "Afrika",
            "Midden-Oosten",
            "Noord-Amerika",
            "Zuid-Amerika",
            "Spanje en Portugal",
            "India en omgeving",
            "Oost- en Zuidoost-Azië en Oceanië"
        };

        string[] topics = {
            "",
            "De Franse Revolutie",
            "De Interbellumperiode",
            "De Industriële Revolutie",
            "De Reformatie",
            "De Russische Revolutie",
            "De Griekse oudheid",
            "Het Romeinse Rijk",
            "Het Oude Egypte",
            "Kolonisatie van Afrika",
            "Israël en Palestina",
            "De rol van de VS in de oorlog in Gaza",
            "Kolonisatie en onafhankelijkheid",
            "Ontdekkingsreizen",
            "Brits-Indië",
            "Kolonisatie en dekolonisatie in Azië"
        };

        int hoveredRegion = 0;
        int selectedRegion = 0;
        bool popupLocked = false;
        float mapWidth = 0;

        // Stylesheet laden
        StyleSheet style = Resources.Load<StyleSheet>(
            "WorldMap/WorldMapStyle"
        );

        if (style != null)
        {
            root.styleSheets.Add(style);
        }

        // UI-elementen van de wereldkaart
        VisualElement mapFrame = root.Q<VisualElement>("map-frame");
        VisualElement mapSurface = root.Q<VisualElement>("map-surface");
        VisualElement mapBase = root.Q<VisualElement>("map-base");
        VisualElement highlight = root.Q<VisualElement>("map-highlight");

        // UI-elementen van de pop-up
        VisualElement popup = root.Q<VisualElement>("region-popup");
        Label popupName = root.Q<Label>("popup-name");
        Label popupTopic = root.Q<Label>("popup-topic");
        Button startQuestion = root.Q<Button>("start-question");

        // UI-elementen van de voortgang
        progressValue = root.Q<Label>("progress-value");
        progressFill = root.Q<VisualElement>("progress-fill");
        selfValue = root.Q<Label>("leader-self-value");
        selfFill = root.Q<VisualElement>("leader-self-fill");

        // Afbeeldingen laden
        Texture2D baseTexture = Resources.Load<Texture2D>(
            "WorldMap/WorldBase"
        );

        Texture2D regionMask = Resources.Load<Texture2D>(
            "WorldMap/RegionMask"
        );

        if (baseTexture == null || regionMask == null)
        {
            Debug.LogError("WorldBase of RegionMask ontbreekt.");
            return;
        }

        mapBase.style.backgroundImage = new StyleBackground(baseTexture);

        Texture2D[] overlays = new Texture2D[16];

        for (int i = 1; i <= 15; i++)
        {
            string fileName = "WorldMap/Region" + i.ToString("00");
            overlays[i] = Resources.Load<Texture2D>(fileName);
        }
        // Veroverde regio's blauw weergeven
        foreach (int region in conquered)
        {
            VisualElement conqueredRegion = new VisualElement();

            conqueredRegion.style.position = Position.Absolute;

            conqueredRegion.style.left = 0;
            conqueredRegion.style.right = 0;
            conqueredRegion.style.top = 0;
            conqueredRegion.style.bottom = 0;

            conqueredRegion.style.backgroundImage =
                new StyleBackground(overlays[region]);

            conqueredRegion.pickingMode = PickingMode.Ignore;

            mapBase.Add(conqueredRegion);
        }

        // De juiste verhouding van de wereldkaart behouden
        mapSurface.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = mapSurface.resolvedStyle.width;

            if (width > 0 && Mathf.Abs(width - mapWidth) > 0.5f)
            {
                mapWidth = width;

                mapSurface.style.height =
                    width * (float)baseTexture.height / baseTexture.width;
            }
        });

        // Bepalen welke regio bij een muispositie hoort
        int GetRegion(Vector2 pos)
        {
            float width = mapSurface.resolvedStyle.width;
            float height = mapSurface.resolvedStyle.height;

            if (width <= 0 || height <= 0)
            {
                return 0;
            }

            float u = pos.x / width;
            float v = pos.y / height;

            if (u < 0 || u >= 1 || v < 0 || v >= 1)
            {
                return 0;
            }

            int x = Mathf.Clamp(
                Mathf.FloorToInt(u * regionMask.width),
                0,
                regionMask.width - 1
            );

            int y = Mathf.Clamp(
                Mathf.FloorToInt((1f - v) * regionMask.height),
                0,
                regionMask.height - 1
            );

            float redValue = regionMask.GetPixel(x, y).r;

            int id = Mathf.RoundToInt(redValue * 255f);

            if (id < 1 || id > 15)
            {
                return 0;
            }

            return id;
        }

        // Pop-up verbergen
        void HidePopup()
        {
            hoveredRegion = 0;
            selectedRegion = 0;
            popupLocked = false;

            popup.style.display = DisplayStyle.None;
            highlight.style.backgroundImage = StyleKeyword.None;
        }

        // Pop-up tonen en inhoud aanpassen
        void ShowPopup(int id, Vector2 pos)
        {
            hoveredRegion = id;

            highlight.style.backgroundImage =
                new StyleBackground(overlays[id]);

            popupName.text = names[id];
            popupTopic.text = "▤  " + topics[id];

            if (conquered.Contains(id))
            {
                startQuestion.text = "Opnieuw oefenen →";
            }
            else if (HistoryProgress.startedQuestions.Contains(id))
            {
                startQuestion.text = "Verder oefenen →";
            }
            else
            {
                startQuestion.text = "Beantwoord de vraag →";
            }

            popup.style.display = DisplayStyle.Flex;

            float width = mapSurface.resolvedStyle.width;
            float height = mapSurface.resolvedStyle.height;

            float px = Mathf.Clamp(
                pos.x + 20f,
                10f,
                Mathf.Max(10f, width - 275f)
            );

            float py = Mathf.Clamp(
                pos.y + 16f,
                10f,
                Mathf.Max(10f, height - 155f)
            );

            popup.style.left = px;
            popup.style.top = py;
        }

        // Hover over de wereldkaart
        mapSurface.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (popupLocked)
            {
                return;
            }

            Vector2 pos = mapSurface.WorldToLocal(evt.position);

            int id = GetRegion(pos);

            if (id == 0)
            {
                HidePopup();
                return;
            }

            ShowPopup(id, pos);
        });

        // Klikken op een regio
        mapSurface.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0)
            {
                return;
            }

            Vector2 pos = mapSurface.WorldToLocal(evt.position);

            int id = GetRegion(pos);

            if (id == 0)
            {
                HidePopup();
                return;
            }

            if (popupLocked && selectedRegion == id)
            {
                HidePopup();
                return;
            }

            selectedRegion = id;
            popupLocked = true;

            ShowPopup(id, pos);

            Debug.Log("Regio geselecteerd: " + names[id]);
            Debug.Log("Geselecteerde regio ID: " + selectedRegion);
        });

        // Pop-up verbergen als de muis de kaart verlaat
        mapFrame.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            if (popupLocked == false)
            {
                HidePopup();
            }
        });

        // Voorkomen dat klikken op de pop-up
        // ook als klikken op de kaart wordt geregistreerd
        popup.RegisterCallback<PointerDownEvent>(evt =>
        {
            evt.StopPropagation();
        });

        // Geschiedenisvraag openen
        startQuestion.clicked += () =>
        {
            if (selectedRegion == 0)
            {
                return;
            }

            Debug.Log("Er is op Beantwoord de vraag geklikt");
            Debug.Log("Regio: " + names[selectedRegion]);
            Debug.Log("Onderwerp: " + topics[selectedRegion]);

            if (selectedRegion == 1)
            {
                OpenScene("VraagFrankrijk");
            }
            else if (selectedRegion == 2)
            {
                OpenScene("VraagDuitsland");
            }
            else if (selectedRegion == 3)
            {
                OpenScene("VraagNoordEuropa");
            }
            else if (selectedRegion == 4)
            {
                OpenScene("VraagMiddenEuropa");
            }
            else if (selectedRegion == 5)
            {
                OpenScene("VraagSovjetUnie");
            }
            else if (selectedRegion == 6)
            {
                OpenScene("VraagGriekenland");
            }
            else if (selectedRegion == 7)
            {
                OpenScene("VraagItalie");
            }
            else if (selectedRegion == 8)
            {
                OpenScene("VraagNoordAfrika");
            }
            else if (selectedRegion == 9)
            {
                OpenScene("VraagAfrika");
            }
            else if (selectedRegion == 10)
            {
                OpenScene("VraagMiddenOosten");
            }
            else if (selectedRegion == 11)
            {
                OpenScene("VraagNoordAmerika");
            }
            else if (selectedRegion == 12)
            {
                OpenScene("VraagZuidAmerika");
            }
            else if (selectedRegion == 13)
            {
                OpenScene("VraagSpanjePortugal");
            }
            else if (selectedRegion == 14)
            {
                OpenScene("VraagIndia");
            }
            else if (selectedRegion == 15)
            {
                OpenScene("VraagAzie");
            }
        };

        popup.style.display = DisplayStyle.None;

        UpdateProgress();
    }

    // Een scene openen
    void OpenScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene is nog niet beschikbaar: " + sceneName);
        }
    }

    // Een regio registreren als veroverd
    public void SetConquered(int regionId)
    {
        if (regionId < 1 || regionId > 15)
        {
            return;
        }

        if (!conquered.Contains(regionId))
        {
            conquered.Add(regionId);
        }

        Debug.Log("Regio veroverd: " + regionId);

        UpdateProgress();
    }

    // De voortgang berekenen
    void UpdateProgress()
    {
        float percentage = conquered.Count / 15f * 100f;

        string label = Mathf.RoundToInt(percentage) + "%";

        progressValue.text = label;
        selfValue.text = label;

        progressFill.style.width = Length.Percent(percentage);
        selfFill.style.width = Length.Percent(percentage);
    }
}