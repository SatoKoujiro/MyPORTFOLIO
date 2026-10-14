using UnityEngine;

public sealed class PlatformerCameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 velocity;

    public void Initialize(Camera gameCamera, Transform followTarget)
    {
        target = followTarget;
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 6.2f;
        gameCamera.backgroundColor = new Color(0.72f, 0.84f, 0.91f);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.transform.position = new Vector3(0f, 2f, -10f);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 destination = new Vector3(Mathf.Max(0f, target.position.x + 2.4f), 2.1f, -10f);
        transform.position = Vector3.SmoothDamp(
            transform.position, destination, ref velocity, 0.18f, Mathf.Infinity, Time.deltaTime);
    }
}
