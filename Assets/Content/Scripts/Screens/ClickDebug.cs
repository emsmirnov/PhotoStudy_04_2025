using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDebug : MonoBehaviour, IPointerDownHandler {
    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("Объект кликнут!");
    }
}