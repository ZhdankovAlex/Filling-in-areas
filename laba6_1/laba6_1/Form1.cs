using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace laba6_1
{
    public partial class Form1 : Form
    {
        Graphics drawArea;
        Pen p;
        SolidBrush b;

        /*
280
340
220
260
140
360
80
320
80
180
300
40
         */

        List<Point> polygon = new List<Point>();
        bool[,] area = new bool[400, 400];

        struct Edge
        {
            public Point A, B;
            public float y; // = A.Y
            public float dx; // = 1.0 * (B.X - A.X) / (B.Y - A.Y)
            public float x; // = A.X + (int)(dx * (y - A.Y))
        }

        List<Edge> edges = new List<Edge>();
        List<Edge> activeEdges = new List<Edge>();
        
        List<List<Point>> cross = new List<List<Point>>();
        List<Point> border = new List<Point>();

        int dx, dy, incrHorizont, incrVertical, incrDiagonal, delta;
        int maxX, minX, maxY, minY;

        bool add = true;

    public Form1()
        {
            InitializeComponent();
            drawArea = pictureBox1.CreateGraphics();
        }

        private void Initialize()
        {
            add = true;

            polygon = new List<Point>();
            border = new List<Point>();
            cross = new List<List<Point>>();
            edges = new List<Edge>();
            activeEdges = new List<Edge>();

            b = new SolidBrush(Color.LightGray);
            drawArea.FillRectangle(b, 0, 0, pictureBox1.Width, pictureBox1.Height);

            p = new Pen(Brushes.Black);

            for (int y = 0; y < pictureBox1.Width; y++)
            {
                for (int x = 0; x < pictureBox1.Height; x++)
                {
                    area[x, y] = false;
                }
            }

            string[] lines_polygon = textBox1.Lines;
            for (int i = 0; i < lines_polygon.Length; i += 2)
            {
                //точки вводят для обхода многоугольника против часовой стрелки
                Point current_point = new Point(int.Parse(lines_polygon[i]), int.Parse(lines_polygon[i + 1]));
                polygon.Add(current_point);
            }
            polygon.Add(new Point(polygon[0].X, polygon[0].Y));

            minX = polygon[0].X;
            maxX = polygon[0].X;
            maxY = polygon[0].Y;
            minY = polygon[0].Y;

            for (int i = 1; i < polygon.Count(); i++)
            {
                if (polygon[i].X > maxX)
                    maxX = polygon[i].X;
                if (polygon[i].Y > maxY)
                    maxY = polygon[i].Y;
                if (polygon[i].X < minX)
                    minX = polygon[i].X;
                if (polygon[i].Y < minY)
                    minY = polygon[i].Y;
            }

            for (int i = 1; i < polygon.Count; i ++)
            {
                Edge currentEdge;
                if (polygon[i - 1].Y > polygon[i].Y)
                {
                    // А должна быть выше В
                    currentEdge.A = new Point(polygon[i - 1].X, pictureBox1.Height - polygon[i - 1].Y);
                    currentEdge.B = new Point(polygon[i].X, pictureBox1.Height - polygon[i].Y);
                }
                else
                {
                    currentEdge.B = new Point(polygon[i - 1].X, pictureBox1.Height - polygon[i - 1].Y);
                    currentEdge.A = new Point(polygon[i].X, pictureBox1.Height - polygon[i].Y);
                }
                currentEdge.y = currentEdge.A.Y;
                currentEdge.dx = (float)(currentEdge.B.X - currentEdge.A.X) /
                                       (currentEdge.B.Y - currentEdge.A.Y);
                currentEdge.x = currentEdge.A.X + (int)(dx * (currentEdge.y - currentEdge.A.Y));
                edges.Add(currentEdge);
            }
        }

        private void FillCross()
        {
            //сортируем список рёберных точек по убыванию Y (в нашей сиcтеме по возрастанию Y)
            border = border.OrderByDescending(p => p.Y).ToList();

            //раскидываем по разным спискам
            List<Point> newList = new List<Point>() { };
            cross.Add(newList);

            cross[0].Add(border[0]); //закидываем первую точку в первый список
            int position = 0; //текущий список, в которой кладём текущую точку
            for (int i = 1; i < border.Count(); i++)
            {
                //если следующая точка лежит на той же горизонтали, что и предыдущая (и это не одна и та же точка)
                if (border[i].Y == border[i - 1].Y && border[i].X != border[i - 1].X)
                {
                    //то закидываем её в этот же список 
                    cross[position].Add(border[i]);
                }
                //иначе - в следующий
                else if (border[i].Y != border[i - 1].Y)
                {
                    newList = new List<Point>() { };
                    cross.Add(newList);

                    position++;
                    cross[position].Add(border[i]);
                }
            }
        }

        private void SetPixel(int x, int y, SolidBrush b)
        {
            drawArea.FillRectangle(b, x, y, 1, 1);
            if (add)
            {
                border.Add(new Point(x, y));
            }
        }

        private bool GetColor(int x, int y)
        {
            if (area[x, y])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool XOR(Point A, Point B)
        {
            if ((!GetColor(A.X, A.Y) && GetColor(B.X, B.Y)) ||
                (GetColor(A.X, A.Y) && !GetColor(B.X, B.Y)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Rasterize()
        {
            for (int i = 1; i < polygon.Count(); i++)
            {
                dx = polygon[i].X - polygon[i - 1].X;
                dy = polygon[i].Y - polygon[i - 1].Y;

                if (dx >= dy && dx >= 0 && dy >= 0)
                {
                    int x = polygon[i - 1].X;
                    int y = polygon[i - 1].Y;

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightUp(x, y);
                }
                else if (dy >= dx && dx >= 0 && dy >= 0)
                {
                    int x = polygon[i - 1].X;
                    int y = polygon[i - 1].Y;

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightUpper(x, y);
                }
                else if (dx >= Math.Abs(dy) && dx >= 0 && dy <= 0)
                {
                    int x = polygon[i - 1].X;
                    int y = polygon[i - 1].Y;
                    dy = Math.Abs(dy);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightDown(x, y);
                }
                else if (Math.Abs(dy) >= dx && dx >= 0 && dy <= 0)
                {
                    int x = polygon[i - 1].X;
                    int y = polygon[i - 1].Y;
                    dy = Math.Abs(dy);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightDowner(x, y);
                }
                else if (Math.Abs(dx) >= Math.Abs(dy) && dx <= 0 && dy <= 0)
                {
                    int x = polygon[i].X;
                    int y = polygon[i].Y;
                    dx = Math.Abs(dx);
                    dy = Math.Abs(dy);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightUp(x, y);
                }
                else if (Math.Abs(dy) >= Math.Abs(dx) && dx <= 0 && dy <= 0)
                {
                    int x = polygon[i].X;
                    int y = polygon[i].Y;
                    dx = Math.Abs(dx);
                    dy = Math.Abs(dy);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightUpper(x, y);
                }
                else if (Math.Abs(dx) >= dy && dx <= 0 && dy >= 0)
                {
                    int x = polygon[i].X;
                    int y = polygon[i].Y;
                    dx = Math.Abs(dx);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightDown(x, y);
                }
                else if (dy >= Math.Abs(dx) && dx <= 0 && dy >= 0)
                {
                    int x = polygon[i].X;
                    int y = polygon[i].Y;
                    dx = Math.Abs(dx);

                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;

                    DrawRightDowner(x, y);
                }
            }
        }

        private void InvertArea()
        {
            //перебираем все пиксели из границы
            for (int i = 0; i < border.Count(); i++)
            {
                //запоминаем в какой линии находимся
                int y = border[i].Y;
                //будем инвертировать все пиксели в линии левее указанной точки
                for (int x = minX; x < border[i].X; x++)
                {
                    area[x, y] = !area[x, y];
                    if (area[x, y])
                    {
                        SetPixel(x, y, new SolidBrush(Color.DarkGreen));
                    }
                    else
                    {
                        SetPixel(x, y, new SolidBrush(Color.LightGray));
                    }
                }
            }
        }

        public void InvertLineArea()
        {
            //перебираем все пиксели из границы
            for (int i = 0; i < border.Count(); i++)
            {
                //запоминаем в какой линии находимся
                int y = border[i].Y;
                //инвертируем все пиксели левее прямой
                if (border[i].X < (minX + maxX) / 2)
                {
                    for (int x = border[i].X; x < (minX + maxX) / 2; x++)
                    {
                        area[x, y] = !area[x, y];

                        if (area[x, y])
                        {
                            SetPixel(x, y, new SolidBrush(Color.Lime));
                            //drawArea.DrawLine(new Pen(Color.Red, 2), (minX + maxX) / 2, 0, (minX + maxX) / 2, pictureBox1.Height);
                        }
                        else
                        {
                            SetPixel(x, y, new SolidBrush(Color.LightGray));
                            //drawArea.DrawLine(new Pen(Color.Red, 2), (minX + maxX) / 2, 0, (minX + maxX) / 2, pictureBox1.Height);
                        }
                    }
                }
                //инвертируем все пиксели правее прямой
                else
                {
                    for (int x = (minX + maxX) / 2; x < border[i].X; x++)
                    {
                        area[x, y] = !area[x, y];

                        if (area[x, y])
                        {
                            SetPixel(x, y, new SolidBrush(Color.Lime));
                            //drawArea.DrawLine(new Pen(Color.Red, 2), (minX + maxX) / 2, 0, (minX + maxX) / 2, pictureBox1.Height);
                        }
                        else
                        {
                            SetPixel(x, y, new SolidBrush(Color.LightGray));
                           // drawArea.DrawLine(new Pen(Color.Red, 2), (minX + maxX) / 2, 0, (minX + maxX) / 2, pictureBox1.Height);
                        }
                    }
                }
            }
        }

        private void FillArea()
        {
            for (int y = 0; y < pictureBox1.Width; y++)
            {
                for (int x = 1; x < pictureBox1.Height; x++)
                {
                    if (XOR(new Point(x - 1, y), new Point(x, y)))
                    {
                        area[x, y] = true;
                        SetPixel(x, y, new SolidBrush(Color.LightGreen));
                    }
                    else
                    {
                        area[x, y] = false;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Initialize();

            //не учитываем нулевую вершину
            for (int i = 1; i < polygon.Count() - 1; i++)
            {
                if ((polygon[i - 1].Y > polygon[i].Y && polygon[i + 1].Y > polygon[i].Y) ||
                    (polygon[i - 1].Y < polygon[i].Y && polygon[i + 1].Y < polygon[i].Y))
                {
                    polygon.Insert(i, new Point(polygon[i].X + 1, polygon[i].Y));
                    i++;
                }
            }
            //проверим для нулевой вершины
            if ((polygon[polygon.Count() - 2].Y > polygon[0].Y && polygon[1].Y > polygon[0].Y) ||
                    (polygon[polygon.Count() - 2].Y < polygon[0].Y && polygon[1].Y < polygon[0].Y))
            {
                polygon.Insert(polygon.Count() - 1, new Point(polygon[polygon.Count() - 1].X + 1, polygon[polygon.Count() - 1].Y));
            }

            Rasterize();

            FillCross();

            //теперь должны пробежать по всем линиям
            for (int i = 0; i < cross.Count(); i++)
            {
                //внутренние списки должны сортануть по Х
                cross[i] = cross[i].OrderByDescending(p => p.X).Distinct().ToList();
                //если у нас на этой прямой только одна вершина 
                //или есть "внутренние" точки многоугольника                
                if (cross[i].Count() % 2 != 0)
                {
                    List<Point> debug_cur_cross = cross[i];
                    for (int j = 0; j < cross[i].Count(); j++)
                    {
                        Point debug_cur_point = new Point(cross[i][j].X, cross[i][j].Y);
                        if (polygon.Contains(cross[i][j]))
                        {
                            cross[i].Insert(j, new Point(cross[i][j].X - 1, cross[i][j].Y));
                            j++;
                        }
                    }
                    //внутренние списки должны сортануть по Х
                    cross[i] = cross[i].OrderByDescending(p => p.X).ToList();
                }
                //просто заполняем имеющиеся отрезки
                for (int j = 1; j < cross[i].Count(); j += 2)
                {
                    int x1, x2;
                    if (cross[i][j - 1].X < cross[i][j].X)
                    {
                        x1 = cross[i][j - 1].X;
                        x2 = cross[i][j].X;
                    }
                    else
                    {
                        x2 = cross[i][j - 1].X;
                        x1 = cross[i][j].X;
                    }
                    int y = cross[i][j - 1].Y;
                    b = new SolidBrush(Color.Green);

                    while (x1 <= x2)
                    {
                        SetPixel(x1, y, b);
                        x1++;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Initialize();
                        
            for (int i = 1; i < polygon.Count(); i++)
            {
                drawArea.DrawLine(new Pen(Color.Blue, 1),
                                  polygon[i - 1].X, pictureBox1.Height - polygon[i - 1].Y,
                                  polygon[i].X, pictureBox1.Height - polygon[i].Y);
            }

            edges = edges.OrderByDescending(p => p.y).Reverse().ToList();

            float y = edges[0].y;
            do
            {
                for(int i=0; i< edges.FindAll(p => p.y == y).Count(); i++)
                {
                    Console.WriteLine("Add new edge: " + edges.FindAll(p => p.y == y)[i].A + "; " + edges.FindAll(p => p.y == y)[i].B);
                }

                activeEdges.AddRange(edges.FindAll(p => p.y == y));
                edges.RemoveAll(p => p.y == y);
                
                activeEdges.Sort(delegate (Edge a, Edge b)
                {
                    return a.x.CompareTo(b.x);
                });

                Console.WriteLine("Current edges: ");
                for (int i = 0; i < activeEdges.Count(); i++)
                {
                    Console.WriteLine((int)activeEdges[i].x + "; " + (int)activeEdges[i].y);
                }

                if (activeEdges.Count() % 2 == 0)
                {
                    for (int i = 1; i < activeEdges.Count(); i += 2)
                    {
                        float x1 = Math.Min(activeEdges[i - 1].x, activeEdges[i].x);
                        float x2 = Math.Max(activeEdges[i - 1].x, activeEdges[i].x);
                        drawArea.DrawLine(new Pen(Color.MediumSpringGreen, 1), x1, y, x2, y);
                    }
                }
                else
                {
                    Console.WriteLine("_______________BUG_________________");
                    drawArea.DrawLine(new Pen(Color.MediumSpringGreen, 1), 
                                      activeEdges[0].x, y, 
                                      activeEdges[activeEdges.Count() - 1].x, y);
                }

                y++;

                for (int i = 0; i < activeEdges.Count(); i++)
                {
                    if (y >= activeEdges[i].B.Y)
                    {
                        Console.WriteLine("End of edge: " + activeEdges[i].A + "; " + activeEdges[i].B);
                        activeEdges.Remove(activeEdges[i]);
                    }
                    else
                    {
                        Edge newEdge;
                        newEdge.x = activeEdges[i].x + activeEdges[i].dx;
                        newEdge.y = activeEdges[i].y;
                        newEdge.dx = activeEdges[i].dx;
                        newEdge.A = activeEdges[i].A;
                        newEdge.B = activeEdges[i].B;
                        activeEdges[i] = newEdge;                        
                    }
                }
            }
            while (activeEdges.Count != 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Initialize();
            
            //не учитываем нулевую вершину
            for (int i = 1; i < polygon.Count() - 1; i++)
            {
                if ((polygon[i - 1].Y > polygon[i].Y && polygon[i + 1].Y > polygon[i].Y) ||
                    (polygon[i - 1].Y < polygon[i].Y && polygon[i + 1].Y < polygon[i].Y))
                {
                    polygon.Insert(i, new Point(polygon[i].X + 1, polygon[i].Y));
                    i++;
                }
            }
            //проверим для нулевой вершины
            if ((polygon[polygon.Count() - 2].Y > polygon[0].Y && polygon[1].Y > polygon[0].Y) ||
                    (polygon[polygon.Count() - 2].Y < polygon[0].Y && polygon[1].Y < polygon[0].Y))
            {
                polygon.Insert(polygon.Count() - 1, new Point(polygon[polygon.Count() - 1].X + 1, polygon[polygon.Count() - 1].Y));
            }

            Rasterize();

            FillArea();
        }       

        private void button4_Click(object sender, EventArgs e)
        {
            Initialize();
            
            add = false;

            Rasterize();
            
            InvertArea();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Initialize();

            add = false;

            //не учитываем нулевую вершину
            for (int i = 1; i < polygon.Count() - 1; i++)
            {
                if ((polygon[i - 1].Y > polygon[i].Y && polygon[i + 1].Y < polygon[i].Y) ||
                    (polygon[i - 1].Y < polygon[i].Y && polygon[i + 1].Y > polygon[i].Y))
                {
                    polygon.Insert(i, new Point(polygon[i].X + 1, polygon[i].Y));
                    i++;
                }
            }
            //проверим для нулевой вершины
            if ((polygon[polygon.Count() - 2].Y > polygon[0].Y && polygon[1].Y < polygon[0].Y) ||
                    (polygon[polygon.Count() - 2].Y < polygon[0].Y && polygon[1].Y > polygon[0].Y))
            {
                polygon.Insert(polygon.Count() - 1, new Point(polygon[polygon.Count() - 1].X + 1, polygon[polygon.Count() - 1].Y));
            }

            Rasterize();

            InvertLineArea();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Initialize();

            Rasterize();

            string[] start = textBox2.Lines;
            Point startPixel = new Point(int.Parse(start[0]), int.Parse(start[1]));

            Stack<Point> fillArea = new Stack<Point>();

            border = border.OrderByDescending(p => p.Y).Reverse().Distinct().ToList();

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    if (border.Contains(new Point(x, y)))
                    {
                        area[x, y] = true;
                    }
                    else
                    {
                        area[x, y] = false;
                    }
                }
            }

            fillArea.Push(startPixel);
            while (fillArea.Count() > 0)
            {
                Point currentPixel = fillArea.Pop();

                SetPixel(currentPixel.X, currentPixel.Y, new SolidBrush(Color.Teal));
                //System.Threading.Thread.Sleep(5);
                area[currentPixel.X, currentPixel.Y] = true;
                
                if (!GetColor(currentPixel.X, currentPixel.Y - 1))
                {
                    fillArea.Push(new Point(currentPixel.X, currentPixel.Y - 1));                    
                }

                if (!GetColor(currentPixel.X, currentPixel.Y + 1))
                {
                    fillArea.Push(new Point(currentPixel.X, currentPixel.Y + 1));
                }

                if (!GetColor(currentPixel.X - 1, currentPixel.Y))
                {
                    fillArea.Push(new Point(currentPixel.X - 1, currentPixel.Y));
                }

                if (!GetColor(currentPixel.X + 1, currentPixel.Y))
                {
                    fillArea.Push(new Point(currentPixel.X + 1, currentPixel.Y));
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Initialize();

            Rasterize();

            string[] start = textBox2.Lines;
            Point startPixel = new Point(int.Parse(start[0]), int.Parse(start[1]));

            Stack<Point> fillArea = new Stack<Point>();

            border = border.OrderByDescending(p => p.Y).Reverse().Distinct().ToList();

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    if (border.Contains(new Point(x, y)))
                    {
                        area[x, y] = true;
                    }
                    else
                    {
                        area[x, y] = false;
                    }
                }
            }

            int x_min, x_max;

            fillArea.Push(startPixel);
            while (fillArea.Count() > 0)
            {
                Point currentPixel = fillArea.Pop();

                x_min = currentPixel.X;
                while (!GetColor(x_min, currentPixel.Y))
                {
                    x_min--;
                }

                x_max = currentPixel.X;
                while (!GetColor(x_max, currentPixel.Y))
                {
                    x_max++;
                }

                for (int x = x_min; x < x_max; x++)
                {
                    SetPixel(x, currentPixel.Y, new SolidBrush(Color.LightSeaGreen));
                    area[x, currentPixel.Y] = true;
                }

                bool flag = true;
                int y = currentPixel.Y - 1;
                //бежим по всем пикселям линии сверху 
                for (int x = x_min + 1; x < x_max; x++)
                {
                    //если не наткнулись на границу
                    if (!GetColor(x, y))
                    {
                        if (flag)
                        {   //закидываем пиксель в очередь (запоминаем линию до границы)
                            fillArea.Push(new Point(x, y));
                            flag = false;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }

                flag = true;
                y = currentPixel.Y + 1;
                //бежим по всем пикселям линии снизу
                for (int x = x_min + 1; x < x_max; x++)
                {
                    //если не наткнулись на границу
                    if (!GetColor(x, y))
                    {
                        if (flag)
                        {   //закидываем пиксель в очередь (запоминаем линию до границы)
                            fillArea.Push(new Point(x, y));
                            flag = false;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
        }        

        private void DrawRightUp(int x, int y)
        {
            delta = 2 * dy - dx;
            incrHorizont = 2 * dy;
            incrDiagonal = 2 * (dy - dx);
            for (int i = 0; i < dx; i++)
            {
                if (delta > 0)
                {
                    y++;
                    x++;
                    delta += incrDiagonal;
                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;
                }
                else
                {
                    delta += incrHorizont;
                    x++;
                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    //горизонтальные не будем добавлять                    
                }                
            }
        }
       
        private void DrawRightUpper(int x, int y)
        {
            delta = 2 * dx - dy;
            incrVertical = 2 * dx;
            incrDiagonal = 2 * (dx - dy);
            for (int i = 0; i < dy; i++)
            {
                if (delta > 0)
                {
                    x++;
                    delta += incrDiagonal;
                }
                else
                    delta += incrVertical;
                y++;

                SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                border.Add(new Point(x, pictureBox1.Height - y));
                area[x, pictureBox1.Height - y] = true;
            }
        }
        
        private void DrawRightDown(int x, int y)
        {
            delta = 2 * dy - dx;
            incrHorizont = 2 * dy;
            incrDiagonal = 2 * (dy - dx);
            for (int i = 0; i < dx; i++)
            {
                if (delta > 0)
                {
                    y--;
                    x++;
                    delta += incrDiagonal;
                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    border.Add(new Point(x, pictureBox1.Height - y));
                    area[x, pictureBox1.Height - y] = true;
                }
                else
                {
                    x++;
                    delta += incrHorizont;
                    SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                    //горизонтальные не будем добавлять
                }                
            }
        }
        
        private void DrawRightDowner(int x, int y)
        {
            delta = 2 * dx - dy;
            incrVertical = 2 * dx;
            incrDiagonal = 2 * (dx - dy);
            for (int i = 0; i < dy; i++)
            {
                if (delta > 0)
                {
                    x++;
                    delta += incrDiagonal;
                }
                else
                    delta += incrVertical;
                y--;

                SetPixel(x, pictureBox1.Height - y, new SolidBrush(Color.Red));
                border.Add(new Point(x, pictureBox1.Height - y));
                area[x, pictureBox1.Height - y] = true;
            }
        }        
    }
}
