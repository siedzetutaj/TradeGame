using UnityEngine.UI;

public class TradeBar : MonoBehaviourSingleton<TradeBar>
{
    public Slider slider;
    private float _value = 0;


    void Start()
    {

        slider.minValue = -100;
        slider.maxValue = 100;
        slider.value = 0;  // Start from center
    }

    public void OnItemSellBuy(int direction, float itemValue)
    {
        _value += direction * itemValue;

        slider.value += _value;


    }

    //gdzies jest button który wyci¹ga z t¹d can buy
    public bool CanBuy()
    {
        if (_value >= 0) 
        {
            return true;
        }
        return false;
    }
}
