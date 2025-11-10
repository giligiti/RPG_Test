using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    [CreateAssetMenu(fileName = "Box", menuName = "我的框架/交互系统/检测形状/盒形")]
    public class BoxDetectionSO : InteractDetectionSO
    {
        [Header("盒形参数")]
        public Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f); // 半尺寸（盒形的一半大小）

        public override ReadOnlySpan<Collider> Detect(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            int count = Physics.OverlapBoxNonAlloc(
                item.Item1, halfExtents, _buffer, item.Item2, layerMask, triggerInteraction);
            if (count == _buffer.Length)
                Debug.LogWarning("盒形检测可能超过缓冲区容量，需扩容");

            return new ReadOnlySpan<Collider>(_buffer, 0, count);
        }
        void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation)
        {
            // 计算盒子的8个顶点（在局部空间）
            Vector3[] vertices = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                vertices[i] = center + rotation * new Vector3(
                    halfExtents.x * (i % 2 == 0 ? 1 : -1),
                    halfExtents.y * (i / 4 == 0 ? 1 : -1),
                    halfExtents.z * ((i / 2) % 2 == 0 ? 1 : -1)
                );
            }

            // 连接顶点绘制盒子的12条边
            int[] indices = new int[] { 0, 1, 1, 3, 3, 2, 2, 0, 4, 5, 5, 7, 7, 6, 6, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
            for (int i = 0; i < indices.Length; i += 2)
            {
                Debug.DrawLine(vertices[indices[i]], vertices[indices[i + 1]], Color.green);
            }
        }

        // Gizmos绘制盒形
        public override void DrawGizmos(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            DrawBox(item.Item1, halfExtents, item.Item2);
            // var (truePos, rotation) = GetTurePos(center, dir);
            // Vector3 _size = halfExtents * 2f;

            // // 绘制检测盒（半透明红色，方便观察）
            // Gizmos.color = new Color(1, 0, 0, 0.3f);
            // Gizmos.matrix = Matrix4x4.TRS(truePos, rotation, Vector3.one);
            // Gizmos.DrawCube(Vector3.zero, _size); // 用完整尺寸绘制

            // // 绘制检测盒线框（红色，突出轮廓）
            // Gizmos.color = Color.red;
            // Gizmos.DrawWireCube(Vector3.zero, _size);

            // // 重置Gizmos矩阵
            // Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

