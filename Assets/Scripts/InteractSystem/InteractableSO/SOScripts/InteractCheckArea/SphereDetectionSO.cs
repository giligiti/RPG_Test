using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    /// <summary>
    /// 球形检测
    /// </summary>
    [CreateAssetMenu(fileName = "SphereDetectionSO", menuName = "交互系统/检测形状/球形")]
    public class SphereDetectionSO : InteractDetectionSO
    {
        [Header("球形参数")]
        public float radius = 2f; // 检测半径

        public override ReadOnlySpan<Collider> Detect(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            center = item.Item1;
            int count = Physics.OverlapSphereNonAlloc(
                center, radius, _buffer, layerMask, triggerInteraction);

            if (count == _buffer.Length)
            {
                Debug.LogWarning("检测到的碰撞体可能超过容器所能装载的极限，需要扩容\n现在未扩容，需要手动修改脚本进行扩容");
            }
            return new ReadOnlySpan<Collider>(_buffer, 0, count);
        }

        // Gizmos绘制球形
        public override void DrawGizmos(Vector3 center, Vector3 dir)
        {
            var item = GetTurePos(center, dir);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(item.Item1, radius);
        }
    }
}

