using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiTextManager : MonoBehaviour
{
    [Header("Text Refference")]
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI nbsCitizenText;
    [SerializeField] private Image faithSliderImage;

    private void OnEnable()
    {
        EventBus.Subscribe<int>(EventType.UPDATE_UI_FoodText, UpdateFoodText);
        EventBus.Subscribe<int>(EventType.UPDATE_UI_WoodText, UpdateWoodText);
        EventBus.Subscribe<int>(EventType.UPDATE_UI_NbsCitizen, UpdateNbsCitizenText);
        EventBus.Subscribe<float>(EventType.UPDATE_UI_FaithFill, UpdateFaithSliderBar);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<int>(EventType.UPDATE_UI_FoodText, UpdateFoodText);
        EventBus.Unsubscribe<int>(EventType.UPDATE_UI_WoodText, UpdateWoodText);
        EventBus.Unsubscribe<int>(EventType.UPDATE_UI_NbsCitizen, UpdateNbsCitizenText);
        EventBus.Unsubscribe<float>(EventType.UPDATE_UI_FaithFill, UpdateFaithSliderBar);
    }

    private void UpdateFoodText(int value)
    {
        foodText.text = value.ToString();
    }
    private void UpdateWoodText(int value)
    {
        woodText.text = value.ToString();
    }
    private void UpdateNbsCitizenText(int value)
    {
        nbsCitizenText.text = value.ToString();
    }

    private void UpdateFaithSliderBar(float value)
    {
        faithSliderImage.fillAmount = value;
    }
}
