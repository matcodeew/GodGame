using GodGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] private PowerType powerType = PowerType.None;
    [SerializeField] private float powerDuration = 5f;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Power UI")]
    [SerializeField] private GameObject ScreenBordureObject;
    private Image ScreenBordureImage;
    [SerializeField] private Sprite IceBordure;
    [SerializeField] private Sprite SickBordure;

    private void Awake()
    {
        ScreenBordureImage = ScreenBordureObject.GetComponent<Image>();
        ScreenBordureObject.SetActive(false);
    }

    public void UsePower(string powerName)
    {
        if (powerName == PowerType.Sickness.ToString())
            powerType = PowerType.Sickness;
        else if (powerName == PowerType.IceTornado.ToString())
            powerType = PowerType.IceTornado;
        else
            return;

        ApplyPowerToAllVillagers();
    }

    private void ApplyPowerToAllVillagers()
    {
        List<Villager> villagers = new();
        if (GameManager.Instance.GetAllCityPos().Count > 0)
        {
            villagers = GameManager.Instance.GetCityByPos(GameManager.Instance.GetAllCityPos()[0]).AllCitizen;
        }



        switch (powerType)
        {
            case PowerType.IceTornado:
                StartCoroutine(ScreenEffect(IceBordure, powerDuration, 0.5f));
                foreach (var v in villagers)
                    v.ApplyFreeze(powerDuration);
                break;

            case PowerType.Sickness:
                StartCoroutine(ScreenEffect(SickBordure, powerDuration, 1f));
                foreach (var v in villagers)
                    v.ApplySickness(powerDuration);
                break;
        }

        // reset après application
        powerType = PowerType.None;
    }

    private IEnumerator ScreenEffect(Sprite border, float duration, float targetAlpha)
    {
        ScreenBordureImage.sprite = border;
        ScreenBordureObject.SetActive(true);

        // fondu d'apparition
        yield return StartCoroutine(FadeBorder(targetAlpha, fadeDuration / 2));

        // attend la durée du pouvoir
        yield return new WaitForSeconds(duration);

        // fondu de disparition
        yield return StartCoroutine(FadeBorder(0f, fadeDuration / 2));

        ScreenBordureObject.SetActive(false);
    }

    private IEnumerator FadeBorder(float targetAlpha, float fadeDuration)
    {
        Color c = ScreenBordureImage.color;
        float startAlpha = c.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            ScreenBordureImage.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        ScreenBordureImage.color = c;
    }
}

public enum PowerType
{
    None,
    IceTornado,
    Sickness,
}
