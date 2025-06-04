using UnityEngine;

public class FlagController : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            GameManager.instance.OnFlagPickedUp();
        }
    }
}
