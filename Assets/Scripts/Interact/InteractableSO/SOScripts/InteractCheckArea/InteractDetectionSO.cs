using System;
using UnityEngine;

namespace InteractSystem
{
    public abstract class InteractDetectionSO : ScriptableObject
    {
        [Header("交互检测相关")]

        [Tooltip("交互检测层级")]
        [SerializeField]
        protected LayerMask layerMask;

        [Tooltip("交互检测时间间隔,单位：ms")]
        public int checkOffset;                     //交互检测时间间隔,单位：ms

        [Tooltip("是否检测触发器")]
        [SerializeField]
        protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide; // 是否检测触发器

        [Tooltip("本地相对于检测者的位置")]
        [SerializeField]
        protected Vector3 localPos;
        // 预先一次性申请缓冲区
        protected Collider[] _buffer = new Collider[64];   // 64 足够大即可

        protected virtual (Vector3,Quaternion) GetTurePos(Vector3 center, Vector3 dir)
        {
            // 提取 dir 的水平方向（忽略 Y 轴，确保在同一水平面）
            Vector3 horizontalDir = new Vector3(dir.x, 0, dir.z).normalized;
            // 避免 dir 垂直向上/下时水平方向为零向量（防止 LookRotation 报错）
            if (horizontalDir.sqrMagnitude < 0.01f)
            {
                horizontalDir = Vector3.forward; // 默认向前
            }

            Vector3 worldOffset = Quaternion.LookRotation(horizontalDir, Vector3.up) *
                      new Vector3(0, localPos.y, localPos.z);

            Vector3 truePos = center + worldOffset;   // 中心就是玩家
            Debug.DrawLine(new Vector3(center.x,localPos.y,center.z),truePos,Color.green);
            return (truePos, Quaternion.LookRotation(horizontalDir, Vector3.up));
        }
        // 抽象方法：执行检测，返回范围内的碰撞体
        // center：检测区域的中心位置（通常是玩家位置）
        // rotation：检测区域的旋转（对盒形、胶囊形有效）
        public abstract ReadOnlySpan<Collider> Detect(Vector3 center, Vector3 dir);

        // 可选：Gizmos绘制，在Scene视图中可视化检测区域
        public abstract void DrawGizmos(Vector3 center, Vector3 dir);
    }
}

