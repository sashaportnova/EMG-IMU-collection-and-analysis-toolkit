using UnityEngine;
using UnityEngine.EventSystems;

public class ChartHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Canvas canvas;
    private int originalSortingOrder;

    private float scaleMultiplier = 2.5f;
    private float transitionSpeed = 10f;
    private int hoverSortingOrder = 100;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            originalSortingOrder = canvas.sortingOrder;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;

        if (canvas != null)
        {
            canvas.sortingOrder = hoverSortingOrder;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;

        if (canvas != null)
        {
            canvas.sortingOrder = originalSortingOrder;
        }
    }
}