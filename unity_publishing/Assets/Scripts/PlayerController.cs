using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 800f;
    public int health = 5;
    public Text scoreText;
    public Text healthText;
    public Text winLoseText;
    public Image winLoseBG;
    private int score = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            rb.linearDamping = 1.2f;
        }
        SetScoreText();
        SetHealthText();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * speed * Time.deltaTime);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("menu");
        }
        if (health == 0)
        {
            if (winLoseBG != null)
            {
                winLoseBG.gameObject.SetActive(true);
                winLoseBG.color = Color.red;
            }
            if (winLoseText != null)
            {
                winLoseText.text = "Game Over!";
                winLoseText.color = Color.white;
            }
            health = 5;
            score = 0;
            SetScoreText();
            SetHealthText();
            StartCoroutine(LoadScene(3));
        }
    }

    IEnumerator LoadScene(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void SetHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            score++;
            SetScoreText();
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Trap"))
        {
            health--;
            SetHealthText();
        }
        else if (other.CompareTag("Goal"))
        {
            if (winLoseBG != null)
            {
                winLoseBG.gameObject.SetActive(true);
                winLoseBG.color = Color.green;
            }
            if (winLoseText != null)
            {
                winLoseText.text = "You Win!";
                winLoseText.color = Color.black;
            }
            StartCoroutine(LoadScene(3));
        }
    }
}
