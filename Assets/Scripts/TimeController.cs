using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeController : MonoBehaviour
{
    public TextMeshProUGUI timeScaleText; // UI Text to display time scale
    public TextMeshProUGUI dateText; // UI Text to display date (Year-Quarter)
    public Button increaseButton; // UI Button to increase time scale
    public Button decreaseButton; // UI Button to decrease time scale
    public Image pauseImage;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite playIcon;

    private float[] timeScales = { 0.25f, 0.5f, 1f, 2f, 3f, 4f, 5f, 10f };
    private int currentScaleIndex = 2; // Start at normal speed (1x)

    private int year = 1;
    private int quarter = 1;
    private float quarterTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1; // Start the game paused
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdateDate();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            IncreaseTimeScale();
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            DecreaseTimeScale();
        }
    }

    private void UpdateDate()
    {
        if (Time.timeScale > 0)
        {
            quarterTimer += Time.deltaTime;

            if (quarterTimer >= 0.25f)
            {
                quarterTimer = 0;
                quarter++;

                if (quarter > 4)
                {
                    quarter = 1;
                    year++;
                }

                UpdateUI();
            }
        }
    }

    public void IncreaseTimeScale()
    {
        if (currentScaleIndex < timeScales.Length - 1)
        {
            currentScaleIndex++;
            if (Time.timeScale > 0)
                Time.timeScale = timeScales[currentScaleIndex];
            UpdateUI();
        }
    }

    public void DecreaseTimeScale()
    {
        if (currentScaleIndex > 0)
        {
            currentScaleIndex--;
            if (Time.timeScale > 0)
                Time.timeScale = timeScales[currentScaleIndex];
            UpdateUI();
        }
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = timeScales[currentScaleIndex];
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        pauseImage.sprite = Time.timeScale == 0 ? playIcon : pauseIcon;

        timeScaleText.text = $"{timeScales[currentScaleIndex]}x";
        dateText.text = $"UT {year}-Q{quarter}";
    }
}
