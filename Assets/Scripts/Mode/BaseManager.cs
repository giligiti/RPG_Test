using System;
using System.Reflection;
using UnityEngine;
//子类要显示实现私有构造函数
public class BaseManager<T>  where T :class  
{
    private static T instance;
    private static readonly object lockObject = new object();
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        Type t = typeof(T);
                        ConstructorInfo info = t.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,Type.EmptyTypes,null);
                        if (info != null)
                            instance = info.Invoke(null) as T;
                        else throw new ArgumentException("错误，单例子类未实现私有构造函数",nameof(info));
                    }
                }
            }
            return instance;
        }
    }
}
