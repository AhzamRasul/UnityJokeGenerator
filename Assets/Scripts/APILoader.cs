using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class APILoader : MonoBehaviour
{
    // UI elements
    public TextMeshProUGUI setupText;
    public TextMeshProUGUI punchlineText;
    public Image progressBarFill;
    public TextMeshProUGUI loadingTimeText;
    public Button refreshButton;
    
    public TextMeshProUGUI InfoText; // Add a reference to the info text

    // Time tracking variables
    private float startTime;

    // API endpoint
    private const string apiUrl = "https://official-joke-api.appspot.com/random_joke";

    // Constants for timing and delays
    private const float timeLimit = 2.0f;
    private const float displayDelay = 0.5f;

    // Track the current running coroutine
    private Coroutine currentCoroutine;

    // Flag to track whether the button has been clicked
    private bool buttonClicked = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Attach button click event
        refreshButton.onClick.AddListener(OnRefreshButtonClick);
    }

    // Called when the refresh button is clicked
    public void OnRefreshButtonClick()
    {
        // Stop the current coroutine if it's running
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Set the flag to true on the first button click
        if (!buttonClicked)
        {
            buttonClicked = true;

            // Disable the usage info text
            if (InfoText != null)
            {
                InfoText.gameObject.SetActive(false);
            }
        }

        // Trigger the API call when the button is pressed
        currentCoroutine = StartCoroutine(GetJoke());
    }

    // Coroutine for fetching a joke from the API
    private IEnumerator GetJoke()
    {
        // Record the start time for tracking progress
        startTime = Time.time;

        // Set the initial color and transparency of the progress bar
        progressBarFill.color = new Color(0, 1, 0, 0);

        // Reset UI text elements
        setupText.text = punchlineText.text = loadingTimeText.text = "";

        // Use UnityWebRequest to make an asynchronous API request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            // Set a timeout for the API request
            webRequest.timeout = 10;

            // Send the API request asynchronously
            var operation = webRequest.SendWebRequest();

            // Update the progress bar while the API request is in progress
            while (!operation.isDone)
            {
                // Calculate progress based on time
                float progress = Mathf.Clamp01((Time.time - startTime) / timeLimit);

                // Update the fill image color and transparency based on time
                progressBarFill.color = new Color(0, 1, 0, progress);
                yield return null;
            }

            // Calculate the duration of the API request
            float loadingTime = Time.time - startTime;

            // Display the loading time on the UI
            loadingTimeText.text = "Loading Time: " + loadingTime.ToString("F2") + " seconds";

            // Check if the API request was successful
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Introduce a delay before displaying the new joke
                yield return new WaitForSeconds(displayDelay);

                // Display the fetched joke
                DisplayJoke(webRequest.downloadHandler.text);
            }
            else
            {
                // Log an error message if the API request fails
                Debug.LogError("Error: " + webRequest.error);

                // Display an error message on the UI
                loadingTimeText.text = "Error Loading API: " + webRequest.error;
            }

            // Set the fill image color and transparency to indicate completion
            progressBarFill.color = new Color(0, 1, 0, 0);

            // Reset the current coroutine reference to null when it completes
            currentCoroutine = null;
        }
    }

    // Display the fetched joke on the UI
    private void DisplayJoke(string jsonData)
    {
        // Parse the JSON data into a Joke object
        Joke joke = JsonUtility.FromJson<Joke>(jsonData);

        // Log the joke data for debugging
        Debug.Log($"Setup: {joke.setup}\nPunchline: {joke.punchline}");

        // Update the UI Text elements with the joke data
        setupText.text = "Setup: " + joke.setup;
        punchlineText.text = "Punchline: " + joke.punchline;
    }
}

// Data structure for storing joke information
[System.Serializable]
public class Joke
{
    public string id;
    public string type;
    public string setup;
    public string punchline;
}
