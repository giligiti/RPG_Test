namespace InventorySystem
{
    //物体的基础设定
    [System.Serializable]
    public struct ItemSetting
    {
        public bool isUsable;
        public bool isStackable;
        public bool isCombinable;
        public bool isDroppable;
    }
    #region 合成系统设置
    [System.Serializable]
    public struct ItemCombineSettings
    {
        public ushort requiredCurrentAmount;
        public ushort requiredSecondAmount;
        public ushort resultItemAmount;         //

        public string combineWithID;
        public string resultCombineID;
        public int playerItemIndex;

        public bool inheritCustomData;
        public bool inheritFromSecond;
        public string inheritKey;
        public ItemCustomData customData;
    }
    public class ItemCustomData
    {
        [UnityEngine.TextArea(3, 10)] public string JsonData;
    }
    #endregion

}