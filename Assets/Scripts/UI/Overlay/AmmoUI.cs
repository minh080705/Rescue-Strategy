// Assets/Scripts/UI/AmmoUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    public GunWeapon gunWeapon;

    [Header("Bullet Icons")]
    public Image[] bulletIcons;
    public Color colorFull  = Color.white;
    public Color colorEmpty = new Color(1f, 1f, 1f, 0f);

    [Header("Text (tuỳ chọn)")]
    public Text ammoText;

    void OnEnable()
    {
        if (gunWeapon == null) return;
        gunWeapon.onAmmoChanged.AddListener(OnAmmoChanged);
        gunWeapon.onReloadEnd.AddListener(OnReloadEnd);
        OnAmmoChanged(gunWeapon.CurrentAmmo, gunWeapon.maxAmmo);
    }

    void OnDisable()
    {
        if (gunWeapon == null) return;
        gunWeapon.onAmmoChanged.RemoveListener(OnAmmoChanged);
        gunWeapon.onReloadEnd.RemoveListener(OnReloadEnd);
    }

    void OnAmmoChanged(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";

        float pct = (float)current / max;

        for (int i = 0; i < bulletIcons.Length; i++)
        {
            if (bulletIcons[i] == null) continue;
            float threshold = (float)i / bulletIcons.Length;
            bulletIcons[i].color = pct > threshold ? colorFull : colorEmpty;
        }
    }

    void OnReloadEnd()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateBulletsRefill());
    }

    IEnumerator AnimateBulletsRefill()
    {
        float delayPerIcon = gunWeapon.reloadDuration / bulletIcons.Length;

        for (int i = 0; i < bulletIcons.Length; i++)
        {
            if (bulletIcons[i] != null)
                bulletIcons[i].color = colorFull;
            yield return new WaitForSeconds(delayPerIcon);
        }
    }
}