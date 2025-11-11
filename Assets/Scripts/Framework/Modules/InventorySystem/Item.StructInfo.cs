namespace InventorySystem
{
    //物体的基础设定
    [System.Serializable]
    public struct ItemSetting
    {
        public bool isUsable;           //是否可以使用
        public bool isStackable;        //是否可以堆叠
        public bool isCombinable;       //是否可以调和（其他物体合成这种物体）
        public bool isDroppable;        //是否可以丢弃
        public bool isSellable;         //是否可以售卖
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