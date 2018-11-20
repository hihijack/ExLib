 /// <summary>
    /// 计算正交相机视口范围
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Bounds GetCamOrthViewRange(Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            camera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }