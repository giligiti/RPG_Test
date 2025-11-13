using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Framework.Editor
{
    public class AddressableResPathGenerator : EditorWindow
    {
        // 配置项
        private AddressableAssetGroup[] _selectedGroups; // 选中的Addressable组
        private string _namespace = "Framework.Resources"; // 命名空间
        private string _rootClassName = "ResPath"; // 根静态类名（如 ResPath）
        private string _outputPath = "Assets/Scripts/Framework/GameCore/Config/Resources"; // 输出路径

        [MenuItem("EditorTool/生成资源路径结构体列表")]
        public static void StartToGenerate()
        {
            GetWindow<AddressableResPathGenerator>("资源路径生成");
        }
        void OnGUI()
        {
            GUILayout.Label("生成配置", EditorStyles.boldLabel);
            var allGroups = AddressableAssetSettingsDefaultObject.Settings.groups.ToArray();
            // 快速选择所有组
            if (GUILayout.Button("选择所有组"))
            {
                _selectedGroups = allGroups;
            }
            _namespace = EditorGUILayout.TextField("命名空间", _namespace);
            _rootClassName = EditorGUILayout.TextField("根静态类名", _rootClassName);
            _outputPath = EditorGUILayout.TextField("输出路径（相对Assets）", _outputPath);
            if (GUILayout.Button("一键生成嵌套结构体", GUILayout.Height(40)))
            {
                if (_selectedGroups == null || _selectedGroups.Length == 0)
                {
                    EditorUtility.DisplayDialog("错误", "请选择至少一个Addressable组！", "确定");
                    return;
                }
                if (string.IsNullOrEmpty(_namespace))
                {
                    EditorUtility.DisplayDialog("错误", "命名空间不能为空！", "确定");
                    return;
                }
                if (string.IsNullOrEmpty(_rootClassName))
                {
                    EditorUtility.DisplayDialog("错误", "根静态类名不能为空！", "确定");
                    return;
                }

                GenerateFunction();

                EditorUtility.DisplayDialog("成功", $"嵌套结构体 {_rootClassName}.cs 生成完成！", "确定");
                AssetDatabase.Refresh();
            }
        }

        private void GenerateFunction()
        {
            // 创建输出目录
            string fullOutputPath = Path.Combine(Application.dataPath, _outputPath.Replace("Assets/", ""));
            Directory.CreateDirectory(fullOutputPath);

            // 收集有效资源（按组分类）
            Dictionary<string, List<AddressableAssetEntry>> groupResources = new();
            foreach (var group in _selectedGroups)
            {
                // 跳过内置组（Built In Data）
                if (group.name.StartsWith("Built In"))
                {
                    Debug.LogWarning($"跳过内置组：{group.name}");
                    continue;
                }
                // 过滤有效资源（非文件夹、Address不为空）
                List<AddressableAssetEntry> validEntries = group.entries
                    .Where(entry => !string.IsNullOrEmpty(entry.address))
                    .Where(entry => entry.TargetAsset != null)
                    .Where(entry => !(entry.TargetAsset is DefaultAsset)) // 排除文件夹
                    .ToList();

                if (validEntries.Count > 0)
                {
                    groupResources[group.name] = validEntries;
                }
            }

            if (groupResources.Count == 0)
            {
                EditorUtility.DisplayDialog("警告", "没有效资源可生成！", "确定");
                return;
            }
            // 生成代码
            string classCode = GenerateClassCode(groupResources);
            // 写入文件
            string filePath = Path.Combine(fullOutputPath, $"{_rootClassName}.cs");
            File.WriteAllText(filePath, classCode);
            Debug.Log($"生成路径：{filePath}");
        }

        private string GenerateClassCode(Dictionary<string, List<AddressableAssetEntry>> groupResources)
        {
            System.Text.StringBuilder structBuilder = new();
            // 遍历每个组，生成嵌套结构体
            foreach (var (groupName, entries) in groupResources)
            {
                // 结构体名：组名转换为规范C#结构体名（如“UI面板组”→UIPanel）
                string structName = SanitizeStructName(groupName);
                // 结构体注释
                structBuilder.AppendLine($"        /// <summary> {groupName}（对应Addressable的{groupName}组） </summary>");
                structBuilder.AppendLine($"        public readonly struct {structName}");
                structBuilder.AppendLine("        {");
                structBuilder.AppendLine();
                // 生成结构体内的资源字段
                foreach (var entry in entries)
                {
                    // 资源文件名（去后缀）→ 字段名
                    string resourceFileName = Path.GetFileNameWithoutExtension(entry.address);
                    // 字段值=Addressable的完整路径（如 Assets/UI/MainPanel.prefab）
                    string fieldValue = entry.address;

                    // 字段代码（带路径注释）
                    structBuilder.AppendLine($"            /// <summary> 路径：{fieldValue} </summary>");
                    structBuilder.AppendLine($"            public static readonly string {resourceFileName} = \"{fieldValue}\";");
                    structBuilder.AppendLine();
                }
                // 闭合结构体
                structBuilder.AppendLine("        }");
                structBuilder.AppendLine(); // 空行分隔不同结构体
            }
            return
$@"namespace {_namespace}
{{
    /// <summary>
    /// 全局资源路径定义（自动生成，请勿手动修改！）
    /// 生成工具：Tools/Addressable/生成嵌套结构体资源路径
    /// 访问格式：{_rootClassName}.组结构体.资源字段
    /// 字段结构：字段名=资源文件名，字段值=完整路径
    /// </summary>
    public static class {_rootClassName}
    {{
        {structBuilder.ToString().TrimEnd()}
    }}
}}";
        }
        /// <summary>
        /// 清理结构体名（符合C#结构体名规范）
        /// </summary>
        private string SanitizeStructName(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return "DefaultStruct";
            // 去除非法字符（空格、横杠、点等），替换为下划线
            string sanitized = System.Text.RegularExpressions.Regex.Replace(groupName, @"[^a-zA-Z0-9_]", "_");
            // 拆分单词并首字母大写（如“ui_panel”→UIPanel，“UI面板”→UIPanel）
            var words = sanitized.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            sanitized = string.Concat(words.Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
            // 确保以字母开头
            if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]))
            {
                sanitized = "Group_" + sanitized;
            }
            return sanitized;
        }
    }
    
}
