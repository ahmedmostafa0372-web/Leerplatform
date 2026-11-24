using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class HistoryUIController : MonoBehaviour
{
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        // Midden-Europa
        int regionId = 4;

        // Registreren dat de vraag is begonnen
        if (!HistoryProgress.startedQuestions.Contains(regionId))
        {
            HistoryProgress.startedQuestions.Add(regionId);
        }

        Debug.Log("Vraag gestart: " + regionId);

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

        // Observaties toevoegen
        void AddObservation()
        {
            string observation = observationInput.value.Trim();

            if (string.IsNullOrEmpty(observation))
            {
                Debug.Log("Observatie is leeg.");
                return;
            }

            if (observations.Contains(observation))
            {
                Debug.Log("Deze observatie bestaat al.");
                return;
            }

            observations.Add(observation);

            Debug.Log("Observatie toegevoegd: " + observation);
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
                selectedObservations.Remove(observation);

                Debug.Log("Observatie verwijderd: " + observation);
            };

            observationCard.Add(icon);
            observationCard.Add(text);
            observationCard.Add(deleteButton);

            observationList.Add(observationCard);

            observationInput.value = "";
            observationInput.Focus();
        }

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

        // Observaties selecteren
        VisualElement questionScreen = root.Q<VisualElement>("screen-select");

        Button getQuestion = root.Q<Button>("next-observations");
        Button backToSelect = root.Q<Button>("back-select");

        ScrollView observationSelection = root.Q<ScrollView>("selection-list");

        getQuestion.clicked += () =>
        {
            Debug.Log("Er is op Ga Verder geklikt");

            observationScreen.style.display = DisplayStyle.None;
            questionScreen.style.display = DisplayStyle.Flex;

            observationSelection.Clear();

            foreach (string observation in observations)
            {
                Button selectionCard = new Button();
                selectionCard.AddToClassList("selection-card");

                Label checkbox = new Label("");
                checkbox.AddToClassList("selection-checkbox");

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

                observationSelection.Add(selectionCard);

                selectionCard.clicked += () =>
                {
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

                    Debug.Log("Geselecteerde observaties: " +
                        string.Join(", ", selectedObservations));
                };
            }
        };

        backToSelect.clicked += () =>
        {
            questionScreen.style.display = DisplayStyle.None;
            observationScreen.style.display = DisplayStyle.Flex;
        };

        // Antwoord formuleren
        VisualElement answerScreen = root.Q<VisualElement>("screen-answer");

        Button goToAnswer = root.Q<Button>("next-select");
        Button backToQuestion = root.Q<Button>("back-answer");

        ScrollView chosenList = root.Q<ScrollView>("chosen-list");
        TextField answerInput = root.Q<TextField>("answer-input");

        goToAnswer.clicked += () =>
        {
            questionScreen.style.display = DisplayStyle.None;
            answerScreen.style.display = DisplayStyle.Flex;

            chosenList.Clear();

            foreach (string observation in selectedObservations)
            {
                VisualElement chosenCard = new VisualElement();
                chosenCard.AddToClassList("observation-row");

                Label icon = new Label("✓");
                icon.AddToClassList("observation-icon");

                Label text = new Label(observation);
                text.AddToClassList("observation-text");

                chosenCard.Add(icon);
                chosenCard.Add(text);

                chosenList.Add(chosenCard);
            }
        };

        backToQuestion.clicked += () =>
        {
            answerScreen.style.display = DisplayStyle.None;
            questionScreen.style.display = DisplayStyle.Flex;
        };

        // Feedback
        VisualElement feedbackScreen = root.Q<VisualElement>("screen-feedback");

        Button submitAnswer = root.Q<Button>("submit-answer");
        Button backFeedback = root.Q<Button>("back-feedback");
        Button retryButton = root.Q<Button>("retry-button");

        Label feedbackStatus = root.Q<Label>("feedback-status");
        Label feedbackText = root.Q<Label>("feedback-text");

        bool answerCorrect = false;

        // Antwoord inleveren
        submitAnswer.clicked += () =>
        {
            string answer = answerInput.value.Trim();

            if (string.IsNullOrEmpty(answer))
            {
                Debug.Log("Het antwoord is leeg.");
                answerInput.Focus();
                return;
            }

            Debug.Log("Antwoord ingeleverd: " + answer);

            Debug.Log("Gebruikte observaties: " +
                string.Join(", ", selectedObservations));

            // Tijdelijke beoordeling
            int result = Random.Range(0, 2);

            if (result == 1)
            {
                answerCorrect = true;
            }
            else
            {
                answerCorrect = false;
            }

            // Antwoord en resultaat opslaan
            HistoryProgress.submittedQuestions.Add(regionId);
            HistoryProgress.submittedAnswers.Add(answer);
            HistoryProgress.answerResults.Add(answerCorrect);

            // Feedback weergeven
            if (answerCorrect)
            {
                feedbackStatus.text = "Voldoende!";

                feedbackText.text =
                    "Je hebt deze vraag succesvol afgerond. " +
                    "Je hebt Midden-Europa veroverd.\n\n" +
                    "Dit is een gesimuleerde beoordeling.";

                if (!HistoryProgress.completedQuestions.Contains(regionId))
                {
                    HistoryProgress.completedQuestions.Add(regionId);
                }

                backFeedback.style.display = DisplayStyle.None;

                retryButton.text = "Terug naar wereldkaart →";

                Debug.Log("Vraag succesvol afgerond: " + regionId);
            }
            else
            {
                feedbackStatus.text = "Onvoldoende";

                feedbackText.text =
                    "Je antwoord is nog niet voldoende. " +
                    "Probeer je antwoord te verbeteren.\n\n" +
                    "Dit is een gesimuleerde beoordeling.";

                backFeedback.style.display = DisplayStyle.Flex;

                retryButton.text = "Terug naar wereldkaart →";

                Debug.Log("Vraag niet succesvol afgerond: " + regionId);
            }

            Debug.Log("Begonnen vragen: " +
                string.Join(", ", HistoryProgress.startedQuestions));

            Debug.Log("Afgeronde vragen: " +
                string.Join(", ", HistoryProgress.completedQuestions));

            Debug.Log("Aantal ingediende antwoorden: " +
                HistoryProgress.submittedAnswers.Count);

            answerScreen.style.display = DisplayStyle.None;
            feedbackScreen.style.display = DisplayStyle.Flex;
        };

        // Antwoord aanpassen
        backFeedback.clicked += () =>
        {
            feedbackScreen.style.display = DisplayStyle.None;
            answerScreen.style.display = DisplayStyle.Flex;
        };

        // Terug naar de wereldkaart
        retryButton.clicked += () =>
        {
            Debug.Log("Terug naar de wereldkaart");

            SceneManager.LoadScene("Wereldkaart");
        };
    }
}