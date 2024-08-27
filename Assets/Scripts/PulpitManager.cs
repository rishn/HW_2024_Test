/* Script that handles screens and pulpits */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PulpitManager : MonoBehaviour {
    /* Classes for fetching JSON */
    [System.Serializable]
    public class PulpitData {
        public PulpitDataValues pulpit_data;
    }

    [System.Serializable]
    public class PulpitDataValues {
        public float min_pulpit_destroy_time;
        public float max_pulpit_destroy_time;
        public float pulpit_spawn_time;
    }

    /* UI elements */
    public GameObject pulpitPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdown;
    public TextMeshProUGUI finalScore;
    public TextMeshProUGUI title;
    public TextMeshProUGUI gameOver; 
    public GameObject rock;
    public Button restartButton;
    public Button startButton; 
    public GameObject startPanel; 
    public AudioSource sound; 

    // Score tracker
    private int score = 0;

    // List of pulpits in the game
    private List<GameObject> activePulpits = new List<GameObject>();

    // Length of pulpit
    private float pulpitLen = 100f;

    // RockMovement object
    private RockMovement rockMovement;

    // Flag for game start
    private bool gameFlag = false;

    // Default values
    private float minPulpitLifetime = 3f;
    private float maxPulpitLifetime = 5f;
    private float spawnTime = 2.5f;

    // JSON URL
    private const string url = "https://s3.ap-south-1.amazonaws.com/superstars.assetbundles.testbuild/doofus_game/doofus_diary.json";

    // Initial function
    private void Start() {
        // Check whether UI components exist
        if (title == null || startPanel == null || restartButton == null || finalScore == null || countdown == null || gameOver == null) {
            Debug.LogError("UI components are not assigned in the Inspector");
            return;
        }

        // Initial configurations
        title.text = "Doofus! Adventure Game";
        startPanel.SetActive(true);
        restartButton.gameObject.SetActive(false);
        countdown.gameObject.SetActive(false);
        finalScore.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);
        sound.loop = false;

        // Connect buttons to their respective functions
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(ReStartGame);
    }

    // Function to read JSON data
    private IEnumerator LoadJsonData() {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            string jsonString = www.downloadHandler.text;
            PulpitData pulpitData = JsonUtility.FromJson<PulpitData>(jsonString);
            minPulpitLifetime = pulpitData.pulpit_data.min_pulpit_destroy_time;
            maxPulpitLifetime = pulpitData.pulpit_data.max_pulpit_destroy_time;
            spawnTime = pulpitData.pulpit_data.pulpit_spawn_time;
            
            countdown.gameObject.SetActive(true);
            rockMovement = rock.GetComponent<RockMovement>();
            StartCoroutine(SpawnPulpits());
        }
        else 
            Debug.LogError("Failed to load JSON data: " + www.error);
    }

    // Function that starts the game
    private void StartGame() {
        // Update score
        score = 0;
        UpdateScore();

        // Remove start button and start screen
        startButton.gameObject.SetActive(false);
        startPanel.SetActive(false); 

        // Display character
        rock.SetActive(true);

        // Play sound
        if (sound != null) {
            sound.loop = true;
            sound.Play();
        }

        // Read JSON
        StartCoroutine(LoadJsonData());
    }

    // Function that restarts the game
    private void ReStartGame() {
        // Update score
        score = 0;
        UpdateScore();

        // Remove start panel components
        startPanel.SetActive(false);
        gameOver.gameObject.SetActive(false);
        finalScore.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        // Display character
        rock.SetActive(true);

        // Play sound
        if (sound != null) {
            sound.loop = true;
            sound.Play();
        }

        // Read JSON
        StartCoroutine(LoadJsonData());
    }
    
    // Function that checks if given position is already occupied by a pulpit
    private bool IsPositionOccupied(Vector3 position) {
        foreach (GameObject pulpit in activePulpits)
            if (Vector3.Distance(pulpit.transform.position, position) < pulpitLen)
                return true;

        return false;
    }

    // Function to randomly rearrange a vector array
    private void Rearrange(Vector3[] array) {
        for (int i = array.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            Vector3 temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    // Function that finds suitable coordinates for new pulpit
    private Vector3 GetNewPulpitPosition() {
        // If at start of game, pulpit must appear at rock's position
        if (activePulpits.Count == 0)
            return rock.transform.position;

        // Find recently added pulpit's position
        Vector3 recentPulpitPos = activePulpits[activePulpits.Count - 1].transform.position;

        // Add coordinates of surrounding
        Vector3[] potPos = new Vector3[] {
            new Vector3(recentPulpitPos.x - pulpitLen, 0, recentPulpitPos.z),
            new Vector3(recentPulpitPos.x + pulpitLen, 0, recentPulpitPos.z),
            new Vector3(recentPulpitPos.x, 0, recentPulpitPos.z - pulpitLen),
            new Vector3(recentPulpitPos.x, 0, recentPulpitPos.z + pulpitLen)
        };

        // Random rearrangement
        Rearrange(potPos);

        // Return position not occupied
        foreach (Vector3 position in potPos)
            if (! IsPositionOccupied(position))
                return position;

        return Vector3.zero;
    }

    // Function that checks whether rock is outside of pulpit
    private bool IsRockWithinRangeOfPulpits() {
        foreach (GameObject pulpit in activePulpits) 
            if (Vector3.Distance(rock.transform.position, pulpit.transform.position) < pulpitLen) 
                return true;

        return false;
    }

    // Function to create pulpits
    private IEnumerator SpawnPulpits() {
        while (true) {
            yield return new WaitForSeconds(spawnTime);

            // If there is only single pulpit, and spawn time has crossed,
            if (activePulpits.Count < 2) {
                Vector3 spawn = GetNewPulpitPosition();

                // If position is not origin,
                if (spawn != Vector3.zero)
                {
                    GameObject newPulpit = Instantiate(pulpitPrefab, spawn, Quaternion.identity);
                    activePulpits.Add(newPulpit);

                    float pulpitLife = Random.Range(minPulpitLifetime, maxPulpitLifetime);
                    StartCoroutine(PulpitLifetime(newPulpit, pulpitLife));

                    score++;

                    if (activePulpits.Count > 1)
                        gameFlag = true;

                    yield return new WaitForSeconds(1f);
                    DestroyAllPulpits(newPulpit);

                    if (gameFlag) {
                        yield return new WaitForSeconds(0.5f);

                        if (!IsRockWithinRangeOfPulpits()) {
                            EndGame();
                            yield break;
                        }
                    }
                }
            }
        }
    }
    
    // Function that counts down pulpit's life
    private IEnumerator PulpitLifetime(GameObject pulpit, float pulpitLife) {
        countdown.gameObject.SetActive(true);
        float elapsedTime = 0f;

        while (elapsedTime < pulpitLife) {
            elapsedTime += Time.deltaTime;
            float timeLeft = pulpitLife - elapsedTime;

            int seconds = Mathf.FloorToInt(timeLeft);
            int millis = Mathf.FloorToInt((timeLeft - seconds) * 1000);
            countdown.text = $"{seconds}:{millis:D2}";

            if (seconds == 3)
                UpdateScore();

            yield return null;
        }

        countdown.gameObject.SetActive(false);
        activePulpits.Remove(pulpit);
        Destroy(pulpit);
    }

    // Function that destroys all pulpits except the current pulpit
    private void DestroyAllPulpits(GameObject currPulpit) {
        foreach (GameObject pulpit in activePulpits.ToArray())
            if (pulpit != currPulpit)
                Destroy(pulpit);
        
        activePulpits.Clear();
        activePulpits.Add(currPulpit);
    }

    // Function to update score
    private void UpdateScore() {
        scoreText.text = "Score: " + score;
    }

    // Function that runs at the end of the game
    private void EndGame() {
        // Stop sound playback
        if (sound != null)
            sound.Stop();

        // Show start panel again
        startPanel.SetActive(true);

        // Display appropriate texts
        gameOver.gameObject.SetActive(true);
        gameOver.text = "Game Over!";

        finalScore.text = "Final Score: " + score;
        finalScore.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        // Remove gameplay components
        rock.SetActive(false);
        countdown.gameObject.SetActive(false);
    }
}
