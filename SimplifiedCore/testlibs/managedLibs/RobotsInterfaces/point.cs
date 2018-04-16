using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotsInterfaces
{
   public class PointDouble
    {
        private  double _x;
        private double _y;
        private double _angle;

        public PointDouble(double x, double y, double angle)
        {
            _x = x;
            _y = y;
            _angle = angle;
        }

        public double X { get => _x; set => _x = value; }
        public double Y { get => _y; set => _y = value; }
        public double Angle { get => _angle; set => _angle = value; }
    }



    public class Point
    {
        private double _x;
        private double _y;
        private double _angle;


        public Point(double x, double y, double angle)
        {
            _x = x;
            _y = y;
            _angle = angle;
        }

        public double X { get => _x; set => _x = value; }


        public double Y { get => _y; set => _y = value; }


        public double Angle { get => _angle; set => _angle = value; }
    }




    public class Pair<T1, T2>
    {
        private T1 first;
        private T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        public T1 First { get => first; set => first = value; }


        public T2 Second { get => second; set => second = value; }
    }


    
}
