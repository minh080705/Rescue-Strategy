using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    public HealthComponent health;
    public Image fillImage;

    void Start()
    {
        if (health != null)
        {
            health.onHPChanged.AddListener(UpdateBar);

            UpdateBar(
                health.CurrentHP,
                health.MaxHP
            );
        }
    }

    void UpdateBar(int currentHP, int maxHP)
    {
        fillImage.fillAmount =
            (float)currentHP / maxHP;
    }
}