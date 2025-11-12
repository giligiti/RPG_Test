using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InteractSystem;
using UnityEngine;

namespace Framework
{
    public class GameCore : MonoBehaviour
    {
        #region 字段
        private static GameCore _instance;
        public static GameCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 找不到实例时，自动创建框架根节点并挂载GameCore
                    GameObject frameworkRoot = new GameObject("[FrameworkRoot]");
                    DontDestroyOnLoad(frameworkRoot); // 跨场景不销毁
                    _instance = frameworkRoot.AddComponent<GameCore>();
                }
                return _instance;
            }
        }

        static Dictionary<Type, IModule> d_ModlueDic = new Dictionary<Type, IModule>();
        static List<IModule> l_moduleList = new List<IModule>();   //根据优先度排序后的列表

        #region 核心字段

        /// <summary>
        /// 交互管理器
        /// </summary>
        /// <typeparam name="InteractMgr"></typeparam>
        /// <returns></returns>
        public static InteractMgr InteractMgr => GetModule<InteractMgr>();


        #endregion

        #endregion
        #region 初始化
        void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else _instance = this;

            InitFramework();
        }
        void InitFramework()
        {
            RegisterModule<Test2>();
            RegisterModule<InteractMgr>(100);
            l_moduleList = d_ModlueDic.Values.OrderBy(t => t.Priority).ToList();
            foreach (var item in l_moduleList)
            {
                item.Init();
            }
        }
        public void RegisterModule<T>(params object[] constructorParams) where T : IModule
        {
            Type moduleType = typeof(T);
            if (d_ModlueDic.ContainsKey(moduleType))
            {
                Debug.LogError($"重复注册，{moduleType.Name}已经存在");
                return;
            }

            try
            {
                // 查找所有私有的构造函数
                ConstructorInfo[] ctors = moduleType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

                ConstructorInfo match = null;

                foreach (ConstructorInfo ctor in ctors)
                {
                    ParameterInfo[] pars = ctor.GetParameters();

                    if ((constructorParams == null || constructorParams.Length == 0) && pars.Length == 0)
                    {
                        match = ctor;
                        break;
                    }
                    if (constructorParams == null) continue;
                    if (pars.Length != constructorParams.Length) continue;

                    bool ok = true;
                    for (int i = 0; i < pars.Length; i++)
                    {
                        Type pType = pars[i].ParameterType;
                        var arg = constructorParams[i];
                        //是否为空，如果是则看该参数是否为可空类型
                        //如果不为空，那就查看类型是否匹配，是否继承于相同的类
                        if (arg == null)
                        {
                            // null 可以赋给引用类型或可空值类型
                            if (pType.IsValueType && Nullable.GetUnderlyingType(pType) == null)
                            {
                                ok = false; break;
                            }
                        }
                        else
                        {
                            var aType = arg.GetType();
                            if (!pType.IsAssignableFrom(aType)) { ok = false; break; }
                        }
                    }
                    if (ok)
                    {
                        match = ctor; break;
                    }
                }

                object instance = null;
                if (match != null)
                {
                    instance = match.Invoke(constructorParams);
                }
                else
                {
                    Debug.LogError($"获取构造函数失败：{moduleType.Name}");
                }
                if (instance == null)
                {
                    Debug.LogError($"创建模块实例失败：{moduleType.Name}");
                    return;
                }

                var module = instance as IModule;
                if (module == null)
                {
                    Debug.LogError($"类型 {moduleType.Name} 不是 IModule");
                    return;
                }
                //登记到字典中
                d_ModlueDic[moduleType] = module;
                l_moduleList.Add(module);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        #endregion

        public static T GetModule<T>() where T : class
        {
            Type t = typeof(T);
            d_ModlueDic.TryGetValue(t, out var module);
            if (module is T mod) return mod;
            else
            {
                throw new Exception($"{t} 该类型没有登记，无法找到");
            }
        }

        #region 游戏结束
        private void DestroyFramework()
        {
            foreach (var item in l_moduleList)
            {
                item.Dispose();
            }
        }

        private void OnApplicationQuit() => DestroyFramework();

        #endregion
    }
}