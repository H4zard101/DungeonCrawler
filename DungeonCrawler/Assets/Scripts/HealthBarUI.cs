using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Header("UI Refrences")]
    public Image fillImage;
    public TMP_Text healthText;

    [Header("Animation")]
    public float smoothSpeed = 10f;
    private float targetFill = 1f;
    private float currentFill = 1f;

    private void Start()
    {
        if(playerHealth == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null)
            {
                playerHealth = p.GetComponent<PlayerHealth>();
            }
        }
        if(playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(OnHealthChanged);

            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    public void Update()
    {
        if(fillImage != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, smoothSpeed * Time.deltaTime);
            fillImage.fillAmount = currentFill;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if(max <= 0)
        {
            max = 1;
        }
        targetFill = (float)current / max;

        if(healthText != null)
        {
            healthText.text = $"HP: {current} / {max}";
        }
           
    }
}
