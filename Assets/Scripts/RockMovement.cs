/* Script that handles character */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RockMovement : MonoBehaviour {
    // Class data
    public float speed = 5f; 
    public TextMeshProUGUI gameOver; 
    public PulpitManager pulpitManager; 
    public float time = 1f; 
    private CharacterController characterController;
    private GameObject curr, prev; 
    public bool IsOnPulpit { get; private set; } = true; 
    private float outsidePulpit = 0f; 

    // Function at the start of the game
    private void Start() {
        characterController = GetComponent<CharacterController>();
        gameOver.gameObject.SetActive(false); 
    }

    // Update state of character
    private void Update() {
        if (! IsOnPulpit) {
            outsidePulpit += Time.deltaTime;

            if (outsidePulpit >= time) {
                EndGame();
                return;
            }
        }

        // Coordinates
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime;

        // Move character
        if (characterController != null) 
            characterController.Move(movement);
    }

    // Function on entering pulpit
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Pulpit")) {
            IsOnPulpit = true;
            outsidePulpit = 0f; 
            prev = curr; 
            curr = other.gameObject; 
        }
    }

    // Function on leaving pulpit
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Pulpit") && other.gameObject == curr) {
            curr = null;
            IsOnPulpit = false; 
        }
    }

    // Function at the end of game
    private void EndGame() {
        // Display game over text
        gameOver.gameObject.SetActive(true);

        // Rock must be deactivated
        gameObject.SetActive(false);

        pulpitManager.StopAllCoroutines();
    }
}
