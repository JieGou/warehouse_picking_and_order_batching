using System;
using System.Collections.Generic;
using System.Linq;

namespace warehouse_picking_core
{
    /// <summary>
    /// 拣货类
    /// </summary>
    public class Pickings : IPickings
    {
        public List<PickingPos> PickingList { get; private set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="w">仓库</param>
        /// <param name="wishSize">拣货个数</param>
        internal Pickings(Warehouse w, int wishSize)
        {
            int nbBlock = w.NbBlock;
            int nbAisles = w.NbAisles;
            int aisleLenght = w.AisleLenght;
            //最大货物数量
            int nbProductMax = nbBlock * nbAisles * aisleLenght;
            //分区货物数量
            int nbProductByBlock = nbAisles * aisleLenght;
            var wishList = new HashSet<PickingPos>();
            var rnd = new Random();
            for (var i = 0; i < wishSize; i++)
            {
                int wishIdx = rnd.Next(1, nbProductMax + 1);
                int blockIdx = (wishIdx - 1) / nbProductByBlock + 1;
                int temp = wishIdx - (blockIdx - 1) * nbProductByBlock;
                int aislesIdx = (temp - 1) / aisleLenght + 1;
                temp = temp - (aislesIdx - 1) * aisleLenght;
                int positionIdx = temp;
                var wish = new PickingPos(wishIdx, blockIdx, aislesIdx, positionIdx, aisleLenght, nbBlock);
                wishList.Add(wish);
            }
            PickingList = wishList.OrderBy(x => x.WishIdx).ToList();
        }

        public Pickings(Warehouse w, List<PickingPos> pickList)
        {
            PickingList = pickList;
        }
    }

    public interface IPickings
    {
        List<PickingPos> PickingList { get; }
    }

    /// <summary>
    /// 单个拣货位置类
    /// </summary>
    public class PickingPos
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int WishIdx { get; private set; } //location in the article

        /// <summary>
        /// 分区序号
        /// </summary>
        public int BlockIdx { get; private set; }

        /// <summary>
        /// 货架序号
        /// </summary>
        public int AislesIdx { get; private set; }

        /// <summary>
        /// 位置序号
        /// </summary>
        public int PositionIdx { get; private set; }

        /// <summary>
        /// 拣货X坐标
        /// </summary>
        public int PickingPointX { get; private set; }

        /// <summary>
        /// 拣货Y坐标
        /// </summary>
        public int PickingPointY { get; private set; }

        /// <summary>
        /// 离向上走可以开始转向的 剩余的Y值
        /// </summary>
        public int UpperLeftX { get; private set; }

        /// <summary>
        /// 离向上走可以开始转向的 剩余的Y值
        /// </summary>
        public int UpperLeftY { get; private set; }

        public int BottomY { get; private set; }
        public int TopY { get; private set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="wishIdx">序号</param>
        /// <param name="blockIdx">分区序号</param>
        /// <param name="aislesIdx">货架序号</param>
        /// <param name="positionIdx">位置序号</param>
        /// <param name="aisleLenght">分区单排货架长度</param>
        /// <param name="nbBlock">分区总数</param>
        public PickingPos(int wishIdx, int blockIdx, int aislesIdx, int positionIdx, int aisleLenght, int nbBlock)
        {
            WishIdx = wishIdx;
            BlockIdx = blockIdx;
            AislesIdx = aislesIdx;
            PositionIdx = positionIdx;
            // 货架两端头各有一排走廊
            PickingPointY = (blockIdx - 1) * (aisleLenght + 2) + positionIdx;

            // aislesIdx = 1 => PickingPointX = 1, aislesIdx = 2 => PickingPointX = 1 //一二排货架的PickingPointX都为1
            // aislesIdx = 3 => PickingPointX = 4, aislesIdx = 4 => PickingPointX = 4
            // aislesIdx = 5 => PickingPointX = 7, aislesIdx = 6 => PickingPointX = 7
            PickingPointX = ((aislesIdx - 1) / 2) * 3 + 1;

            // 我们改变Y的方向以便能够从屏幕的左上角进行归档
            UpperLeftY = (nbBlock - blockIdx) * (aisleLenght + 2) + (aisleLenght - positionIdx + 1);

            // aislesIdx = 1 => UpperLeftX = 0, aislesIdx = 2 => UpperLeftX = 2
            // aislesIdx = 3 => UpperLeftX = 3, aislesIdx = 4 => UpperLeftX = 5
            // aislesIdx = 5 => UpperLeftX = 6, aislesIdx = 6 => UpperLeftX = 8
            UpperLeftX = (aislesIdx / 2) * 3 + (aislesIdx % 2 - 1);

            BottomY = (blockIdx - 1) * (aisleLenght + 2);
            TopY = BottomY + aisleLenght + 1;
        }

        public override string ToString()
        {
            return "WishIdx : " + WishIdx
                   + ", BlockIdx : " + BlockIdx
                   + ", AislesIdx : " + AislesIdx
                   + ", PositionIdx : " + PositionIdx;
        }

        public override int GetHashCode()
        {
            return WishIdx;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (PickingPos)obj;
            return WishIdx == other.WishIdx;
        }
    }

    internal static class ClientWishPosExtention
    {
        internal static ShiftPoint ConverToShiftPoint(this PickingPos c)
        {
            return new ShiftPoint(c.PickingPointX, c.PickingPointY);
        }
    }
}