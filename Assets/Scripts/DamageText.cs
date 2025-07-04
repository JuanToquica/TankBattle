using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private Color NormalColor;
    [SerializeField] private Color MaxDamageColor;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private float duration;
    [SerializeField] private float speed;
    [SerializeField] private float disolveSpeed;
    [SerializeField] private float scaleMultiplier;
    public int damage;
    private Vector3 initialPosition;

    private void Update()
    {
        canvas.transform.forward = Camera.main.transform.forward;
        float distance = Vector3.Distance(Camera.main.transform.position, text.transform.position);
        float scaleFactor = distance * scaleMultiplier;

        text.transform.localScale = Vector3.one * scaleFactor;
    }

    public void Initialize(int damage)
    {
        this.damage = damage;
        if (damage > 75)
            text.color = MaxDamageColor;
        else
            text.color = NormalColor;
        canvas.alpha = 1;
        text.text = damage.ToString();
        StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        
        float timer = 0;

        while (timer < duration)
        {
            Vector3 targetPosition = transform.position + transform.up * Time.deltaTime * speed;
            transform.position = targetPosition;
            timer += Time.deltaTime;
            yield return null;
        }

        while(canvas.alpha > 0)
        {
            Vector3 targetPosition = transform.position + transform.up * Time.deltaTime * speed;
            transform.position = targetPosition;
            canvas.alpha -= Time.deltaTime * disolveSpeed;
            yield return null;
        }
        canvas.alpha = 0;
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
