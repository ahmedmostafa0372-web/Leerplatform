using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

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
        List<string> observations = new List<string>();
        List<string> selectedObservations = new List<string>();

        void AddObservation()
        {
            string observation = observationInput.value.Trim();
            if (string.IsNullOrEmpty(observation))
            {
                Debug.Log("Observatie is leeg, kan niet toegevoegd worden.");
                return;
            }

            observations.Add(observation);
            Debug.Log("Observatie toegevoegd aan lijst: " + observation);
            Debug.Log("Huidige observaties: " + string.Join(", ", observations));

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
                observations.Remove(observation);
                Debug.Log("Observatie verwijderd: " + observation);
                Debug.Log("Huidige observaties: " + string.Join(", ", observations) + ")"); 
            };
            observationCard.Add(icon);
            observationCard.Add(text);
            observationCard.Add(deleteButton);

            observationList.Add(observationCard);

            observationInput.value = "";

            observationInput.Focus();
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
            ScrollView observationSelection = root.Q<ScrollView>("selection-list");
            foreach (string observation in observations)
                {
                Button selectionCard = new Button();
                selectionCard.AddToClassList("selection-card");

                Label checkbox = new Label("");

                Label observationText = new Label(observation);
                observationText.AddToClassList("selection-text");

                selectionCard.Add(checkbox);
                selectionCard.Add(observationText);

                if (selectedObservations.Contains(observation))
                {
                    selectionCard.AddToClassList("selection-card-selected");
                    checkbox.AddToClassList("selection-checkbox-selected");
                    checkbox.text = "✓";
                }

                checkbox.AddToClassList("selection-checkbox");

                observationSelection.Add(selectionCard);

                selectionCard.clicked += () =>
                {
                    Debug.Log("Observatie geselecteerd " + observation);
                    if (selectedObservations.Contains(observation))
                        {
                        selectedObservations.Remove(observation);

                        selectionCard.RemoveFromClassList("selection-card-selected");
                        checkbox.RemoveFromClassList("selection-checkbox-selected");
                        checkbox.text = "";
                    }
                    else
                    {
                        selectedObservations.Add(observation);

                        selectionCard.AddToClassList("selection-card-selected");
                        checkbox.AddToClassList("selection-checkbox-selected");
                        checkbox.text = "✓";
                    }
                    Debug.Log("Huidige geselecteerde observaties: " + string.Join(", ", selectedObservations) + ")");
                };
            }
        };
        backToSelect.clicked += () =>
        {
            Debug.Log("Er is op 'Vorige' geklikt");
            questionScreen.style.display = DisplayStyle.None;
            observationScreen.style.display = DisplayStyle.Flex;
        };
        
        VisualElement answerScreen = root.Q<VisualElement>("screen-answer");
        Button goToAnswer = root.Q<Button>("next-select");
        Button backToQuestion = root.Q<Button>("back-answer");


        goToAnswer.clicked += () =>
        {
            questionScreen.style.display = DisplayStyle.None;
            answerScreen.style.display = DisplayStyle.Flex;
        };

        backToQuestion.clicked += () =>
        {
            answerScreen.style.display = DisplayStyle.None;
            questionScreen.style.display = DisplayStyle.Flex;
        };

        Button submitAnswer = root.Q<Button>("submit-answer");
        VisualElement feedbackScreen = root.Q<VisualElement>("screen-feedback");
        submitAnswer.clicked += () =>
        {
            answerScreen.style.display = DisplayStyle.None;
            feedbackScreen.style.display = DisplayStyle.Flex;
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
