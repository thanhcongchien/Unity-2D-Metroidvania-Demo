using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Camera targetCamera;

    [Header("Parallax")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float parallaxX = 0.5f;
    [SerializeField] private float parallaxY = 0.2f;

    [Header("Camera Lock")]
    [SerializeField] private bool lockXToCamera;
    [SerializeField] private bool lockYToCamera;

    [Header("Coverage")]
    [SerializeField] private bool autoFitToCamera;
    [SerializeField] private bool useTiledSprite = true;
    [SerializeField] private Vector2 extraCoverage = new Vector2(2f, 2f);

    private Transform tr;
    private SpriteRenderer spriteRenderer;
    private Vector3 startWorldPos;
    private Vector3 startCamPos;
    private Vector3 cameraOffset;
    private float lastCamOrthoSize = -1f;
    private float lastCamAspect = -1f;

    private void Awake()
    {
        tr = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ResolveCamera();

        startWorldPos = tr.position;
        startCamPos = cam != null ? cam.position : Vector3.zero;
        cameraOffset = tr.position - startCamPos;

        ApplyCoverage(true);
    }

    private void LateUpdate()
    {
        ResolveCamera();

        if (cam == null)
            return;

        Vector3 camOffset = cam.position - startCamPos;

        float targetX = tr.position.x;
        float targetY = tr.position.y;

        if (lockXToCamera)
            targetX = cam.position.x + cameraOffset.x;
        else if (followX)
            targetX = startWorldPos.x + camOffset.x * parallaxX;

        if (lockYToCamera)
            targetY = cam.position.y + cameraOffset.y;
        else if (followY)
            targetY = startWorldPos.y + camOffset.y * parallaxY;

        tr.position = new Vector3(targetX, targetY, startWorldPos.z);
        ApplyCoverage(false);
    }

    private void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = cam != null ? cam.GetComponent<Camera>() : Camera.main;

        if (targetCamera != null)
            cam = targetCamera.transform;
    }

    private void ApplyCoverage(bool force)
    {
        if (autoFitToCamera == false || targetCamera == null || targetCamera.orthographic == false || spriteRenderer == null)
            return;

        if (force == false)
        {
            bool sizeChanged = Mathf.Abs(lastCamOrthoSize - targetCamera.orthographicSize) > 0.001f;
            bool aspectChanged = Mathf.Abs(lastCamAspect - targetCamera.aspect) > 0.001f;
            if (sizeChanged == false && aspectChanged == false)
                return;
        }

        float height = targetCamera.orthographicSize * 2f + extraCoverage.y;
        float width = height * targetCamera.aspect + extraCoverage.x;

        if (useTiledSprite)
        {
            if (spriteRenderer.drawMode != SpriteDrawMode.Tiled)
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;

            spriteRenderer.size = new Vector2(width, height);
        }
        else
        {
            if (spriteRenderer.sprite != null)
            {
                float spriteWidth = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
                float spriteHeight = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;

                if (spriteWidth > 0f && spriteHeight > 0f)
                {
                    Vector3 scale = tr.localScale;
                    scale.x = width / spriteWidth;
                    scale.y = height / spriteHeight;
                    tr.localScale = scale;
                }
            }
        }

        lastCamOrthoSize = targetCamera.orthographicSize;
        lastCamAspect = targetCamera.aspect;
    }
}
