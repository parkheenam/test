using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using MicronBEAssy;
using Mozart.Studio.UIComponents;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using System.Windows.Controls;
using Mozart.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using MicronBEAssyUserInterface.Class;

namespace MicronBEAssyUserInterface.ProductRoute
{
    public partial class ProductRouteView : XtraGridControlView
    {
        IExperimentResultItem _result;
        ResultDataContext _resultDataContext = null;
        RouteMainView _mainView = new RouteMainView();
        #region DataMart
        Dictionary<Tuple<string, string, bool, bool, int>, UIProduct> _prodInfos = new Dictionary<Tuple<string, string, bool, bool, int>, UIProduct>();
        private static Dictionary<Tuple<string, string>, UIProductDetail> _productDetailList = new Dictionary<Tuple<string, string>, UIProductDetail>();
        Dictionary<string, HashSet<string>> _comboxProductIDItems = new Dictionary<string, HashSet<string>>();
        Dictionary<string, HashSet<string>> _comboxProductNameItems = new Dictionary<string, HashSet<string>>();
        #endregion

        Bar _selectBar = null;

        public ProductRouteView()
            : base()
        {
            InitializeComponent();
        }

        public ProductRouteView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                _result = this.Document.GetResultItem();

                if (_result == null)
                    return;

                _resultDataContext = this.Document.GetResultItem().GetCtx<ResultDataContext>();
            }

            InitializeData();

