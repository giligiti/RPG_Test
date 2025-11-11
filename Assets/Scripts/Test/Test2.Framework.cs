
using Framework;
using UnityEngine;

public class Test2 : IModule
{
    private Test2()
    {
        Debug.Log("——————创建了Test2的实例对象——————");
    }

    public int Priority => 1;

    public void Dispose()
    {
    }

    public void Init()
    {
        Debug.Log("——————初始化了Test2的实例对象——————");
    }

    public void Run(float deltaTime)
    {
        
    }
}