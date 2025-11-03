using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace InventorySystem
{
    [CustomEditor(typeof(ItemData))]
    public class CreatDataGUID : Editor
    {
        private void OnEnable()
        {
            itemData = (ItemData)target;
        }
        private ItemData itemData;
        public override void OnInspectorGUI()
        {
            // 绘制默认的Inspector字段（Title、Icon等）
            DrawDefaultInspector();

            EditorGUILayout.Space(15); // 空行分隔
            EditorGUILayout.LabelField("GUID管理", EditorStyles.boldLabel);

            // 显示当前GUID（灰色，不可编辑）
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("当前GUID", itemData.GUID);
            }

            EditorGUILayout.Space(5);

            // 按钮1：手动刷新GUID（如需重新生成）
            if (GUILayout.Button("刷新GUID", GUILayout.Height(30)))
            {
                GenerateUniqueGUID();
                EditorUtility.SetDirty(itemData); // 标记为已修改，方便保存
                AssetDatabase.SaveAssets(); // 保存资源
            }
        }

        private void GenerateUniqueGUID()
        {
            string newGUID;
            do
            {
                // 生成标准UUID（如：550e8400-e29b-41d4-a716-446655440000）
                newGUID = Guid.NewGuid().ToString();
            }
            // 循环生成，直到找到全局唯一的GUID
            while (IsGUIDDuplicate(newGUID));

            // 赋值新GUID
            itemData.GUID = newGUID;
            Debug.Log($"已为 [{itemData.itemName}] 生成新GUID：{newGUID}");
        }

        private bool IsGUIDDuplicate(string newGUID)
        {
            // 1. 查找项目中所有ItemData类型的资源（后缀为.asset）
            string[] itemAssetPaths = AssetDatabase.FindAssets("t:ItemData")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            // 2. 遍历所有ItemData，对比GUID
            foreach (string path in itemAssetPaths)
            {
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item == null) continue;
                if (item.GUID == newGUID && item != itemData)
                {
                    // 找到重复的GUID（排除当前物品自身）
                    return true;
                }
            }
            return false;
        }
    }
}