            InitializeControl();
        }

        private void InitializeControl()
        {
            this.elementHost1.Child = _mainView;
            _mainView.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(_mainView_PreviewKeyDown);
            _mainView.PreviewMouseWheel += new MouseWheelEventHandler(_mainView_PreviewMouseWheel);

            this.comboBoxLine.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxLine.Items.AddRange(_comboxProductIDItems.Keys.ToArray());
            this.comboBoxLine.SelectedItem = this.comboBoxLine.Items[0];
        }

        #region Init
        private void InitializeData()
        {
            if (_resultDataContext == null)
                return;

            _prodInfos = DataHelper.LoadProductRoute(_resultDataContext);

            Dictionary<Tuple<string, string>, UIProcess> processList = DataHelper.LoadProcessStep(_resultDataContext);
            _productDetailList = DataHelper.LoadProductDetail(_resultDataContext, processList, true);

            CollectComobBoxData();
        }

        private void CollectComobBoxData()
        {
            foreach (UIProduct prod in _prodInfos.Values)
            {
#if DEBUG
                if(prod.ProductID == "369350")
                    Console.WriteLine();
#endif

                if (prod.IsMidPart)
                    continue;

                if (prod.IsMcpPart)
                    continue;

                if (prod.IsWaferPart)
                    continue;

                if (prod.ProductDetail == null)
                    continue;

                HashSet<string> productIDList;
                if (_comboxProductIDItems.TryGetValue(prod.LineID, out productIDList) == false)
                    _comboxProductIDItems.Add(prod.LineID, productIDList = new HashSet<string>());

                productIDList.Add(prod.ProductID);

                HashSet<string> productNameList;
                if (_comboxProductNameItems .TryGetValue(prod.LineID, out productNameList) == false)
                    _comboxProductNameItems.Add(prod.LineID, productNameList = new HashSet<string>());

                productNameList.Add(prod.ProductDetail.ProductName);
            }
        }

        #endregion

        #region Event

        double zoom = 1;
        void _mainView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point mousePos = e.GetPosition(this._mainView.canvas1);

            double rate = 0;

            if (e.Delta > 0)
                rate = 0.1;
            else if (e.Delta < 0)
                rate = -0.1;

            zoom = zoom + rate;

            if (zoom <= 0)
                zoom = 0.1;

            this._mainView.canvas1.RenderTransform = new ScaleTransform(zoom, zoom);

            ReSizeCanvas(Math.Ceiling(this._mainView.canvas1.Width + (this._mainView.canvas1.Width * rate)), Math.Ceiling(this._mainView.canvas1.Height + (this._mainView.canvas1.Height * rate)));

            this._mainView.scrollViewer1.ScrollToHorizontalOffset(0);
            this._mainView.scrollViewer1.ScrollToVerticalOffset(0);
        }


        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            string lineID = this.comboBoxLine.SelectedItem.ToString();

            if (this.radioButtonProductID.Checked)
            {
                HashSet<string> list;
                if (this._comboxProductIDItems.TryGetValue(lineID, out list))
                {
                    this.comboBoxProduct.Items.Clear();
                    this.comboBoxProduct.Items.AddRange(list.ToArray());
                    this.comboBoxProduct.SelectedItem = this.comboBoxProduct.Items[0];
                }

                this.layoutControlItemProduct.Text = "PRODUCT_ID";
            }
            else if (this.radioButtonProductName.Checked)
            {
                HashSet<string> list;
                if (this._comboxProductNameItems.TryGetValue(lineID, out list))
                {
                    this.comboBoxProduct.Items.Clear();
                    this.comboBoxProduct.Items.AddRange(list.ToArray());
                    this.comboBoxProduct.SelectedItem = this.comboBoxProduct.Items[0];
                }

                this.layoutControlItemProduct.Text = "PRODUCT_NAME";
            }
        }

        private void comboBoxProduct_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox cb = sender as System.Windows.Forms.ComboBox;

            string lineID = cb.SelectedItem.ToString();

            this.comboBoxProduct.Items.Clear();

            if (this.radioButtonProductID.Checked)
            {
                HashSet<string> list;
                if (this._comboxProductIDItems.TryGetValue(lineID, out list))
                {
                    this.comboBoxProduct.Items.AddRange(list.ToArray());
                    this.comboBoxProduct.SelectedItem = this.comboBoxProduct.Items[0];
                }
            }
            else
            {
                HashSet<string> list;
                if (this._comboxProductNameItems.TryGetValue(lineID, out list))
                {
                    this.comboBoxProduct.Items.AddRange(list.ToArray());
                    this.comboBoxProduct.SelectedItem = this.comboBoxProduct.Items[0];
                }
            }

            ClearMainView();
        }

        void _mainView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_selectBar != null && e.Key == Key.C && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                System.Windows.Clipboard.SetDataObject(_selectBar.TextBlock.Text);
            }
        }

        private void MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Bar bar = sender as Bar;
            //if (bar.IsMouseOver)
            //{

            //}
        }

        private void MouseLeave(object sender, EventArgs e)
        {
            //if (currentDragBar == null)
            //{
            //    Bar bar = sender as Bar;

            //    foreach (var info in bar.Children)
            //    {
            //        if (info is System.Windows.Shapes.Rectangle)
            //        {
            //            System.Windows.Shapes.Rectangle rec = info as System.Windows.Shapes.Rectangle;

            //            rec.StrokeThickness = 1;

            //            break;
            //        }
            //    }
            //}
        }

        private void Line_MouseEnter(object sender, EventArgs e)
        {
            if (_currentDragBar == null)
            {
                System.Windows.Shapes.Line line = sender as System.Windows.Shapes.Line;

                line.StrokeThickness = 3;
            }
        }

        private void Line_MouseLeave(object sender, EventArgs e)
        {
            if (_currentDragBar == null)
            {
                System.Windows.Shapes.Line line = sender as System.Windows.Shapes.Line;

                line.StrokeThickness = 1;
            }
        }

        System.Windows.Point prevMousePoint;
        Bar _currentDragBar = null;
        private void MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bar bar = sender as Bar;

            this._mainView.canvas1.Children.Remove(bar);
            this._mainView.canvas1.Children.Add(bar);
            
            _currentDragBar = bar;
            bar.Rectangle.StrokeThickness = 3;
            prevMousePoint = e.GetPosition(bar);
        }

        private void MouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bar bar = sender as Bar;

            _currentDragBar = null;
            
            if(_selectBar != null)
            {
                if (bar.NextBarList.Values.Contains(_selectBar) == false 
                    && bar.PrevBarList.Values.Contains(_selectBar) == false
                    && bar.AltProdBar.Values.Contains(_selectBar) == false
                    && bar.PrevAltLineList.Keys.Contains(_selectBar) == false)
                    bar.Rectangle.StrokeThickness = 1;
            }
            else
                bar.Rectangle.StrokeThickness = 1;

            prevMousePoint = e.GetPosition(bar);
        }

        private void MouseRightButtonDown(object sender, EventArgs e)
        {
            Bar bar = sender as Bar;
            bar.Focus();

            if (bar.Rectangle.Fill == bar.OriginalColor)
            {
                if (_selectBar != null)
                {
                    _selectBar.Rectangle.Fill = _selectBar.OriginalColor;
                    SetHighLight(_selectBar, true);
                    _selectBar = null;
                }

                GradientStop start = new GradientStop(System.Windows.Media.Colors.White, 0.4);
                GradientStop end = new GradientStop(bar.OriginalSolidColorBursh.Color, 1);
                LinearGradientBrush gb = new LinearGradientBrush(new GradientStopCollection() { start, end }, 90);

                bar.Rectangle.Fill = gb;
                _selectBar = bar;

                SetHighLight(bar, false);
            }
            else
            {
                bar.Rectangle.Fill = bar.OriginalColor;
                SetHighLight(bar, true);
                _selectBar = null;
            }
        }
        private void SetHighLightAltLine(Dictionary<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>> altLines, double strokeThickness, FontWeight fontWeight)
        {
            foreach (KeyValuePair<Bar, Tuple<System.Windows.Shapes.Path, TextBlock>> info in altLines)
            {
                info.Key.Rectangle.StrokeThickness = strokeThickness;
                info.Value.Item1.StrokeThickness = strokeThickness;
                info.Value.Item2.FontWeight = fontWeight;
            }
        }

        private void SetHighLight(Bar bar, bool isClear)
        {
            double strokeThickness = isClear ? 1 : 3;
            FontWeight fontWeight = isClear ? FontWeights.Normal : FontWeights.Bold;

            foreach (KeyValuePair<Bar, Tuple<System.Windows.Shapes.Line, ToTextBlock>> info in bar.LineList)
            {
                info.Key.Rectangle.StrokeThickness = strokeThickness;
                info.Value.Item1.StrokeThickness = strokeThickness;
                if (info.Value.Item2.LineText != null)
                    info.Value.Item2.LineText.FontWeight = fontWeight;

                if (info.Value.Item2.BinText != null)
                    info.Value.Item2.BinText.FontWeight = fontWeight;
            }

            foreach (Bar prevBar in bar.PrevBarList.Values)
            {
                Tuple<System.Windows.Shapes.Line, ToTextBlock> info;
                if (prevBar.LineList.TryGetValue(bar, out info))
                {
                    prevBar.Rectangle.StrokeThickness = strokeThickness;

                    info.Item1.StrokeThickness = strokeThickness;
                    if (info.Item2.LineText != null)
                        info.Item2.LineText.FontWeight = fontWeight;

                    if (info.Item2.BinText != null)
                        info.Item2.BinText.FontWeight = fontWeight;
                }
            }

            SetHighLightAltLine(bar.AltLineList, strokeThickness, fontWeight);
            SetHighLightAltLine(bar.PrevAltLineList, strokeThickness, fontWeight);
        }

        private void PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bar bar = sender as Bar;

            if (_currentDragBar == bar && e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point point = e.GetPosition(bar);

                double pointX = point.X - prevMousePoint.X;
                double pointY = point.Y - prevMousePoint.Y;

                Canvas.SetTop(bar, Canvas.GetTop(bar) + pointY);
                Canvas.SetLeft(bar, Canvas.GetLeft(bar) + pointX);

                double width = Canvas.GetLeft(bar);
                double heigth = Canvas.GetTop(bar);

                width += bar.Rectangle.Width + 100;
                heigth += bar.Rectangle.Height + 100;

                if(this._mainView.canvas1.Width < width)
                    this._mainView.canvas1.Width = width;

                if (this._mainView.canvas1.Height < heigth)
                    this._mainView.canvas1.Height = heigth;

                ReDrawLines(bar, pointX, pointY);

                ReDrawAltLine(bar);
            }
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            ClearMainView();

            string lineID = this.comboBoxLine.Text;

            string productID = this.comboBoxProduct.Text;

            if (lineID == null || productID == null)
                return;

            if (radioButtonProductName.Checked)
            {
                UIProductDetail productDetail = FindProductDetail(lineID, productID);
                if(productDetail != null)
                    productID = productDetail.ProductID;
            }

