namespace warehouse_picking_core
{
    /// <summary>
    /// 仓库类
    /// </summary>
    public class Warehouse
    {
        /// <summary>
        /// 分区数量
        /// </summary>
        public int NbBlock { get; set; }

        /// <summary>
        /// 货架排数
        /// </summary>
        public int NbAisles { get; set; }

        /// <summary>
        ///分区单排货架长度
        /// </summary>
        public int AisleLenght { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="nbBlock">分块个数</param>
        /// <param name="nbAisles">货架排数</param>
        /// <param name="aisleLenght">分区单排货架长度</param>
        public Warehouse(int nbBlock, int nbAisles, int aisleLenght)
        {
            NbBlock = nbBlock;
            NbAisles = nbAisles;
            AisleLenght = aisleLenght;
        }
    }
}