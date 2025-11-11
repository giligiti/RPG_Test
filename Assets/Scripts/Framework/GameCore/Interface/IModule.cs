namespace Framework
{
    public interface IModule
    {
        /// <summary>
        /// 初始化优先级（数值越小越先初始化）
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// 初始化（加载配置、绑定事件）     
        /// </summary>
        void Init();
        /// <summary>
        /// 帧更新               
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        void Run(float deltaTime);
        /// <summary>
        /// 销毁释放（游戏结束的时候）
        /// </summary>
        void Dispose();            
    }
}