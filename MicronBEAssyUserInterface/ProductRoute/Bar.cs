using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Controls;
using System.Windows;
using MicronBEAssyUserInterface.Class;
using System.Windows.Media;

namespace MicronBEAssyUserInterface.ProductRoute
{
    public class Bar : Grid
    {
        public UIProduct Product { get; private set; }
        public System.Windows.Shapes.Rectangle Rectangle { get; set; }
        public TextBlock TextBlock { get; set; }
        public Dictionary<Tuple<string, string, bool, bool, int>, Bar> NextBarList { get; set; }
        public Dictionary<Tuple<string, string, bool, bool, int>, Bar> PrevBarList { get; set; }
        public int Depth { get; set; }
        public Dictionary<Bar, Tuple<System.Windows.Shapes.Line, ToTextBlock>> LineList {get; set;}
        public System.Windows.Media.Brush OriginalColor { get; set; }
        public Dictionary<int, Bar> AltProdBar { get; set; }
        public Dictionary<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>> AltLineList { get; set; }
        public Dictionary<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>> PrevAltLineList { get; set; }
        public SolidColorBrush OriginalSolidColorBursh { get; set; }

        public Tuple<string, string, bool, bool, int> Key { get; private set; }

        public Bar(UIProduct product)
        {
            NextBarList = new Dictionary<Tuple<string, string, bool, bool, int>, Bar>();
            PrevBarList = new Dictionary<Tuple<string, string, bool, bool, int>, Bar>();
            LineList = new Dictionary<Bar, Tuple<System.Windows.Shapes.Line, ToTextBlock>>();
            AltProdBar = new Dictionary<int, Bar>();
            AltLineList = new Dictionary<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>>();
            PrevAltLineList = new Dictionary<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>>();
            Product = product;
            Key = product.Key;
        }

        public System.Windows.Point GetLeftCenterPoint()
        {
            System.Windows.Point point = new System.Windows.Point();

            double x = Canvas.GetLeft(this);
            double y = Canvas.GetTop(this);

            double w = 0;
            double h = 0;
            foreach (var info in this.Children)
            {
                if (info is System.Windows.Shapes.Rectangle)
                {
                    w = (info as System.Windows.Shapes.Rectangle).Width;
                    h = (info as System.Windows.Shapes.Rectangle).Height;
                }
            }

            point.X = x;
            point.Y = y + (h / 2);

            return point;
        }

        public System.Windows.Point GetRightCenterPoint()
        {
            System.Windows.Point point = new System.Windows.Point();

            double x = Canvas.GetLeft(this);
            double y = Canvas.GetTop(this);

            double w = 0;
            double h = 0;
            foreach(var info in this.Children)
            {
                if (info is System.Windows.Shapes.Rectangle)
                {
                    w = (info as System.Windows.Shapes.Rectangle).Width;
                    h = (info as System.Windows.Shapes.Rectangle).Height;
                }
            }

            point.X = x + w;
            point.Y = y + (h / 2);

            return point;
        }
    }
}
