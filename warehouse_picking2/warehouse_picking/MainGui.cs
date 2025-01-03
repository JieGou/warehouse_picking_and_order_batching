﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using warehouse_picking_core;
using warehouse_picking_core.Solver;

namespace warehouse_picking
{
    public partial class MainGui : Form
    {
        public MainGui()
        {
            InitializeComponent();
        }

        private Drawer _drawer;
        private Warehouse _currentWarehouse;
        private IPickings _currentPickings;

        private void generate_Click(object sender, EventArgs e)
        {
            var rnd = new Random();
            //const int nbBlock = 1;
            //int nbBlock = rnd.Next(1, 5);
            //int nbAisles = rnd.Next(1, 20);
            //int aisleLenght = rnd.Next(5, 25);
            int nbBlock = 2;
            int nbAisles = 12;
            int aisleLenght = 15;
            // <image url="$(ProjectDir)\DocumentImages\ShortestPickRoute.png" scale="0.4" />
            nbAisles = 8;
            aisleLenght = 3;
            if (_drawer == null)
            {
                _drawer = new Drawer();
            }
            else
            {
                _drawer.Clear();
            }
            //int wishSize = rnd.Next(1, nbBlock * nbAisles * aisleLenght) / 1;
            int wishSize = 24;
            wishSize = 6;
            //拣货问题
            var problem = WarehousePickingCoreGenerator.GenerateProblem(nbBlock, nbAisles, aisleLenght, wishSize);
            var warehouse = problem.Item1;
            IPickings pickings = problem.Item2;

            // <image url="$(ProjectDir)\DocumentImages\ShortestPickRoute_PickData.png"/>
            //按实例订单创建
            var pickInfos = new List<PickingPos>()
                {
                    new PickingPos(9,1,3,3,3,2),
                    new PickingPos(11,1,4,2,3,2),

                    new PickingPos(25,2,1,1,3,2),
                    new PickingPos(27,2,1,3,3,2),
                    new PickingPos(40,2,6,1,3,2),
                    new PickingPos(47,2,8,2,3,2),
                };
            var createdPickings = new Pickings(warehouse, pickInfos);
            problem = WarehousePickingCoreGenerator.GenerateProblem(nbBlock, nbAisles, aisleLenght, createdPickings);
            warehouse = problem.Item1;
            pickings = problem.Item2;

            _drawer.DrawWarehouse(warehouse);
            Paint += _drawer.Drawing_handler;
            _drawer.DrawPickingObjectif(pickings);
            Refresh();

            _currentWarehouse = warehouse;
            _currentPickings = pickings;
            _dummySolver = null;
            _sShapeSolver = null;
            _sShapeSolverV2 = null;
            _largestGapSolver = null;
            _returnSolver = null;
            _compositeSolver = null;
        }

        private void UpdateDistanceLastSolution(ISolution s)
        {
            distanceLastSolution.Text = s.Length().ToString(CultureInfo.InvariantCulture);
        }

        private bool IsValidSolution(ISolution s, Warehouse currentWarehouse)
        {
            for (int i = 0; i < s.ShiftPointList.Count - 1; i++)
            {
                var shiftPoint = s.ShiftPointList[i];
                var nextShiftPoint = s.ShiftPointList[i + 1];
                var isHoritontalMouvement = nextShiftPoint.Y == shiftPoint.Y;

                if (isHoritontalMouvement)
                {
                    var moveOnY = shiftPoint.Y % (currentWarehouse.AisleLenght + 2);
                    if (moveOnY != 0 && moveOnY != currentWarehouse.AisleLenght + 1)
                    {
                        var error = "Forbidden move " + shiftPoint + " to " + nextShiftPoint;
                        Console.WriteLine(error);
                        MessageBox.Show(error);
                        return false;
                    }
                }
                else
                {
                    if (shiftPoint.X % 3 == 1 && nextShiftPoint.X % 3 == 1 && shiftPoint.X == nextShiftPoint.X) continue;
                    var error = "Forbidden move " + shiftPoint + " to " + nextShiftPoint;
                    Console.WriteLine(error);
                    MessageBox.Show(error);
                    return false;
                }
            }
            return true;
        }

        private ISolver _dummySolver;
        private ISolver _sShapeSolver;
        private ISolver _sShapeSolverV2;
        private ISolver _largestGapSolver;
        private ISolver _returnSolver;
        private ISolver _compositeSolver;

