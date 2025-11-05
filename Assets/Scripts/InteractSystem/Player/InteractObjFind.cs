using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ToolSpace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace InteractSystem
{
    /// <summary>
    /// 负责检测可交互物体
    /// 实现方法：
    /// 提供检测到的可交互物体的接口
    /// 切换选中的可交互物体
    /// </summary>
    public class InteractObjFind : MonoBehaviour
    {
        [SerializeField]
        protected InteractDetectionSO interactSO;               //交互检测配置文件

        [Tooltip("交互检测范围配置文件,表示检测区域的有效范围")]
        [SerializeField]
        protected RectRangeConfigSO rangeConfigSO;              //根据这个来过滤不在此范围的物体

        protected Camera mainCamera;
        List<InteractInterfaceHandle> interfaceInfos;           //检测到的所有交互封装对象

        CustomePriorityQueue<InteractInterfaceHandle, float> priorityQueue;  //堆排序优先队列
        
        protected InteractInterfaceHandle itf_interactable;
        public InteractInterfaceHandle Itf_interactable => itf_interactable;
        private CancellationTokenSource tokenSource;

        public UnityEvent<InteractInterfaceHandle> interactUpdateEvent;
        public Func<InteractInterfaceHandle, float> sortAction;

        #region 初始化
        void Awake()
        {
            interfaceInfos = new(4);
            priorityQueue = new(4);
            tokenSource = new();
            mainCamera = Camera.main;
        }
        void OnDisable()
        {
            StopCheck();
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        public void AddListener(UnityAction<InteractInterfaceHandle> callBack)
        {
            interactUpdateEvent.AddListener(callBack);
        }
        /// <summary>
        /// 注销可交互接口对象检测事件
        /// </summary>
        /// <param name="callBack"></param>
        public void RemoveListener(UnityAction<InteractInterfaceHandle> callBack)
        {
            interactUpdateEvent.RemoveListener(callBack);
        }
        
        /// <summary>
        /// 默认排序方法:优先选择靠近摄像机视角中心的物体
        /// </summary>
        private float DefaultSortFunction(InteractInterfaceHandle handle)
        {
            Vector3 sightDirection = mainCamera.transform.forward;
            Vector3 dir = handle.obj.transform.position - mainCamera.transform.position;
            // 计算两个方向的夹角（角度）
            return Vector3.Angle(sightDirection, dir);
        }

        #endregion
        /// <summary>
        /// 开始检测
        /// </summary>
        /// <param name="sortAction">排序方法：接收一个交互接口封装的handle对象返回优先级的委托。让方法内部能够根据这个委托获取优先级</param>
        public void StartCheck(UnityAction<InteractInterfaceHandle> callBack, Func<InteractInterfaceHandle, float> sortAction)
        {
            interactUpdateEvent.AddListener(callBack);
            _ = CheckLoop(tokenSource.Token);
            this.sortAction = sortAction ?? DefaultSortFunction;
        }
        /// <summary>
        /// 停止检测
        /// </summary>
        public void StopCheck()
        {
            tokenSource.Cancel();
            interactUpdateEvent.RemoveAllListeners();
        }

        /// <summary>
        /// 交互检测方法
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask CheckLoop(CancellationToken token)
        {
            Debug.Log("开始检测");
            try
            {
                while (true)
                {
                    await UniTask.Delay(interactSO.checkOffset, cancellationToken: token);
                    //开始检测
                    Check();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("检测已经取消");
            }
            void Check()
            {
                //获取所有检测到的交互脚本，通过其中的属性获取含有物体实现的所有接口的对象
                interfaceInfos.Clear();
                Debug.Log(1);
                foreach (var c in interactSO.Detect(transform.position, mainCamera.transform.forward))
                {
                    Debug.Log("检测到可交互层级的物体，尝试检测是否符合有效范围");
                    //判断检测到的可交互物体的最靠近检测范围中心的点是否在有效范围内
                    if (!rangeConfigSO.IsContains(transform.position, transform.forward, c.ClosestPoint(GetIntectConfigWorldCenter())))
                        continue;          // 跳过不在配置文件定义的有效范围内的物体
                    Debug.Log("检测到有效范围内的物体，尝试获取接口对象");
                    bool canGet = InteractMgr.Instance.TryGetInteractHandle(c.gameObject, out InteractableObject interactObj);
                    if (canGet) interfaceInfos.Add(interactObj.InterfaceHandle);
                    else Debug.Log("获取接口对象失败");
                }
                //如果检测到存在可交互对象
                if (interfaceInfos.Count != 0)
                {
                    InteractInterfaceHandle handle = sortAction == null ? ProvideInterface() : ProvideInterface(sortAction);
                    if (handle == null) Debug.LogWarning("检测到对象结果返回空");
                    //如果最优先的可交互对象改变，则触发事件
                    if (itf_interactable == null || itf_interactable != handle)
                    {
                        interactUpdateEvent.Invoke(handle);
                        itf_interactable = handle;
                    }
                }
                //如果没有检测到可交互对象，而之前检测到了，表明交互对象消失，则进行通知
                else if (itf_interactable != null)
                {
                    interactUpdateEvent.Invoke(null);
                    itf_interactable = null;
                }
            }
        }
        #region 辅助函数
        /// <summary>
        /// 提供检测到的接口对象
        /// </summary>
        /// <returns></returns>
        public InteractInterfaceHandle ProvideInterface() => interfaceInfos.FirstOrDefault();
        /// <summary>
        /// 让外界能够进行优先级排序,返回最优先的接口对象
        /// </summary>
        /// <param name="action">接收一个交互接口封装的handle对象返回优先级的委托。让方法内部能够根据这个委托获取优先级</param>
        /// <returns></returns>
        public InteractInterfaceHandle ProvideInterface(Func<InteractInterfaceHandle, float> action)
        {
            priorityQueue.Clear();
            foreach (var item in interfaceInfos)
            {
                if (item == null) continue;
                priorityQueue.Enqueue(item, action.Invoke(item));
            }
            priorityQueue.TryPeek(out InteractInterfaceHandle element, out float priority);
            return element;
        }
        /// <summary>
        /// 用于得到有效检测范围的此时世界坐标系下的中心
        /// </summary>
        /// <returns></returns>
        Vector3 GetIntectConfigWorldCenter()
        {
            return transform.position + Quaternion.LookRotation(transform.forward) * rangeConfigSO.Center;
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)        // 编辑模式直接return
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Quaternion.LookRotation(transform.forward) * rangeConfigSO.Center, rangeConfigSO.Size);
            interactSO.DrawGizmos(transform.position, mainCamera.transform.forward);
        }
        void OnGUI()
        {
            if (!Application.isPlaying) return;             // 运行期才画

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                normal = { textColor = Color.white }
            };
            Rect rect = new Rect(20, 20, 400, 80);          // 左上角显示
            Rect rect1 = new Rect(20, 80, 400, 80);
            GUI.color = Color.white;
            GUI.Label(rect, $"检测到的物体: {interfaceInfos.Count}", style);
            string n = itf_interactable != null ? itf_interactable.obj.name : "null";
            GUI.Label(rect1, $"当前选中的: {n}", style);
        }
        
        #endregion
    }
    

}
