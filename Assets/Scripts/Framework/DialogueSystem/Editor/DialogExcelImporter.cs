using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DialogueSystem;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// //找到Excel的文件位置筛选选取所有的 .xlsx文件，以此得到Excel文件的绝对路径
/// 然后开始创建SO实例以及初始化SO
/// 先通过拼凑出SO的文件名
/// </summary>
public static class DialogExcelImporter
{
    private static string soDirPath = "Assets/Resources/Config/Dialog";
    private static string excelDirpath = "Assets/Scripts/Framework/DialogueSystem/DialogExcel/Excel";
    private static Dictionary<string, Type> allInterfaceCollition;

    [MenuItem("EditorTool/DialogueSystem/Excel导入")]
    public static void ImportAllExcel()
    {
        FindDialogInterface();
        DirectoryInfo directoryInfo = Directory.CreateDirectory(excelDirpath);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        foreach (var item in fileInfos)
        {
            if (item.Extension == ".xlsx" && !item.Name.StartsWith("~$"))
            {
                Debug.Log(item.Name);
                InstantiateSOAsset(item.FullName);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    private static void InstantiateSOAsset(string excelPath)
    {
        //拼凑出SO的绝对路径（改变后缀，合并存储路径）
        FileInfo fileInfo = new FileInfo(excelPath);
        string newName = Path.ChangeExtension(fileInfo.Name, ".asset");
        string configPath = Path.Combine(soDirPath, newName);
        //寻找或者创建实例
        DialogConfig dialogConfig = AssetDatabase.LoadAssetAtPath<DialogConfig>(configPath);
        bool needCreat = dialogConfig == null;
        if (needCreat) Debug.LogWarning($"没找到该实例 :{configPath}");
        if (needCreat) dialogConfig = ScriptableObject.CreateInstance<DialogConfig>();

        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];
            int maxrow = worksheet.Dimension.End.Row;
            int maxCol = worksheet.Dimension.End.Column;
            
            for (int i = 2; i < maxrow; i++)
            {
                if (string.IsNullOrWhiteSpace(worksheet.Cells[i, 1].Text.Trim())) break;
                DialogStepConfig step = new();
                step.isPlayer = System.Convert.ToBoolean(worksheet.Cells[i, 1].Value);
                step.content = worksheet.Cells[i, 2].Text.Trim();
                step.onStartEvent = ConverDialogEvent(worksheet.Cells[i,3].Text.Trim());
                Debug.Log("设置表格的数据为 ：" + step.isPlayer + "内容为：" + step.content);

                if (needCreat) dialogConfig.stepConfigs.Add(step);
            }
        }

        if (needCreat) AssetDatabase.CreateAsset(dialogConfig, configPath);
        EditorUtility.SetDirty(dialogConfig);
    }
    private static List<IDialogEvent> ConverDialogEvent(string eventString)
    {
        List<IDialogEvent> dialogEvents = new List<IDialogEvent>();
        if (string.IsNullOrEmpty(eventString)) return dialogEvents;
        //这里是自定义规则，定义换行符是不同的IDialogEvent对象 分割不同的事件
        string[] eventStrings = eventString.Split('\n');        
        
        foreach (var item in eventStrings)
        {
            string[] eventStringSplits = item.Split(':');
            if (eventStringSplits.Length != 2) Debug.LogError("对话事件格式错误：" + eventStrings);

            string typeString = eventStringSplits[0];
            string valueString = eventStringSplits[1];

            if (allInterfaceCollition.TryGetValue($"Dialog{typeString}Event", out var eventype))
            {
                IDialogEvent eventobj = (IDialogEvent)Activator.CreateInstance(eventype);
                eventobj.ConverString(valueString);
                dialogEvents.Add(eventobj);
                Debug.Log($"编辑器：{eventype} 类型对象添加成功");
            }
            else Debug.LogError($"该对话事件类型不存在： Dialog{typeString}Event, {typeString} | {valueString}");
        }
        return dialogEvents;
    }

    private static void FindDialogInterface()
    {
        allInterfaceCollition = new();
        Type type = typeof(IDialogEvent);
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();    //所有的程序集
        foreach (var item in assemblies)
        {
            //获取继承于IDialogEvent且不是抽象类的类型
            Type[] types = item.GetTypes().Where(i => type.IsAssignableFrom(i) && !i.IsAbstract).ToArray();
            foreach (var t in types)
            {
                allInterfaceCollition.Add(t.Name,t);
            }
        }
    }

}