using UnityEngine;
using Lean.Touch;

[RequireComponent(typeof(RectTransform))]
public class BoundedLeanDrag : LeanDragTranslate
{
    private RectTransform parentRect;
    private RectTransform selfRect;

    public float sensitivity = 1.1f;

    protected override void Awake()
    {
        base.Awake();
        selfRect = GetComponent<RectTransform>();
        parentRect = transform.parent.GetComponent<RectTransform>();
    }

    protected override void Update()
    {
        // Базовый функционал перемещения
        base.Update();

        // Применяем ограничения после перемещения
        // if (parentRect != null)
        // {
        //     ClampPosition();
        // }
    }

    void LateUpdate()
    {
        // Применяем ограничения после перемещения
        if (parentRect != null)
        {
            ClampPosition();
        }
    }

    private void ClampPosition()
    {
        // selfRect = GetComponent<RectTransform>();
        // parentRect = transform.parent.GetComponent<RectTransform>();
        // Получаем границы в локальных координатах
        Bounds parentBounds = GetBounds(parentRect);
        Bounds selfBounds = GetBounds(selfRect);

        // Рассчитываем допустимые смещения
        Vector3 localPos = transform.localPosition;
        localPos.x = Mathf.Clamp(
            localPos.x,
            parentBounds.min.x - selfBounds.min.x,
            parentBounds.max.x - selfBounds.max.x
        );
        localPos.y = Mathf.Clamp(
            localPos.y,
            parentBounds.min.y - selfBounds.min.y,
            parentBounds.max.y - selfBounds.max.y
        );
        transform.localPosition = localPos * sensitivity;
        print("locat transform::" + transform.localPosition);
    }

    private Bounds GetBounds(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetLocalCorners(corners);

        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; i++)
        {
            bounds.Encapsulate(corners[i]);
        }
        return bounds;
    }
}