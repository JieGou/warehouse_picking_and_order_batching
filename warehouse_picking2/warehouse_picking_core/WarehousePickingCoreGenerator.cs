using System;
using warehouse_picking_core.Solver;

namespace warehouse_picking_core
{
    /// <summary>
    /// 仓库拣选生成器类
    /// </summary>
    public static class WarehousePickingCoreGenerator
    {
        /// <summary>
        /// 生成拣货问题
        /// </summary>
        /// <param name="nbBlock">分块个数</param>
        /// <param name="nbAisles">货架排数</param>
        /// <param name="aisleLenght">分区单排货架长度</param>
        /// <param name="wishSize">拣货数量</param>
        /// <returns></returns>
        public static Tuple<Warehouse, IPickings> GenerateProblem(int nbBlock, int nbAisles, int aisleLenght, int wishSize)
        {
            var warehouse = new Warehouse(nbBlock, nbAisles, aisleLenght);
            var pickings = new Pickings(warehouse, wishSize);
            return new Tuple<Warehouse, IPickings>(warehouse, pickings);
        }

        public static Tuple<Warehouse, IPickings> GenerateProblem(int nbBlock, int nbAisles, int aisleLenght, IPickings pickings)
        {
            var warehouse = new Warehouse(nbBlock, nbAisles, aisleLenght);
            return new Tuple<Warehouse, IPickings>(warehouse, pickings);
        }

        public static ISolver GenerateSolver(string name, Warehouse w, IPickings p)
        {
            switch (name)
            {
                case SShapeSolver.SolverName:
                    return new SShapeSolver(w, p);

                case SShapeSolverV2.SolverName:
                    return new SShapeSolverV2(w, p);

                case ReturnSolver.SolverName:
                    return new ReturnSolver(w, p);

                case LargestGapSolver.SolverName:
                    return new LargestGapSolver(w, p);

                case DummySolver.SolverName:
                    return new DummySolver(w, p);

                case CompositeSolver.SolverName:
                    return new CompositeSolver(w, p);

                default:
                    Console.WriteLine("Could not recognize algo " + name + ". Default to dummy");
                    return new DummySolver(w, p);
            }
        }
    }
}