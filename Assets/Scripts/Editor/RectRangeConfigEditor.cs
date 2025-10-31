using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RectRangeConfigSO))]
public class RectRangeConfigEditor : Editor
{
    private RectRangeConfigSO _targetConfig;

    // 使用 SerializedProperty 来保存编辑状态
    private SerializedProperty _isEditingProperty;

    private void OnEnable()
    {
        _targetConfig = target as RectRangeConfigSO;

        // 在 ScriptableObject 中创建一个序列化属性来保存编辑状态
        _isEditingProperty = serializedObject.FindProperty("_isEditing");

        if (_isEditingProperty == null)
        {
            // 如果属性不存在，创建一个
            serializedObject.Update();
            var field = _targetConfig.GetType().GetField("_isEditing",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogWarning("需要在 RectRangeConfigSO 中添加 _isEditing 字段");
            }
        }

        // 强制刷新 SceneView
        SceneView.duringSceneGui += DuringSceneGUI;
        SceneView.RepaintAll();

    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        if (_targetConfig == null)
        {
            EditorGUILayout.HelpBox("目标资产为空!", MessageType.Error);
            return;
        }

        serializedObject.Update();

        // 绘制默认属性
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("场景编辑控制", EditorStyles.boldLabel);

        // 编辑状态切换
        bool currentEditingState = _isEditingProperty != null ?
            _isEditingProperty.boolValue : false;

        bool newEditingState = EditorGUILayout.Toggle("启用 Scene 编辑", currentEditingState);

        if (newEditingState != currentEditingState)
        {
            if (_isEditingProperty != null)
            {
                _isEditingProperty.boolValue = newEditingState;
                serializedObject.ApplyModifiedProperties();
            }

            // 强制刷新所有 SceneView
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                sceneView.Repaint();
            }
            Debug.Log($"编辑状态: {newEditingState}");
        }

        // 手动刷新按钮
        if (GUILayout.Button("强制刷新 Scene 视图"))
        {
            RefreshAllSceneViews();
        }

        // 显示当前配置信息
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("当前配置:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"中心: {_targetConfig.Center}");
        EditorGUILayout.LabelField($"大小: {_targetConfig.Size}");
    }

    // 关键：使用 duringSceneGui 而不是 OnSceneGUI
    private void DuringSceneGUI(SceneView sceneView)
    {
        if (_targetConfig == null) return;

        bool isEditing = _isEditingProperty != null ? _isEditingProperty.boolValue : false;

        if (!isEditing) return;

        Handles.BeginGUI();
        GUILayout.Window(0, new Rect(10, 10, 200, 100),
            DrawSceneGUIWindow, "矩形配置编辑");
        Handles.EndGUI();

        DrawSceneHandles();
    }

    private void DrawSceneGUIWindow(int id)
    {
        GUILayout.Label($"编辑: {_targetConfig.name}");
        GUILayout.Label($"中心: {_targetConfig.Center}");
        GUILayout.Label($"大小: {_targetConfig.Size}");

        if (GUILayout.Button("结束编辑"))
        {
            if (_isEditingProperty != null)
            {
                _isEditingProperty.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                RefreshAllSceneViews();
            }
        }
    }