#if DEBUG
            if(productID == "369350")
                Console.WriteLine();
#endif

            Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(lineID, productID, false, false, 1);

            Dictionary<Tuple<string, string, bool, bool, int>, Bar> barList = new Dictionary<Tuple<string, string, bool, bool, int>, Bar>();

            double maxWidth = 0;
            double maxHeidth = 0;

            Bar firstBar = null;
            UIProduct prod;
            if (_prodInfos.TryGetValue(key, out prod))
            {
                Bar bar = CreateBar(prod, 0);
                firstBar = bar;

                if (barList.ContainsKey(bar.Key) == false)
                    barList.Add(bar.Key, bar);

                GetBars(bar, bar.Depth - 1, barList, true);
                GetBars(bar, bar.Depth + 1, barList, false);

                //List<Bar> list = new List<Bar>(barList.Values);

                //foreach (Bar info in list)
                //{
                //    GetBars(info, info.Depth - 1, barList, true);
                //    GetBars(info, info.Depth + 1, barList, false);
                //}

                List<Bar> resultBarList = new List<Bar>(barList.Values);

                SetReCalcDepth(resultBarList);

                SortBarList(resultBarList);
                
                SetAltProdBar(barList);

                DrawBar(resultBarList, out maxWidth, out maxHeidth);

                DrawLine(resultBarList);

                DrawAltLine(resultBarList);
            }

            ReSizeCanvas(firstBar, maxWidth, maxHeidth);
        }

        private void ClearMainView()
        {
            _mainView.canvas1.Children.Clear();
            this._mainView.canvas1.RenderTransform = null;
            _selectBar = null;
        }

        private void DrawAltLine(List<Bar> resultBarList)
        {
            Random r = new Random();

            foreach (Bar bar in resultBarList)
            {
                System.Windows.Media.Color color = new System.Windows.Media.Color();
                color.R = (byte)r.Next(0, 255);
                color.G = (byte)r.Next(0, 255);
                color.B = (byte)r.Next(0, 255);
                color.A = (byte)255;

                SolidColorBrush scb = new SolidColorBrush();
                scb.Color = color;

                foreach (KeyValuePair<int, Bar> altBar in bar.AltProdBar)
                {
                    System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                    path.Stroke = scb;
                    
                    path.StrokeDashArray = new DoubleCollection { 10, 1 };

                    PathGeometry pg = new PathGeometry();
                    PathFigure pf = new PathFigure();
                    pg.Figures.Add(pf);
                    pf.StartPoint = bar.GetRightCenterPoint();

                    BezierSegment bs = new BezierSegment();

                    bs.Point1 = bar.GetRightCenterPoint();
                    bs.Point2 = GetBezierSegmentPoint2(bar.GetRightCenterPoint(), altBar.Value.GetRightCenterPoint());
                    bs.Point3 = altBar.Value.GetRightCenterPoint();

                    pf.Segments.Add(bs);

                    PolyLineSegment poly = new PolyLineSegment();
                    List<System.Windows.Point> arrowPointList = GetArrowLinePoints(bs.Point2, bs.Point3);

                    foreach (System.Windows.Point p in arrowPointList)
                    {
                        poly.Points.Add(p);
                    }
                    
                    pf.Segments.Add(poly);

                    path.Data = pg;

                    TextBlock pri = CreateTextBlock(altBar.Key.ToString());
                    pri.FontSize = 9;

                    DrawTextBlockByAltLine(pri, bs.Point1, bs.Point2, bs.Point3);

                    if(bar.AltLineList.ContainsKey(altBar.Value) == false)
                        bar.AltLineList.Add(altBar.Value, new Tuple<System.Windows.Shapes.Path, TextBlock>(path, pri));

                    if (altBar.Value.PrevAltLineList.ContainsKey(bar) == false)
                        altBar.Value.PrevAltLineList.Add(bar, new Tuple<System.Windows.Shapes.Path, TextBlock>(path, pri));

                    this._mainView.canvas1.Children.Add(path);
                    this._mainView.canvas1.Children.Add(pri);
                }
            }
        }

        private void DrawTextBlockByAltLine(TextBlock pri, System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3)
        {
            double t = 0.5;

            double p1x = p1.X;
            double p2x = p2.X;
            double p3x = p3.X;

            double x = (1-t) * ((1-t)*p1x + t*p2x) + t*((1-t)*p2x + t*p3x);

            Canvas.SetTop(pri, p2.Y - pri.FontSize);

            Canvas.SetLeft(pri, x);
        }

        private List<System.Windows.Point> GetArrowLinePoints(System.Windows.Point p1, System.Windows.Point p2)
        {
            double theta = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            System.Windows.Point ap1 = new System.Windows.Point(
                p2.X + (5 * cost - 2 * sint),
                p2.Y + (5 * sint + 2 * cost));

            System.Windows.Point ap2 = new System.Windows.Point(
                p2.X + (5 * cost + 2 * sint),
                p2.Y - (2 * cost - 5 * sint));

            List<System.Windows.Point> list = new List<System.Windows.Point>();

            list.Add(ap1);
            list.Add(ap2);
            list.Add(p2);

            return list;
        }


        #endregion Event

        #region CreateControl

        private System.Windows.Shapes.Rectangle CreateRectangle(SolidColorBrush brush)
        {
            System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();

            rec.Width = 100;
            rec.Height = 20;
            rec.Stroke = brush;
            rec.RadiusX = 5;
            rec.RadiusY = 5;

            return rec;
        }

        private TextBlock CreateProductTextBlock(UIProduct product)
        {
            string text = this.radioButtonProductID.Checked ? product.ProductID : product.ProductName;

            return CreateTextBlock(text);
        }

        private TextBlock CreateTextBlock(string text)
        {
            TextBlock textBlock = new TextBlock();

            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            textBlock.Text = text;

            return textBlock;
        }

        private System.Windows.Shapes.Line CreateLine()
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.Stroke = System.Windows.Media.Brushes.LightGray;
            line.StrokeThickness = 1;

            //line.MouseLeave += Line_MouseLeave;
            //line.MouseEnter += Line_MouseEnter;

            return line;
        }

        private Bar CreateBar(UIProduct prod, int depth)
        {
            Bar bar = new Bar(prod);
            bar.Focusable = true;
            bar.Depth = depth;

            TextBlock textBlock = CreateProductTextBlock(prod);

            SolidColorBrush brush = null;
            if (prod.IsWaferPart)
                brush = System.Windows.Media.Brushes.SkyBlue;
            else if (prod.IsMidPart)
                brush = System.Windows.Media.Brushes.Orange;
            else if (prod.IsMcpPart)
                brush = System.Windows.Media.Brushes.ForestGreen;
            else
                brush = System.Windows.Media.Brushes.Red;

            System.Windows.Shapes.Rectangle rec = CreateRectangle(brush);

            bar.Rectangle = rec;
            bar.TextBlock = textBlock;

            bar.Children.Add(rec);
            bar.Children.Add(textBlock);

            GradientStop start = new GradientStop(System.Windows.Media.Colors.White, 0.7);
            GradientStop end = new GradientStop(brush.Color, 1);
            LinearGradientBrush gb = new LinearGradientBrush(new GradientStopCollection() { start, end }, 90);

            bar.OriginalColor = gb;
            bar.OriginalSolidColorBursh = brush;

            bar.Rectangle.Fill = bar.OriginalColor;

            bar.MouseEnter += MouseEnter;
            bar.MouseLeave += MouseLeave;
            bar.MouseLeftButtonDown += MouseLeftButtonDown;
            bar.MouseRightButtonDown += MouseRightButtonDown;
            bar.MouseLeftButtonUp += MouseLeftButtonUp;
            bar.PreviewMouseMove += PreviewMouseMove;

            return bar;
        }

        #endregion CreateControl

        #region Draw

        private void ReDrawAltLine(Bar bar)
        {
            foreach (Tuple<System.Windows.Shapes.Path, TextBlock> lineInfo in bar.AltLineList.Values)
            {
                PathGeometry pg = lineInfo.Item1.Data as PathGeometry;
                PathFigure pf = pg.Figures.ElementAt(0) as PathFigure;

                BezierSegment bs = null;
                foreach (PathSegment ps in pf.Segments)
                {
                    if (ps is BezierSegment)
                    {
                        bs = ps as BezierSegment;

                        pf.StartPoint = bar.GetRightCenterPoint();
                        bs.Point1 = bar.GetRightCenterPoint();
                        bs.Point2 = GetBezierSegmentPoint2(bs.Point1, bs.Point3);
                    }
                    else if (ps is PolyLineSegment)
                    {
                        PolyLineSegment poly = ps as PolyLineSegment;

                        poly.Points.Clear();

                        List<System.Windows.Point> list = GetArrowLinePoints(bs.Point2, bs.Point3);
                        foreach (System.Windows.Point p in list)
                            poly.Points.Add(p);
                    }

                    DrawTextBlockByAltLine(lineInfo.Item2, bs.Point1, bs.Point2, bs.Point3);
                }
            }

            foreach (Tuple<System.Windows.Shapes.Path, TextBlock> lineInfo in bar.PrevAltLineList.Values)
            {
                PathGeometry pg = lineInfo.Item1.Data as PathGeometry;
                PathFigure pf = pg.Figures.ElementAt(0) as PathFigure;

                BezierSegment bs = null;

                foreach (PathSegment ps in pf.Segments)
                {
                    if (ps is BezierSegment)
                    {
                        bs = ps as BezierSegment;
                        bs.Point3 = bar.GetRightCenterPoint();
                        bs.Point2 = GetBezierSegmentPoint2(bs.Point1, bs.Point3);
                    }
                    else if (ps is PolyLineSegment)
                    {
                        PolyLineSegment poly = ps as PolyLineSegment;

                        poly.Points.Clear();

                        List<System.Windows.Point> list = GetArrowLinePoints(bs.Point2, bs.Point3);
                        foreach (System.Windows.Point p in list)
                            poly.Points.Add(p);
                    }

                    DrawTextBlockByAltLine(lineInfo.Item2, bs.Point1, bs.Point2, bs.Point3);
                }
            }
        }

        private void ReDrawLines(Bar bar, double pointX, double pointY)
        {
            foreach (Tuple<System.Windows.Shapes.Line, ToTextBlock> lineInfo in bar.LineList.Values)
            {
                lineInfo.Item1.X1 = lineInfo.Item1.X1 + pointX;
                lineInfo.Item1.Y1 = lineInfo.Item1.Y1 + pointY;

                DrawTextBlockByLine(lineInfo.Item2.LineText, lineInfo.Item1);

                DrawBinTextBlockByLine(lineInfo.Item2.BinText, lineInfo.Item1);
            }

            foreach (Bar prevBar in bar.PrevBarList.Values)
            {
                Tuple<System.Windows.Shapes.Line, ToTextBlock> prevLineInfo;
                if (prevBar.LineList.TryGetValue(bar, out prevLineInfo))
                {
                    prevLineInfo.Item1.X2 = prevLineInfo.Item1.X2 + pointX;
                    prevLineInfo.Item1.Y2 = prevLineInfo.Item1.Y2 + pointY;

                    DrawTextBlockByLine(prevLineInfo.Item2.LineText, prevLineInfo.Item1);

                    DrawBinTextBlockByLine(prevLineInfo.Item2.BinText, prevLineInfo.Item1);
                }
            }
        }

        private void DrawLine(List<Bar> resultBarList)
        {
            foreach (Bar info in resultBarList)
            {
                if (info.NextBarList.Count > 0)
                {
                    foreach (Bar toBar in info.NextBarList.Values)
                    {
                        if (info.LineList.ContainsKey(toBar) == false)
                        {
                            System.Windows.Shapes.Line line = CreateLine();

                            line.X1 = info.GetRightCenterPoint().X;
                            line.Y1 = info.GetRightCenterPoint().Y;
                            line.X2 = toBar.GetLeftCenterPoint().X;
                            line.Y2 = toBar.GetLeftCenterPoint().Y;

                            _mainView.canvas1.Children.Add(line);

                            TextBlock tb = CreateTextBlock(info.Product.StepID);
                            tb.FontSize = 9;

                            DrawTextBlockByLine(tb, line);

                            _mainView.canvas1.Children.Add(tb);

                            ToTextBlock ttb = new ToTextBlock(line);
                            ttb.LineText = tb;

                            UIBinSplitInfo bin;
                            if (info.Product.BinSplitInfos.TryGetValue(toBar.Product, out bin))
                            {
                                TextBlock binTB = CreateTextBlock(string.Format("P : {0}%", bin.Portion));
                                binTB.FontSize = 9;

                                ttb.BinText = binTB;

                                DrawBinTextBlockByLine(binTB, line);

                                _mainView.canvas1.Children.Add(binTB);
                            }

                            Tuple<System.Windows.Shapes.Line, ToTextBlock> lineInfo = new Tuple<System.Windows.Shapes.Line, ToTextBlock>(line, ttb);

                            if (info.LineList.ContainsKey(toBar) == false)
                                info.LineList.Add(toBar, lineInfo);
                        }
                    }
                }
            }
        }

        private void DrawBar(List<Bar> resultBarList, out double maxWidth, out double maxHeidth)
        {
            maxWidth = 10;
            maxHeidth = 10;

            Bar maxLengthBar = FindMaxLengthBar(resultBarList);

            double recWidth = GetStringSize(maxLengthBar.TextBlock).Width + 50;

            int depth = 0;
            int i = 0;

            foreach (Bar b in resultBarList)
            {
                b.Rectangle.Width = recWidth;

                if (depth != b.Depth)
                {
                    i = 0;
                    maxWidth += recWidth + (double)this.numericUpDownLineLength.Value;
                }

                depth = b.Depth;

                double recHeigth = 10 + i * 50;

                if (maxHeidth < recHeigth)
                    maxHeidth = recHeigth;

                Canvas.SetTop(b, recHeigth);
                Canvas.SetLeft(b, maxWidth);

                i++;

                this._mainView.canvas1.Children.Add(b);
            }
        }

        private void DrawTextBlockByLine(TextBlock tb, System.Windows.Shapes.Line line)
        {
            if (tb == null)
                return;

            System.Windows.Size strSize = GetStringSize(tb);

            Canvas.SetTop(tb, line.Y2 - tb.FontSize - ((line.Y2 - line.Y1) / 1.5));
            Canvas.SetLeft(tb, line.X2 - ((line.X2 - line.X1) / 1.5));
        }

        private void DrawBinTextBlockByLine(TextBlock tb, System.Windows.Shapes.Line line)
        {
            if (tb == null)
                return;

            Canvas.SetTop(tb, line.Y2 - tb.FontSize - ((line.Y2 - line.Y1) / 4));
            Canvas.SetLeft(tb, line.X2 - ((line.X2 - line.X1) / 4));
        }

        #endregion

        #region Helper
        private UIProductDetail FindProductDetail(string lineID, string productName)
        {
            Tuple<string, string> key = new Tuple<string, string>(lineID, productName);

            UIProductDetail productDetail;
            _productDetailList.TryGetValue(key, out productDetail);

            return productDetail;
        }

        private void SetReCalcDepth(List<Bar> resultBarList)
        {
            int minDepth = resultBarList.Min(x => x.Depth);

            resultBarList.ForEach(x => x.Depth = x.Depth + (int)Math.Abs(minDepth));

            resultBarList.Sort((x, y) => x.Depth.CompareTo(y.Depth));

            foreach (Bar bar in resultBarList)
            {
                if (bar.Product.Key.Item3 == true && bar.Product.Key.Item4 == false)//AssyInPart ( MCP 투입 Part )이면 Depth 1로 고정
                    bar.Depth = 1;
                else if (bar.PrevBarList.Count == 0)
                    bar.Depth = 0;
            }

            foreach (Bar bar in resultBarList)
            {
                if (bar.PrevBarList.Values.Count > 0)
                {
                    int maxDepth = bar.PrevBarList.Values.Max(x => x.Depth);

                    bar.Depth = maxDepth + 1;
                }
            }
        }

        private Bar FindMaxLengthBar(List<Bar> resultBarList)
        {
            Bar maxLengthBar = null;
            foreach (Bar b in resultBarList)
            {
                if (maxLengthBar == null || maxLengthBar.TextBlock.Text.Length < b.TextBlock.Text.Length)
                    maxLengthBar = b;
            }

            return maxLengthBar;
        }

        private System.Windows.Size GetStringSize(TextBlock tb)
        {
            FormattedText ft = new FormattedText(
                tb.Text,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, tb.FontFamily),
                tb.FontSize,
                System.Windows.Media.Brushes.Black);

            System.Windows.Size size = new System.Windows.Size(ft.Width, ft.Height);

            return size;
        }

        private void ReSizeCanvas(Bar bar, double width, double heidth)
        {
            double canvasWidth = width;
            double canvasHeight = heidth;
            if (bar != null)
            {
                canvasWidth = width + bar.Rectangle.Width + 100;
                canvasHeight = heidth + bar.Rectangle.Height + 100;
            }

            ReSizeCanvas(canvasWidth, canvasHeight);
        }

        private void ReSizeCanvas(double canvasWidth, double canvasHeight)
        {
            double actualWidth = this._mainView.scrollViewer1.ActualWidth;
            double actualHeight = this._mainView.scrollViewer1.ActualHeight;

            this._mainView.canvas1.Width = Math.Max(canvasWidth, actualWidth);
            this._mainView.canvas1.Height = Math.Max(canvasHeight, actualHeight);
        }

        private void GetBars(Bar bar, int depth, Dictionary<Tuple<string, string, bool, bool, int>, Bar> bars, bool isPrev)
        {
            List<UIProduct> prodList = isPrev ? bar.Product.Prevs : bar.Product.Nexts;

            foreach (UIProduct info in prodList)
            {
#if DEBUG
                if(info.ProductID == "999999")
                    Console.WriteLine();
#endif
                Bar b;
                if (bars.TryGetValue(info.Key, out b) == false)
                    b = CreateBar(info, depth);

                if(bars.ContainsKey(b.Key) == false)
                    bars.Add(b.Key, b);

                if (isPrev)
                {
                    if(bar.PrevBarList.ContainsKey(b.Key) == false)
                        bar.PrevBarList.Add(b.Key, b);

                    if(b.NextBarList.ContainsKey(bar.Key) == false)
                        b.NextBarList.Add(bar.Key, bar);
                }
                else
                {
                    if(b.PrevBarList.ContainsKey(bar.Key) == false)
                        b.PrevBarList.Add(bar.Key, bar);

                    if(bar.NextBarList.ContainsKey(b.Key) == false)
                        bar.NextBarList.Add(b.Key, b);
                }

                GetBars(b, isPrev ? b.Depth - 1 : b.Depth + 1, bars, isPrev);
            }
        }

        private System.Windows.Point GetBezierSegmentPoint2(System.Windows.Point start, System.Windows.Point end)
        {
            double y = Math.Max(start.Y, end.Y);

            double gapY = Math.Abs(start.Y - end.Y);

            double maxX = Math.Max(start.X, end.X);

            System.Windows.Point point = new System.Windows.Point(maxX + 100, y - (gapY / 2));

            return point;
        }

        private void SetAltProdBar(Dictionary<Tuple<string, string, bool, bool, int>, Bar> barList)
        {
            List<Bar> list = new List<Bar>(barList.Values);

            foreach (Bar bar in list)
            {
                foreach (KeyValuePair<int, UIProduct> info in bar.Product.AltProductInfos)
                {
                    Bar altBar;
                    if (barList.TryGetValue(info.Value.Key, out altBar))
                        bar.AltProdBar.Add(info.Key, altBar);
                }
            }
        }

        private void SortBarList(List<Bar> resultBarList)
        {
            resultBarList.Sort(new BarComparer());
        }

        #endregion
    }

    public class BarComparer : IComparer<Bar>
    {
        public int Compare(Bar x, Bar y)
        {
            try
            {
                int cmp = 0;

                if (cmp == 0)
                    cmp = x.Depth.CompareTo(y.Depth);

                if (cmp == 0)
                    cmp = x.Product.CompSeq.CompareTo(y.Product.CompSeq);

                if (cmp == 0)
                    cmp = x.Product.ProductID.CompareTo(y.Product.ProductID);

                return cmp;
            }
            catch
            {
                return 0;
            }
        }

        public BarComparer()
        {
        }
    }
}
