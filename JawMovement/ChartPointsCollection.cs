using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace JawMovementTool
{
    public class ChartPointsCollection : RingArray<ChartPoint>
    {
        private const int TOTAL_POINTS = 10000;

        public ChartPointsCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }

    public class ChartPoint
    {
        public double y { get; set; }
        public double x { get; set; }
        public double index { get; set; }
        public ChartPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
