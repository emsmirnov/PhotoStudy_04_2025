using UnityEngine;
using Lean.Touch;

[RequireComponent(typeof(RectTransform))]
public class BoundedLeanPinch : LeanPinchScale
{
    public Vector2 MinScale;// = Vector2.one * 0.5f;
    public Vector2 MaxScale;// = Vector2.one * 2f;
    private RectTransform parentRect;

    protected override void Awake()
    {
        base.Awake();
        parentRect = transform.parent.GetComponent<RectTransform>();
    }

    protected override void Update()
    {
        // Сохраняем текущий масштаб
        Vector3 oldScale = transform.localScale;

        // Пробуем применить масштабирование
        base.Update();

        // Применяем ограничения
        if (parentRect != null)
        {
            transform.localScale = GetClampedScale(oldScale);
        }
    }

    private Vector3 GetClampedScale(Vector3 baseScale)
    {
        Vector3 newScale = transform.localScale;

        // Ограничение по минимальному/максимальному масштабу
        newScale.x = Mathf.Clamp(newScale.x, MinScale.x, MaxScale.x);
        newScale.y = Mathf.Clamp(newScale.y, MinScale.y, MaxScale.y);

        // Дополнительная проверка границ родителя
        // if (CheckBounds(newScale))
        // {
            return newScale;
        // }
        // return baseScale;
    }

    private bool CheckBounds(Vector3 testScale)
    {
        Vector3[] parentCorners = new Vector3[4];
        parentRect.GetWorldCorners(parentCorners);

        Vector3[] selfCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(selfCorners);

        // Рассчитываем новые границы после масштабирования
        Vector3 center = (selfCorners[0] + selfCorners[2]) * 0.5f;
        for (int i = 0; i < 4; i++)
        {
            selfCorners[i] = center + (selfCorners[i] - center).normalized * 
                           Vector3.Distance(selfCorners[i], center) * testScale.x;
        }

        // Проверяем вхождение
        return selfCorners[0].x >= parentCorners[0].x &&
               selfCorners[2].x <= parentCorners[2].x &&
               selfCorners[0].y >= parentCorners[0].y &&
               selfCorners[2].y <= parentCorners[2].y;
    }
}