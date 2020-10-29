using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace Automaton
{
    public enum edge
    {
        Left, Right, Top, Bottom
    }

    // полуплоскость
    [Serializable]
    public class plane
    {
        public edge kind;
        public int coord;

        // точка внутри полуплоскости
        public bool inside(int x, int y)
        {
            return (kind == edge.Left && x <= coord) || (kind == edge.Right && x >= coord) || (kind == edge.Top && y <= coord) || (kind == edge.Bottom && y >= coord);
        }

        public bool inside(Point p)
        {
            return inside(p.X, p.Y);
        }

        public plane(int coord0, edge kind0)
        {
            kind = kind0;
            coord = coord0;
        }

    }

    // пересечение полуплоскостей
    [Serializable]
    public class plane_intercept : List<plane>
    {
        // точка внутри пересечения полуплоскостей
        public bool inside(int x, int y)
        {
            foreach (plane t in this)
            {
                if (!t.inside(x, y)) return false;
            }
            return true;
        }

        public bool inside(Point p)
        {
            return inside(p.X, p.Y);
        }

    }

    // объединение и пересечение полуплоскостей
    [Serializable]
    public class plane_union : List
     <plane_intercept>
    {
        // точка внутри объединения
        public bool inside(int x, int y)
        {
            foreach (plane_intercept t in this)
            {
                if (t.inside(x, y)) return true;
            }
            return false;
        }

        public bool inside(Point p)
        {
            return inside(p.X, p.Y);
        }

    }

    [Serializable]
    public class lab
    {
        public Automaton savedAutomaton;
        // список вручную установленных стенок
        public Dictionary<Point, bool> wall = new Dictionary<Point, bool>();

        // расположение лабиринта на вкладке
        public int Left, Top;
        // размеры лабиринта на экране в клетках
        public int xm, ym;
        // размер клетки в пикселях
        public int d = 0;

        // координаты верхнего левого угла отображаемой области лабиринта
        public int x0 = 0;
        public int y0 = 0;

        // список полуплоскостей
        plane_union planes = new plane_union();

        // наличие стенок во всем лабиринте по умолчанию 
        public bool space = false;

        // координаты точки A
        public Point a = new Point(10000, 10000);
        // координаты точки B
        public Point b = new Point(10000, 10000);

        public Point start = new Point(10000, 10000);

        // найденный наилучший путь
        public List<Point> best_path =
         new List<Point>();
        // все просмотренные при поиске пути клетки
        public List<Point> all_path =
         new List<Point>();

        // канвас вкладки, на которой рисуется лабиринт
        [NonSerialized()]
        public Graphics can;

        // основное окно
        [NonSerialized()]
        public form f;

        // кисти для клеток лабиринта
        [NonSerialized()]
        SolidBrush brush_white, brush_black, brush_all_path, brush_best_path, brush_automaton;

        // ячейка лабиринта с координатами x,y
        // true - есть стенка, false - нет
        public bool cell(int x, int y)
        {
            var p = new Point(x, y);
            if (wall.ContainsKey(p))
            {
                return wall[p];
            }

            if (planes.inside(x, y))
            {
                return true;
            }

            return space;
        }

        public bool cell(Point p)
        {
            return cell(p.X, p.Y);
        }

        // установить/удалить стенку в ячейке лабиринта x,y
        public void setcell(int x, int y, bool u)
        {
            var p = new Point(x, y);
            if (wall.ContainsKey(p))
            {
                wall[p] = u;
            }
            else
            {
                wall.Add(p, u);
            }
        }


        public void Clear(bool sp)
        {
            space = sp;
            wall.Clear();
            planes.Clear();
            paint();
            /*
            foreach (var t in plane)
               {
               if (t.kind == edge.Left)
                  {
                  t.kind = edge.Right;
                  }
               }
            */
        }

        public void add_plane(int coord, edge kind)
        {
            var t = new plane(coord, kind);
            var it = new plane_intercept();
            it.Add(t);
            planes.Add(it);
            var temp = new List<Point>();
            foreach (Point p in wall.Keys)
            {
                if (t.inside(p))
                {
                    temp.Add(p);
                }
            }
            foreach (Point p in temp)
            {
                wall.Remove(p);
            }
        }

        // операция объединения лабиринтов
        public void unify(lab dest, form f)
        {
            var tab = new page(f);
            tab.Text = "Объединение";
            var tp = f.tabc.TabPages;
            tp.Insert(f.tabc.TabCount, tab);

            var src = this;
            var res = tab.lab;

            foreach (Point p in src.wall.Keys)
            {
                int x = p.X;
                int y = p.Y;
                res.setcell(x, y,
                 src.cell(x, y) || dest.cell(x, y));
            }
            foreach (Point p in dest.wall.Keys)
            {
                int x = p.X;
                int y = p.Y;
                res.setcell(x, y,
                 src.cell(x, y) || dest.cell(x, y));
            }
            foreach (plane_intercept t in src.planes)
            {
                res.planes.Add(t);
            }
            foreach (plane_intercept t in dest.planes)
            {
                res.planes.Add(t);
            }
            res.space = src.space || dest.space;

            tab.lab.f = f;
            tab.lab.init();
            f.resize(null, null);
            f.tabc.SelectedTab = tab;
        }

        // операция пересечения лабиринтов
        public void intercept(lab dest, form f)
        {
            var tab = new page(f);
            tab.Text = "Пересечение";
            var tp = f.tabc.TabPages;
            tp.Insert(f.tabc.TabCount, tab);

            var src = this;
            var res = tab.lab;

            foreach (Point p in src.wall.Keys)
            {
                int x = p.X;
                int y = p.Y;
                res.setcell(x, y,
                 src.cell(x, y) && dest.cell(x, y));
            }
            foreach (Point p in dest.wall.Keys)
            {
                int x = p.X;
                int y = p.Y;
                res.setcell(x, y,
                 src.cell(x, y) && dest.cell(x, y));
            }
            foreach (plane_intercept t in src.planes)
            {
                foreach (plane_intercept s in dest.planes)
                {
                    var r = new plane_intercept();
                    foreach (plane p in t)
                    {
                        r.Add(p);
                    }
                    foreach (plane q in s)
                    {
                        r.Add(q);
                    }
                    res.planes.Add(r);
                }
            }
            res.space = src.space && dest.space;

            tab.lab.f = f;
            tab.lab.init();
            f.resize(null, null);
            f.tabc.SelectedTab = tab;
        }

        public bool check_path()
        {
            return !(a.IsEmpty || b.IsEmpty || a == b || cell(a.X, a.Y) || cell(b.X, b.Y) || Math.Sqrt(dr(a, b)) > 5000);
        }

        public void find_path()
        {
            if (!check_path()) return;
            var Open = new List<Point>();
            var Close = new List<Point>();
            Open.Add(a);

            var chain = new Dictionary<Point, Point>();
            bool found = false;
            for (int n = 1; n <= 10000; n++)
            {
                if (Open.Count == 0)
                {
                    found = false;
                    break;
                }
                Point best = Open[0];
                double r = dr(best, a);
                foreach (Point q in Open)
                {
                    if (dr(q, a) < r)
                    {
                        best = q;
                        r = dr(q, a);
                    }
                }
                Point p;
                var neigh = new List<Point>();
                p = new Point(best.X + 1, best.Y);
                if (!cell(p)) neigh.Add(p);
                p = new Point(best.X - 1, best.Y);
                if (!cell(p)) neigh.Add(p);
                p = new Point(best.X, best.Y + 1);
                if (!cell(p)) neigh.Add(p);
                p = new Point(best.X, best.Y - 1);
                if (!cell(p)) neigh.Add(p);
                Open.Remove(best);
                Close.Add(best);
                int xx = best.X - x0;
                int yy = best.Y - y0;
                if (xx >= 1 && xx <= xm &&
                 yy >= 1 && yy <= ym)
                {
                    can.FillRectangle(brush_all_path,
                     Left + 1 + (xx - 1) * d, Top + 1 + (yy - 1) * d,
                     d - 1, d - 1);
                    int dn = 2 * (int)Math.Sqrt(1 + n / 10);
                    if (n % dn == 0) Thread.Sleep(50);
                    {

                    }
                }
                foreach (Point next in neigh)
                {
                    if (!chain.ContainsKey(next))
                    {
                        chain.Add(next, best);
                    }
                    if (next == b)
                    {
                        found = true;
                        break;
                    }
                    if (Open.Contains(next) || Close.Contains(next))
                    {
                        continue;
                    }
                    Open.Add(next);
                }
                if (found)
                {
                    break;
                }
            }
            if (found)
            {
                all_path = Close;
                all_path.Remove(a);
                best_path = new List<Point>();
                Point p = chain[b];
                while (p != a)
                {
                    best_path.Add(p);
                    p = chain[p];
                }
                all_path = new List<Point>();
            }
            else
            {
                all_path = new List<Point>();
                best_path = new List<Point>();
                MessageBox.Show("Путь не найден!");
            }
            paint();
        }
        double dr(Point p, Point q)
        {
            return (p.X - q.X) * (p.X - q.X) + (p.Y - q.Y) * (p.Y - q.Y);
        }

        public void paint_path(Point p, int mode)
        {
            SolidBrush brush_path;
            if (mode == 1)
            {
                brush_path = brush_automaton;
                can.FillRectangle(brush_path,
                 Left + 1 + (p.X - 1) * d, Top + 1 + (p.Y - 1) * d,
                 d - 1, d - 1);
            }
            else if(mode == 2)
            {
                brush_path = brush_white;
                can.FillRectangle(brush_path,
                 Left + 1 + (p.X - 1) * d, Top + 1 + (p.Y - 1) * d,
                 d - 1, d - 1);
            }
            else if(mode == 3)
            {
                var rb = new SolidBrush(Color.Red);
                var ft = new Font("arial", d / 5);
                can.DrawString("Start", ft, rb,
                 Left + 1 + (p.X - 1) * d + d / 9,
                 Top + 1 + (p.Y - 1) * d + d / 11);
                rb.Dispose();
            }

        }

        public bool Start = false;

        // отрисовка лабиринта
        public void paint()
        {
            for (int x = 1; x <= xm; x++)
                for (int y = 1; y <= ym; y++)
                {
                    paint_cell(x, y);
                }


            foreach (Point p in all_path)
            {
                int x = p.X - x0;
                int y = p.Y - y0;
                if (x >= 1 && x <= xm &&
                 y >= 1 && y <= ym)
                {
                    paint_cell(x, y, 1);
                }
            }

            foreach (Point p in best_path)
            {
                int x = p.X - x0;
                int y = p.Y - y0;
                if (x >= 1 && x <= xm &&
                 y >= 1 && y <= ym)
                {
                    paint_cell(x, y, 2);
                }
            }

            

            if (best_path.Count >= 1)
            {
                int x = a.X - x0;
                int y = a.Y - y0;
                if (x >= 1 && x <= xm &&
                 y >= 1 && y <= ym)
                {
                    paint_cell(x, y, 2);
                }
                x = b.X - x0;
                y = b.Y - y0;
                if (x >= 1 && x <= xm &&
                 y >= 1 && y <= ym)
                {
                    paint_cell(x, y, 2);
                }

            }
            var b_vert = new LinearGradientBrush(new RectangleF(Left, Top, xm * d, ym * d), Color.Transparent, Color.Transparent, LinearGradientMode.Vertical);
            var b_horiz = new LinearGradientBrush(new RectangleF(Left, Top, xm * d, ym * d), Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal);

            var blendx = new ColorBlend();
            var blendy = new ColorBlend();

            int nm = 2 * xm;
            blendx.Positions = new float[nm + 1];
            blendx.Colors = new Color[nm + 1];
            for (int x = 0; x <= nm; x++)
            {
                blendx.Positions[x] = x * 1.0f / nm;
                if (x % 2 == 0)
                {
                    blendx.Colors[x] = Color.White;
                }
                else
                {
                    blendx.Colors[x] = Color.Black;
                }
            }

            nm = 2 * ym;
            blendy.Positions = new float[nm + 1];
            blendy.Colors = new Color[nm + 1];
            for (int x = 0; x <= nm; x++)
            {
                blendy.Positions[x] = x * 1.0f / nm;
                if (x % 2 == 0)
                {
                    blendy.Colors[x] = Color.White;
                }
                else
                {
                    blendy.Colors[x] = Color.Black;
                }
            }

            b_vert.InterpolationColors = blendy;
            b_horiz.InterpolationColors = blendx;

            Pen p_vert = new Pen(b_vert);
            Pen p_horiz = new Pen(b_horiz);
            p_vert.Width = 1;
            p_horiz.Width = p_vert.Width;

            for (int x = 1; x <= xm - 1; x++)
            {
                can.DrawLine(p_vert,
                 Left + x * d, Top,
                 Left + x * d, Top + ym * d);
            }
            for (int y = 1; y <= ym - 1; y++)
            {
                can.DrawLine(p_horiz,
                 Left, Top + y * d,
                 Left + xm * d, Top + y * d);
            }

            p_vert.Dispose();
            p_horiz.Dispose();
            b_vert.Dispose();
            b_horiz.Dispose();
        }

        // закрашивание отдельной клетки лабиринта
        public void paint_cell(int x, int y, int mode = 0)
        {
            SolidBrush brush;
            if (mode == 1)
            {
                brush = brush_all_path;
            }
            else if (mode == 2)
            {
                brush = brush_best_path;
            }
            else if (cell(x + x0, y + y0))
            {
                brush = brush_black;
            }
            else
            {
                brush = brush_white;
            }
            can.FillRectangle(brush,
             Left + 1 + (x - 1) * d, Top + 1 + (y - 1) * d,
             d - 1, d - 1);

            var p = new Point(x + x0, y + y0);
            if (p == a)
            {
                var rb = new SolidBrush(Color.Red);
                var ft = new Font("arial", d / 5);
                can.DrawString("A", ft, rb,
                 Left + 1 + (x - 1) * d + d / 9,
                 Top + 1 + (y - 1) * d + d / 11);
                rb.Dispose();
            }
            if (p == b)
            {
                var rb = new SolidBrush(Color.Red);
                var ft = new Font("arial", d / 5);
                can.DrawString("B", ft, rb,
                 Left + 1 + (x - 1) * d + d / 9,
                 Top + 1 + (y - 1) * d + d / 11);
                rb.Dispose();
            }
            if (p == start)
            {
                var rb = new SolidBrush(Color.Red);
                var ft = new Font("arial", d / 5);
                can.DrawString("Start", ft, rb,
                 Left + 1 + (x - 1) * d + d / 9,
                 Top + 1 + (y - 1) * d + d / 11);
                rb.Dispose();
            }
        }

        
        // конструктор лабиринта
        public lab()
        {
            init();
        }

        public void init()
        {
            brush_white = new SolidBrush(Color.White);
            brush_black = new SolidBrush(Color.Black);
            brush_all_path = new SolidBrush
             (form.rgb(255, 180, 75));
            brush_best_path = new SolidBrush
             (Color.LightBlue);
            brush_automaton = new SolidBrush(Color.LightGreen);
        }


        public void automaton()
        {
            Automaton automaton = new Automaton(new lab());
            savedAutomaton = automaton;
            /*automaton.SetInitState("q1");
            automaton.AddState("q2");
            automaton.AddNewTransition("q1", new AIO(15), "q2", new AIO(1));
            automaton.AddNewTransition("q2", new AIO(15), "q2", new AIO(2));
            automaton.AddNewTransition("q1", new AIO(0), "q1", new AIO(0));
            automaton.AddNewTransition("q1", new AIO(1), "q1", new AIO(1));*/
        }

    }
}