    private void DrawSceneHandles()
    {
        Vector3 center = _targetConfig.Center;
        Vector3 size = _targetConfig.Size;
        float handleSize = HandleUtility.GetHandleSize(center) * 0.3f;

        // 绘制中心点手柄
        Handles.color = Color.yellow;
        Handles.SphereHandleCap(0, center, Quaternion.identity, handleSize, EventType.Repaint);

        // 移动手柄
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_targetConfig, "移动矩形中心");
            _targetConfig.SetCenter(newCenter);
            EditorUtility.SetDirty(_targetConfig);
        }

        // 绘制3D立体矩形框
        Draw3DRectangle(center, size);

        // 绘制尺寸手柄
        Draw3DSizeHandles(center, size);
    }

    private void Draw3DRectangle(Vector3 center, Vector3 size)
    {
        Handles.color = Color.cyan;

        // 计算8个顶点
        Vector3 halfSize = size * 0.5f;

        Vector3[] vertices = new Vector3[8]
        {
        // 底部四个顶点
        center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // 0: 前左下
        center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z), // 1: 前右下
        center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z), // 2: 后右下
        center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z), // 3: 后左下
        
        // 顶部四个顶点
        center + new Vector3(-halfSize.x,  halfSize.y, -halfSize.z), // 4: 前左上
        center + new Vector3( halfSize.x,  halfSize.y, -halfSize.z), // 5: 前右上
        center + new Vector3( halfSize.x,  halfSize.y,  halfSize.z), // 6: 后右上
        center + new Vector3(-halfSize.x,  halfSize.y,  halfSize.z)  // 7: 后左上
        };

        // 绘制12条边
        // 底部四边形
        Handles.DrawLine(vertices[0], vertices[1]);
        Handles.DrawLine(vertices[1], vertices[2]);
        Handles.DrawLine(vertices[2], vertices[3]);
        Handles.DrawLine(vertices[3], vertices[0]);

        // 顶部四边形
        Handles.DrawLine(vertices[4], vertices[5]);
        Handles.DrawLine(vertices[5], vertices[6]);
        Handles.DrawLine(vertices[6], vertices[7]);
        Handles.DrawLine(vertices[7], vertices[4]);

        // 垂直边
        Handles.DrawLine(vertices[0], vertices[4]);
        Handles.DrawLine(vertices[1], vertices[5]);
        Handles.DrawLine(vertices[2], vertices[6]);
        Handles.DrawLine(vertices[3], vertices[7]);

        // 可选：绘制对角线以便更好地观察3D结构
        Handles.color = new Color(0, 1, 1, 0.3f); // 半透明的青色
        Handles.DrawLine(vertices[0], vertices[6]); // 前左下到后右上
        Handles.DrawLine(vertices[1], vertices[7]); // 前右下到后左上
        Handles.DrawLine(vertices[2], vertices[4]); // 后右下到前左上
        Handles.DrawLine(vertices[3], vertices[5]); // 后左下到前右上
    }

    private void Draw3DSizeHandles(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;

        // 在6个方向绘制尺寸手柄（每个面的中心）
        Vector3[] handlePositions = new Vector3[6]
        {
        center + new Vector3(halfSize.x, 0, 0),        // +X 面中心
        center + new Vector3(-halfSize.x, 0, 0),       // -X 面中心
        center + new Vector3(0, halfSize.y, 0),        // +Y 面中心
        center + new Vector3(0, -halfSize.y, 0),       // -Y 面中心
        center + new Vector3(0, 0, halfSize.z),        // +Z 面中心
        center + new Vector3(0, 0, -halfSize.z)        // -Z 面中心
        };

        Handles.color = Color.red;

        for (int i = 0; i < handlePositions.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.FreeMoveHandle(
                handlePositions[i],
                HandleUtility.GetHandleSize(handlePositions[i]) * 0.1f,
                Vector3.zero,
                Handles.SphereHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_targetConfig, "调整矩形尺寸");

                Vector3 newSize = size;
                Vector3 delta = newPos - handlePositions[i];

                switch (i)
                {
                    case 0: newSize.x = Mathf.Max(0.1f, size.x + delta.x * 2); break; // +X
                    case 1: newSize.x = Mathf.Max(0.1f, size.x - delta.x * 2); break; // -X  
                    case 2: newSize.y = Mathf.Max(0.1f, size.y + delta.y * 2); break; // +Y
                    case 3: newSize.y = Mathf.Max(0.1f, size.y - delta.y * 2); break; // -Y
                    case 4: newSize.z = Mathf.Max(0.1f, size.z + delta.z * 2); break; // +Z
                    case 5: newSize.z = Mathf.Max(0.1f, size.z - delta.z * 2); break; // -Z
                }

                _targetConfig.SetSize(newSize);
                EditorUtility.SetDirty(_targetConfig);
            }
        }

        // 可选：在角上添加额外的手柄，用于同时调整两个维度
        DrawCornerHandles(center, size);
    }

    private void DrawCornerHandles(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;

        // 8个角点
        Vector3[] cornerPositions = new Vector3[8]
        {
        center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // 前左下
        center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z), // 前右下
        center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z), // 后右下
        center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z), // 后左下
        center + new Vector3(-halfSize.x,  halfSize.y, -halfSize.z), // 前左上
        center + new Vector3( halfSize.x,  halfSize.y, -halfSize.z), // 前右上
        center + new Vector3( halfSize.x,  halfSize.y,  halfSize.z), // 后右上
        center + new Vector3(-halfSize.x,  halfSize.y,  halfSize.z)  // 后左上
        };

        Handles.color = Color.green;

        for (int i = 0; i < cornerPositions.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.FreeMoveHandle(
                cornerPositions[i],
                HandleUtility.GetHandleSize(cornerPositions[i]) * 0.08f,
                Vector3.zero,
                Handles.CubeHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_targetConfig, "调整矩形角点");

                Vector3 delta = newPos - cornerPositions[i];
                Vector3 newCenter = center;
                Vector3 newSize = size;

                // 根据角点索引调整对应的尺寸和中心
                // 这是一个简化的实现，实际可能需要更复杂的逻辑
                // 这里我们只调整尺寸，保持中心不变
                switch (i)
                {
                    case 0: // 前左下
                        newSize.x = Mathf.Max(0.1f, size.x - delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y - delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z - delta.z);
                        break;
                    case 1: // 前右下
                        newSize.x = Mathf.Max(0.1f, size.x + delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y - delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z - delta.z);
                        break;
                    case 2: // 后右下
                        newSize.x = Mathf.Max(0.1f, size.x + delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y - delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z + delta.z);
                        break;
                    case 3: // 后左下
                        newSize.x = Mathf.Max(0.1f, size.x - delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y - delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z + delta.z);
                        break;
                    case 4: // 前左上
                        newSize.x = Mathf.Max(0.1f, size.x - delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y + delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z - delta.z);
                        break;
                    case 5: // 前右上
                        newSize.x = Mathf.Max(0.1f, size.x + delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y + delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z - delta.z);
                        break;
                    case 6: // 后右上
                        newSize.x = Mathf.Max(0.1f, size.x + delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y + delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z + delta.z);
                        break;
                    case 7: // 后左上
                        newSize.x = Mathf.Max(0.1f, size.x - delta.x);
                        newSize.y = Mathf.Max(0.1f, size.y + delta.y);
                        newSize.z = Mathf.Max(0.1f, size.z + delta.z);
                        break;
                }

                _targetConfig.SetSize(newSize);
                EditorUtility.SetDirty(_targetConfig);
            }
        }
    }

    private void RefreshAllSceneViews()
    {
        foreach (SceneView sceneView in SceneView.sceneViews)
        {
            sceneView.Repaint();
        }
    }
}