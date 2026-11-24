using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class VraagMiddenOosten : MonoBehaviour
{
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        int regionId = 10;

        if (!HistoryProgress.startedQuestions.Contains(regionId))
        {
            HistoryProgress.startedQuestions.Add(regionId);
        }
        Debug.Log("Vraag het Midden-Oosten gestart");

        // De spotprent laden en op de drie schermen tonen
        Texture2D bron = Resources.Load<Texture2D>("VraagMiddenOosten/Bron");

        if (bron == null)
        {
            Debug.LogWarning("Afbeelding ontbreekt: VraagMiddenOosten/Bron");
        }

        Image sourceImage = root.Q<Image>("source-poster");
        Image observationImage = root.Q<Image>("observation-poster");
        Image selectImage = root.Q<Image>("select-poster");

        sourceImage.image = bron;
        observationImage.image = bron;
        selectImage.image = bron;

        // Naar de observaties
        VisualElement sourceScreen = root.Q<VisualElement>("screen-source");
        VisualElement observationScreen = root.Q<VisualElement>("screen-observations");
        Button nextButton = root.Q<Button>("next-source");
        nextButton.clicked += () =>
        {
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
                return;
            }
            if (observations.Contains(observation))
            {
                return;
            }
            observations.Add(observation);
            Debug.Log("Observatie toegevoegd: " + observation);
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
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
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
                    Debug.Log("Geselecteerde observaties: " + string.Join(", ", selectedObservations));
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

        // Antwoord beoordelen en voortgang opslaan
        VisualElement feedbackScreen = root.Q<VisualElement>("screen-feedback");
        Button submitAnswer = root.Q<Button>("submit-answer");
        Button backFeedback = root.Q<Button>("back-feedback");
        Button retryButton = root.Q<Button>("retry-button");
        Label feedbackStatus = root.Q<Label>("feedback-status");
        Label feedbackText = root.Q<Label>("feedback-text");
        submitAnswer.clicked += () =>
        {
            string answer = answerInput.value.Trim();
            if (string.IsNullOrEmpty(answer))
            {
                Debug.Log("Het antwoord is leeg.");
                answerInput.Focus();
                return;
            }
            // Tijdelijke willekeurige beoordeling, later vervangen door de LLM
            int result = Random.Range(0, 2);
            bool answerCorrect = false;
            if (result == 1)
            {
                answerCorrect = true;
            }
            HistoryProgress.submittedQuestions.Add(regionId);
            HistoryProgress.submittedAnswers.Add(answer);
            HistoryProgress.answerResults.Add(answerCorrect);
            Debug.Log("Antwoord ingeleverd: " + answer);
            Debug.Log("Gebruikte observaties: " + string.Join(", ", selectedObservations));
            if (answerCorrect)
            {
                feedbackStatus.text = "Voldoende!";
                feedbackText.text = "Je hebt deze vraag succesvol afgerond. Je hebt het Midden-Oosten veroverd.\n\nDit is een gesimuleerde beoordeling.";
                if (!HistoryProgress.completedQuestions.Contains(regionId))
                {
                    HistoryProgress.completedQuestions.Add(regionId);
                }
                backFeedback.style.display = DisplayStyle.None;
            }
            else
            {
                feedbackStatus.text = "Onvoldoende";
                feedbackText.text = "Probeer je antwoord te verbeteren.\n\nDit is een gesimuleerde beoordeling; je antwoord is nog niet inhoudelijk gecontroleerd.";
                backFeedback.style.display = DisplayStyle.Flex;
            }
            retryButton.text = "Terug naar wereldkaart →";
            Debug.Log("Begonnen vragen: " + string.Join(", ", HistoryProgress.startedQuestions));
            Debug.Log("Afgeronde vragen: " + string.Join(", ", HistoryProgress.completedQuestions));
            Debug.Log("Aantal ingediende antwoorden: " + HistoryProgress.submittedAnswers.Count);
            answerScreen.style.display = DisplayStyle.None;
            feedbackScreen.style.display = DisplayStyle.Flex;
        };
        backFeedback.clicked += () =>
        {
            feedbackScreen.style.display = DisplayStyle.None;
            answerScreen.style.display = DisplayStyle.Flex;
        };
        retryButton.clicked += () =>
        {
            SceneManager.LoadScene("Wereldkaart");
        };
    }
}
