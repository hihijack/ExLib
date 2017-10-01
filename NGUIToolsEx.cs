
using UnityEngine;

public static class NGUIToolsEx
{
    /// <summary>
    /// 取UI空间宽/高
    /// </summary>
    /// <returns></returns>
    public static Vector2 GetUISize()
    {
        Vector2 r = Vector2.zero;
        UIRoot root = UIRoot.list[0];
        if (root != null)
        {
            float s = (float)root.activeHeight / Screen.height;
            int height = Mathf.CeilToInt(Screen.height * s);
            int width = Mathf.CeilToInt(Screen.width * s);
            r.x = width;
            r.y = height;
        }
        return r;
    }

    /// <summary>
    /// 获取在UI空间中坐标
    /// </summary>
    /// <param name="tf"></param>
    /// <returns></returns>
    public static Vector2 GetUIPos(Transform tfUICamera, Transform tf)
    {
        Vector3 posL = tfUICamera.worldToLocalMatrix.MultiplyPoint(tf.position);
        return new Vector2(posL.x, posL.y);
    }
}