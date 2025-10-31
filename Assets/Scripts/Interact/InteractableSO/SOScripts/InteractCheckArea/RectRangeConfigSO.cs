using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "RectRangeConfig", menuName = "交互系统/矩形范围配置")]
public class RectRangeConfigSO : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, HideInInspector]
    private bool _isEditing = false;
#endif
    [Header("矩形范围参数")]
    [SerializeField] private Vector3 _center = Vector3.zero; // 矩形中心
    [SerializeField] private Vector3 _size = new Vector3(4, 3, 1); // 矩形大小（x=宽, y=高, z=厚度）
    [SerializeField] private Vector3 _rotation = Vector3.zero; // 矩形旋转（欧拉角）

    // 公开属性（只读）
    public Vector3 Center => _center;
    public Vector3 Size => _size;
    public Quaternion Rotation => Quaternion.Euler(_rotation);

    // 公开修改方法（解决访问权限问题）
    public void SetCenter(Vector3 newCenter) => _center = newCenter;
    public void SetSize(Vector3 newSize) => _size = newSize;
    public void SetRotation(Vector3 newRotation) => _rotation = newRotation;
    /// <summary>
    /// 返回该点是否处于该区域内
    /// </summary>
    /// <returns></returns>
    public bool IsContains(Vector3 selfPos, Vector3 selfForward, Vector3 targetPos)
    {
        // 1. 构造旋转：严格让范围的正方向（Z轴）对准玩家正前方（selfForward）
        // 确保旋转只绕Y轴（上下方向），避免左右偏航以外的旋转影响正前方
        Quaternion rot = Quaternion.LookRotation(selfForward, Vector3.up);

        // 2. 计算范围的世界中心：玩家位置 + 相对于玩家正前方的局部偏移
        // _center 建议只在Z轴设置值（如(0, 1, 3)表示玩家前方3米、高处1米）
        Vector3 worldCenter = selfPos + rot * _center;

        // 3. 尺寸（size）是范围的物理大小，不需要需要旋转（旋转不改变尺寸）
        Vector3 halfExtents = _size * 0.5f; // 半尺寸（方便边界判断）

        // 4. 将目标点转换到范围的局部空间（以范围中心为原点，Z轴朝前）
        Vector3 localTarget = Quaternion.Inverse(rot) * (targetPos - worldCenter);

        // 5. OBB包含测试：判断目标点是否在范围的局部边界内
        // 由于范围Z轴始终对准玩家前方，这里的判断自然约束在正前方区域
        return Mathf.Abs(localTarget.x) <= halfExtents.x &&
               Mathf.Abs(localTarget.y) <= halfExtents.y &&
               Mathf.Abs(localTarget.z) <= halfExtents.z;
    }
}