namespace Framework
{
    public class PoolManager : IModule
    {
        #region IModule接口实现
        public int Priority => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Run(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }

    /// <summary>
    /// 可池化物体接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 当被取出的时候
        /// </summary>
        void OnTakeOut();
        /// <summary>
        /// 当被返回对象池的时候
        /// </summary>
        void OnReturn();
    }

}