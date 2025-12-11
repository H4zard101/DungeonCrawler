using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Enemy enemy;
    public Image fillImage;
    public float lerpSpeed = 10f;
    public Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        if (enemy == null)
        {
            enemy = GetComponentInParent<Enemy>();
        }
    }

    private void Update()
    {
        if (enemy == null)
            return;

        // Update fill amount smoothly
        float target = enemy.HealthPercent;
        if (fillImage != null)
        {
            float current = fillImage.fillAmount;
            float newFill = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);
            fillImage.fillAmount = newFill;
        }

        // Keep the bar positioned above the enemy in world space
        if (enemy != null)
        {
            transform.position = enemy.transform.position + worldOffset;
        }

        // If you want the bar to always face the camera (2.5D / 3D)
        // but in top-down 2D orthographic you can usually skip this
        if (mainCam != null)
        {
            // For pure 2D top-down you usually don't need rotation
            // but if it looks off, you can uncomment this:
            // transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
    }
}