        private void DummySolver_Click(object sender, EventArgs e)
        {
            if (_currentWarehouse == null || _currentPickings == null)
            {
                MessageBox.Show(@"Please start to generate a warehouse");
                return;
            }
            if (_dummySolver == null)
            {
                _dummySolver =
                    WarehousePickingCoreGenerator.GenerateSolver(warehouse_picking_core.Solver.DummySolver.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            var solution = _dummySolver.Solve();
            if (IsValidSolution(solution, _currentWarehouse))
            {
                _drawer.DrawSolution(solution);
                Refresh();
                UpdateDistanceLastSolution(solution);
            }
        }

        private ISolution SimplifySolution(ISolution s)
        {
            var simplifiedSolution = new DummySolution { ShiftPointList = new List<ShiftPoint>(), Color = s.Color };
            var origin = s.ShiftPointList[0];
            simplifiedSolution.ShiftPointList.Add(origin);
            var destination = s.ShiftPointList[1];
            var i = 1;
            var isHoritontalMouvement = origin.Y == destination.Y;
            var wayUp = isHoritontalMouvement ? origin.X < destination.X : origin.Y < destination.Y;
            while (i < s.ShiftPointList.Count - 1)
            {
                var shiftPoint = s.ShiftPointList[i];
                var nextShiftPoint = s.ShiftPointList[i + 1];
                var isHoritontalMouvement2 = nextShiftPoint.Y == shiftPoint.Y;
                var wayUp2 = isHoritontalMouvement2 ? shiftPoint.X < nextShiftPoint.X : shiftPoint.Y < nextShiftPoint.Y;
                if (isHoritontalMouvement.Equals(isHoritontalMouvement2) && wayUp.Equals(wayUp2))
                {
                    destination = s.ShiftPointList[i + 1];
                }
                else
                {
                    simplifiedSolution.ShiftPointList.Add(destination);
                    isHoritontalMouvement = isHoritontalMouvement2;
                    wayUp = wayUp2;
                    destination = nextShiftPoint;
                }
                i++;
            }
            simplifiedSolution.ShiftPointList.Add(destination);
            return simplifiedSolution;
        }

        private void SShapeSolver_Click(object sender, EventArgs e)
        {
            if (_sShapeSolver == null)
            {
                _sShapeSolver =
                    WarehousePickingCoreGenerator.GenerateSolver(warehouse_picking_core.Solver.SShapeSolver.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            Solver_Click(_sShapeSolver);
        }

        private void LargestGapSolver_Click(object sender, EventArgs e)
        {
            if (_largestGapSolver == null)
            {
                _largestGapSolver =
                    WarehousePickingCoreGenerator.GenerateSolver(
                        warehouse_picking_core.Solver.LargestGapSolver.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            Solver_Click(_largestGapSolver);
        }

        private void ReturnSolver_Click(object sender, EventArgs e)
        {
            if (_returnSolver == null)
            {
                _returnSolver =
                    WarehousePickingCoreGenerator.GenerateSolver(
                        warehouse_picking_core.Solver.ReturnSolver.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            Solver_Click(_returnSolver);
        }

        private void CompositeSolver_Click(object sender, EventArgs e)
        {
            if (_compositeSolver == null)
            {
                _compositeSolver =
                    WarehousePickingCoreGenerator.GenerateSolver(
                        warehouse_picking_core.Solver.CompositeSolver.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            Solver_Click(_compositeSolver);
        }

        private void Solver_Click(ISolver solver)
        {
            if (_currentWarehouse == null || _currentPickings == null)
            {
                MessageBox.Show(@"Please start to generate a warehouse");
                return;
            }
            if (solver == null)
            {
                MessageBox.Show(@"Solver should be create before click");
                return;
            }
            var solution = solver.Solve();
            if (!IsValidSolution(solution, _currentWarehouse)) return;
            var simplifiedSolution = SimplifySolution(solution);
            _drawer.DrawSolution(simplifiedSolution);
            Refresh();
            UpdateDistanceLastSolution(solution);
        }

        private void clear_Click(object sender, EventArgs e)
        {
            if (_drawer != null)
            {
                _drawer.Clear();
                _drawer.DrawWarehouse(_currentWarehouse);
                _drawer.DrawPickingObjectif(_currentPickings);
                Refresh();
            }
        }

        private void SShapeSolverV2_Click(object sender, EventArgs e)
        {
            if (_sShapeSolverV2 == null)
            {
                _sShapeSolverV2 =
                    WarehousePickingCoreGenerator.GenerateSolver(
                        warehouse_picking_core.Solver.SShapeSolverV2.SolverName,
                        _currentWarehouse,
                        _currentPickings);
            }
            Solver_Click(_sShapeSolverV2);
        }
    }
}