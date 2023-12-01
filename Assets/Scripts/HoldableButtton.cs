using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class HoldableButtton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
 
    private bool _buttonPressed;
    private Action _onSpawnButtonHold;
    private Action _onSpawnButtonRelease;

    public void Init(Action onSpawnButtonHold, 
        Action onSpawnButtonRelease)
    {
        _onSpawnButtonHold = onSpawnButtonHold;
        _onSpawnButtonRelease = onSpawnButtonRelease;
    }
 
    public void OnPointerDown(PointerEventData eventData){
        _onSpawnButtonHold?.Invoke();
    }
 
    public void OnPointerUp(PointerEventData eventData){
        _onSpawnButtonRelease?.Invoke();
    }
}