using UnityEngine;

[DefaultExecutionOrder(1000)]
public class UI_MiniHealthBar : MonoBehaviour
{
    private Transform cachedTransform;
    private Transform followTarget;
    private Vector3 worldOffset;
    private Camera mainCamera;

    private void Awake()
    {
        cachedTransform = transform;
        followTarget = cachedTransform.parent;
        mainCamera = Camera.main;

        // 记录初始世界偏移，后续只按世界坐标跟随目标。
        if (followTarget != null)
            worldOffset = cachedTransform.position - followTarget.position;

        // 脱离会左右翻转的父层级，避免 Slider Fill 受翻转矩阵影响出现抖动。
        cachedTransform.SetParent(null, true);
        cachedTransform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            // 目标不存在时主动销毁，避免场景里留下孤立血条对象。
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = followTarget.position + worldOffset;

        // 按像素网格对齐位置，消除世界空间 UI 边缘的子像素抖动。
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && mainCamera.orthographic && Screen.height > 0)
        {
            float worldPixelSize = (mainCamera.orthographicSize * 2f) / Screen.height;
            if (worldPixelSize > 0f)
            {
                targetPosition.x = Mathf.Round(targetPosition.x / worldPixelSize) * worldPixelSize;
                targetPosition.y = Mathf.Round(targetPosition.y / worldPixelSize) * worldPixelSize;
            }
        }

        // 只跟随位置，不继承旋转，保证小血条填充方向稳定。
        cachedTransform.position = targetPosition;
    }
}

