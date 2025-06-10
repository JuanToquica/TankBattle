using UnityEngine;
using UnityEngine.EventSystems;

public class GarageClickArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GarageTankController tankController;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (tankController != null)
            {
                tankController.SetCanRotateTank(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (tankController != null)
            {
                tankController.SetCanRotateTank(false);
            }
        }
    }
}
