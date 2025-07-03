using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class CentreSlider : MonoBehaviour
{
    private Slider _slider;
    // This can be set to a SerializeField if you want to be able to
    // change this in editor
    private readonly float _startValue = 0f;
    private readonly float _centerAnchorX = 0.5f;

    void Awake()
    {
        _slider = GetComponent<Slider>();

        // If the slider isn't null, add the UpdateSlider method as a
        // listener for the slider's onValueChanged event
        if (_slider)
        {
            _slider.onValueChanged.AddListener(UpdateSlider);
        }
    }

    private void Start()
    {
        // Ensures the slider value is set to the center
        _slider.value = _startValue;
        // If the value of the slider is already 0, the onValueChanged
        // event won't be fired, so make sure it does to ensure the fill
        // is centered from the start
        UpdateSlider(_startValue);
    }

    private void OnDisable()
    {
        if (_slider)
        {
            _slider.onValueChanged.RemoveListener(UpdateSlider);
        }
    }

    /// <summary>
    /// Updates the fillRect's anchors based on the position of the handle's anchors, which get adjusted as the
    /// handle is moved. If the handle is in the "negative", the fill is adjusted to stretch from the center to
    /// the position of the handle, and vice versa in the "positive".
    /// </summary>
    void UpdateSlider(float value)
    {
        _slider.fillRect.anchorMin = new Vector2(Mathf.Clamp(_slider.handleRect.anchorMin.x, 0, _centerAnchorX), 0);
        _slider.fillRect.anchorMax = new Vector2(Mathf.Clamp(_slider.handleRect.anchorMin.x, _centerAnchorX, 1), 1);
    }
}