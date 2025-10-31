using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    [CreateAssetMenu(fileName = "Box", menuName = "交互系统/检测形状/盒形")]
    public class BoxDetectionSO : InteractDetectionSO
    {
        [Header("盒形参数")]
        public Vector3 halfExtents = new Vector3(1f, 1f, 2f); // 半尺寸（盒形的一半大小）

        public override ReadOnlySpan<Collider> Detect(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            int count = Physics.OverlapBoxNonAlloc(
                item.Item1, halfExtents, _buffer, item.Item2, layerMask, triggerInteraction);

            if (count == _buffer.Length)
                Debug.LogWarning("盒形检测可能超过缓冲区容量，需扩容");

            return new ReadOnlySpan<Collider>(_buffer, 0, count);
        }

        // Gizmos绘制盒形
        public override void DrawGizmos(Vector3 center, Vector3 dir)
        {
            var (truePos, rotation) = GetTurePos(center, dir);
            Vector3 _size = halfExtents * 2f;

            // 绘制检测盒（半透明红色，方便观察）
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.matrix = Matrix4x4.TRS(truePos, rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, _size); // 用完整尺寸绘制

            // 绘制检测盒线框（红色，突出轮廓）
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, _size);

            // 重置Gizmos矩阵
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

