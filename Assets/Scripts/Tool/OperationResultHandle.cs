namespace ToolSpace
{
    /// <summary>
    /// 操作结果对象 使用对象池复用
    /// </summary>
    public class OperationResultHandle
    {
        private readonly bool isSuccess;
        public bool IsSuccess => isSuccess;
        private string message;
        public string Message => message;

        private OperationResultHandle()
        {
            
        }
    }
}