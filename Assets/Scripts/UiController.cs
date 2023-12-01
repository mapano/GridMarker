using UnityEngine;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Button clearButton;
    [SerializeField]
    private HoldableButtton spawnButton;
    [SerializeField]
    private Slider cameraZoomSlider;

    private bool _isHolding = false;

    private Action _onClearButtonClick;
    private Action<float> _onSliderChange;

    public void Init(Action onClearButtonClick, 
                     Action onSpawnButtonHold, 
                     Action onSpawnButtonRelease, 
                     Action<float> onSliderChange)
    {
        _onClearButtonClick = onClearButtonClick;
        _onSliderChange = onSliderChange;

        clearButton.onClick.AddListener(OnClick);
        cameraZoomSlider.onValueChanged.AddListener(OnSliderValueChanged);
        spawnButton.Init(onSpawnButtonHold , onSpawnButtonRelease);
    }

    private void OnClick()
    {
        _onClearButtonClick?.Invoke(); 
    }

    private void OnSliderValueChanged(float value)
    {
        _onSliderChange?.Invoke(value); 
    }
}
