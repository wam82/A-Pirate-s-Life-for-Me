using UnityEngine;

public static class DebugUtils
{
    public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1f)
    {
        up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        Matrix4x4 matrix = new Matrix4x4();

        matrix[0] = right.x;
        matrix[1] = right.y;
        matrix[2] = right.z;

        matrix[4] = up.x;
        matrix[5] = up.y;
        matrix[6] = up.z;

        matrix[8] = forward.x;
        matrix[9] = forward.y;
        matrix[10] = forward.z;

        Vector3 lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        Vector3 nextPoint = Vector3.zero;

        for (var i = 0; i < 91; i++)
        {
            nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
            nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
            nextPoint.y = 0;

            nextPoint = position + matrix.MultiplyPoint3x4(nextPoint);

            Debug.DrawLine(lastPoint, nextPoint, color);
            lastPoint = nextPoint;
        }
    }
}