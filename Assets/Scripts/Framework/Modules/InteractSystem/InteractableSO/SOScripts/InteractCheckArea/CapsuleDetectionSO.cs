using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    [CreateAssetMenu(fileName = "Capsule", menuName = "我的框架/交互系统/检测形状/胶囊形")]
    public class CapsuleDetectionSO : InteractDetectionSO
    {
        [Header("胶囊形参数")]
        public float radius = 0.5f; // 胶囊半径
        public float height = 2f; // 胶囊高度（不含两端半球）
        public Vector3 direction = Vector3.up; // 胶囊轴向（up/right/forward）

        public override ReadOnlySpan<Collider> Detect(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            center = item.Item1;
            Quaternion rotation = item.Item2;
            // 1. 计算胶囊两端
            Vector3 normalizedDir = rotation * direction.normalized;
            float halfHeight = height * 0.5f;
            Vector3 start = center - normalizedDir * halfHeight;
            Vector3 end = center + normalizedDir * halfHeight;

            // 2. NonAlloc 检测
            int count = Physics.OverlapCapsuleNonAlloc(
                start, end, radius, _buffer, layerMask, triggerInteraction);

            if (count == _buffer.Length)
                Debug.LogWarning("胶囊检测可能超过缓冲区容量，需扩容");

            return new ReadOnlySpan<Collider>(_buffer, 0, count);
        }

        // Gizmos绘制胶囊形
        public override void DrawGizmos(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            center = item.Item1;
            Quaternion rotation = item.Item2;
            Gizmos.color = Color.red;
            Vector3 normalizedDir = rotation * direction.normalized;
            float halfHeight = height / 2f;
            Vector3 start = center - normalizedDir * halfHeight;
            Vector3 end = center + normalizedDir * halfHeight;

            // 绘制胶囊的圆柱部分
            Gizmos.DrawLine(start, end);
            // 绘制两端的半球（简化为球体）
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);
        }
    }
}

