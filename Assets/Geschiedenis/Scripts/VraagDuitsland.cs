using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class VraagDuitsland : MonoBehaviour
{
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        int regionId = 2;

        if (!HistoryProgress.startedQuestions.Contains(regionId))
        {
            HistoryProgress.startedQuestions.Add(regionId);
        }
        Debug.Log("Vraag Duitsland gestart");

        // Bronnen laden
        Texture2D[] bronnen = new Texture2D[3];
        bronnen[0] = Resources.Load<Texture2D>("VraagDuitsland/Bron1");
        bronnen[1] = Resources.Load<Texture2D>("VraagDuitsland/Bron2");
        bronnen[2] = Resources.Load<Texture2D>("VraagDuitsland/Bron3");

        string[] beschrijvingen = {
            "Bron 1 — Duitse verkiezingsposter, 1932.\n\nVertaling: Kies Hitler! De leider die ons uit ellende en nood leidt!",
            "Bron 2 — Duitse verkiezingsposter, 1932.\nOntwerper: Hans Schweitzer (Mjölnir).\n\nVertaling: Onze laatste hoop: Hitler.",
            "Bron 3 — Duitse verkiezingsposter, 1932.\nOntwerper: Hans Schweitzer (Mjölnir).\n\nVertaling: Nu is het genoeg! Stem op Hitler."
        };

        int huidigeBron = 0;
        Image sourceImage = root.Q<Image>("source-poster");
        Image observationImage = root.Q<Image>("observation-poster");
        Image selectImage = root.Q<Image>("select-poster");
        Label sourceNumber = root.Q<Label>("source-poster-number");
        Label observationNumber = root.Q<Label>("observation-poster-number");
        Label selectNumber = root.Q<Label>("select-poster-number");
        Label sourceDescription = root.Q<Label>("source-description");
        Label observationDescription = root.Q<Label>("observation-poster-caption");
        Label selectDescription = root.Q<Label>("select-poster-caption");

        void ShowSource()
        {
            if (bronnen[huidigeBron] == null)
            {
                Debug.LogWarning("Afbeelding ontbreekt: VraagDuitsland/Bron" + (huidigeBron + 1));
            }
            sourceImage.image = bronnen[huidigeBron];
            observationImage.image = bronnen[huidigeBron];
            selectImage.image = bronnen[huidigeBron];
            string bronnummer = "Bron " + (huidigeBron + 1) + " van 3";
            sourceNumber.text = bronnummer;
            observationNumber.text = bronnummer;
            selectNumber.text = bronnummer;
            sourceDescription.text = beschrijvingen[huidigeBron];
            observationDescription.text = beschrijvingen[huidigeBron];
            selectDescription.text = beschrijvingen[huidigeBron];
        }

        void NextSource()
        {
            huidigeBron++;
            if (huidigeBron > 2)
            {
                huidigeBron = 0;
            }
            ShowSource();
        }

        void PreviousSource()
        {
            huidigeBron--;
            if (huidigeBron < 0)
            {
                huidigeBron = 2;
            }
            ShowSource();
        }

        Button sourceNext = root.Q<Button>("source-poster-next");
        Button sourcePrevious = root.Q<Button>("source-poster-prev");
        Button observationNext = root.Q<Button>("observation-poster-next");
        Button observationPrevious = root.Q<Button>("observation-poster-prev");
        Button selectNext = root.Q<Button>("select-poster-next");
        Button selectPrevious = root.Q<Button>("select-poster-prev");
        sourceNext.clicked += NextSource;
        sourcePrevious.clicked += PreviousSource;
        observationNext.clicked += NextSource;
        observationPrevious.clicked += PreviousSource;
        selectNext.clicked += NextSource;
        selectPrevious.clicked += PreviousSource;
        ShowSource();

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
            observation = "Bron " + (huidigeBron + 1) + ": " + observation;
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
                feedbackText.text = "Je hebt deze vraag succesvol afgerond. Je hebt Duitsland veroverd.\n\nDit is een gesimuleerde beoordeling.";
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
