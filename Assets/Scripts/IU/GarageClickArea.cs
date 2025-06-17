using UnityEngine;
using UnityEngine.EventSystems;

public class GarageClickArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GarageTankController tankController;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject equippedImage;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            equipButton.SetActive(false);
            buyButton.SetActive(false);
            equippedImage.SetActive(false);
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
