using UnityEngine;
using UnityEngine.UIElements;

public class HistoryUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        VisualElement sourceScreen = root.Q<VisualElement>("screen-source");
        VisualElement observationScreen = root.Q<VisualElement>("screen-observations");
        Button nextButton = root.Q<Button>("next-source");
        nextButton.clicked += () =>
        {
            Debug.Log("Er is op Ga Verder geklikt");
            sourceScreen.style.display = DisplayStyle.None;
            observationScreen.style.display = DisplayStyle.Flex;
        };

        TextField observationInput = root.Q<TextField>("observation-input");
        Button addObservationButton = root.Q<Button>("add-observation");
        ScrollView observationList = root.Q<ScrollView>("observations-list");

        void AddObservation()
        {
            string observation = observationInput.value.Trim();
            if (string.IsNullOrEmpty(observation))
            {
                Debug.Log("Observatie is leeg, kan niet toegevoegd worden.");
                return;
            }

            VisualElement observationCard = new VisualElement();
            observationCard.AddToClassList("observation-row");

            Label icon = new Label("▤");
            icon.AddToClassList("observation-icon");

            Label text = new Label(observation);
            text.AddToClassList("observation-text");

            Button deleteButton = new Button();
            deleteButton.text = "×";
            deleteButton.AddToClassList("observation-delete");

            deleteButton.clicked += () =>
            {
                observationCard.RemoveFromHierarchy();
                Debug.Log("Observatie verwijderd: " + observation);
            };
            observationCard.Add(icon);
            observationCard.Add(text);
            observationCard.Add(deleteButton);

            observationList.Add(observationCard);

            observationInput.value = "";

            observationInput.Focus();

            Debug.Log("Observatie toegevoegd: " + observation);
        };
        addObservationButton.clicked += AddObservation;
        observationInput.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return ||
                evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopImmediatePropagation();
                evt.PreventDefault();

                AddObservation();
            }
        }, TrickleDown.TrickleDown);

        VisualElement questionScreen = root.Q<VisualElement>("screen-select");
        Button getQuestion = root.Q<Button>("next-observations");
        Button backToSelect = root.Q<Button>("back-select");
        getQuestion.clicked += () =>
        {
            Debug.Log("Er is op ga verder geklikt");
            observationScreen.style.display = DisplayStyle.None;
            questionScreen.style.display = DisplayStyle.Flex;
        };
        backToSelect.clicked += () =>
        {
            Debug.Log("Er is op 'Vorige' geklikt");
            questionScreen.style.display = DisplayStyle.None;
            observationScreen.style.display = DisplayStyle.Flex;
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
