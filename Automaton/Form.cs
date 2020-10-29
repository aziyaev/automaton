using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automaton
{
    [Serializable]
    public class form : Form
    {
        // создать новую кнопку с текстом Text
        void Button(float x, float y, string Text,
         bool center = false)
        {
            float dx = can.MeasureString(Text, Font).ToSize().Width;
            if (center)
            {
                x -= dx / 2;
            }
            float e = (float)1.25 * d;

            RectangleF r;

            if (!but_name.Contains(Text))
            {
                Rectangle r0 = new Rectangle(Convert.ToInt32(x), Convert.ToInt32(y),
                 Convert.ToInt32(dx + 0.25 * d), Convert.ToInt32(e));
                but.Add(r0);
                but_name.Add(Text);
            }

            if (adjust) return;

            Pen pen = new Pen(Color.Black, 2);
            SolidBrush brush = new SolidBrush(Color.LightBlue);
            float t = e;
            r = new RectangleF((float)(x + 0.5 * t), y,
             (float)(dx + 0.25 * d - t), e);
            can.FillRectangle(brush, r);

            r = new RectangleF(x, y, t, e);
            can.FillEllipse(brush, r);
            r = new RectangleF(x + dx + 0.25f * d - t, y, t, e);
            can.FillEllipse(brush, r);
            brush.Color = Color.Black;
            can.DrawString(Text, Font, brush,
             Convert.ToInt32(x + 0.15 * d), Convert.ToInt32(y + 0.07 * d));
            pen.Dispose();
            brush.Dispose();
        }

        // создать кнопку увеличить/уменьшить масштаб
        void button_zoom(float x, float y, bool plus)
        {
            float dx = 1.25f * d;
            Rectangle r;

            if (!but_name.Contains(Text))
            {
                r = new Rectangle(Convert.ToInt32(x), Convert.ToInt32(y),
                 Convert.ToInt32(dx), Convert.ToInt32(dx));
                but.Add(r);
                if (plus)
                {
                    but_name.Add("+");
                }
                else
                {
                    but_name.Add("-");
                }
            }

            if (adjust) return;

            Pen pen = new Pen(Color.Black, 2);

            float e = 0.2f * dx;
            float h = 0.11f * dx;
            SolidBrush brush = new SolidBrush(Color.Red);

            can.FillRectangle(brush, x + h, y + 0.5f * dx - 0.5f * e, dx - 2 * h, e);
            if (plus)
                can.FillRectangle(brush, x + 0.5f * dx - 0.5f * e, y + h, e, dx - 2 * h - 1);

            pen.Dispose();
            brush.Dispose();
        }

        // вывести текст (подсказку) в statusbar
        public void paint_status(bool Clear = false)
        {
            if (!Clear && last_status == status) return;
            last_status = status;
            SolidBrush brush = new SolidBrush(tabc.BackColor);
            can.FillRectangle(brush, 0, tabc.Bottom + 1, w, 2 * d);
            brush.Color = Color.Black;
            can.DrawString(status, Font, brush,
             Convert.ToInt32(0.6 * d), tabc.Bottom + d / 6);
            brush.Dispose();
        }

        // отрисовка окна
        public void paint(object sen, EventArgs e)
        {
            if (!adjust)
            {
                remove_focus();

                SolidBrush brush = new SolidBrush(BackColor);
                can.FillRectangle(brush, 0, 0, w, h);
                brush.Dispose();
            }

            if (adjust)
            {
                but_name.Clear();
                but.Clear();
            }

            float yb = (float)0.4 * d;
            Button((float)0.5 * d, yb, "Создать");
            Button(but[0].Right + d, yb, "Открыть");
            Button(but[1].Right + d, yb, "Сохранить");
            Button(but[2].Right + d, yb, "Переименовать");
            button_zoom(but[3].Right + d, yb, true);
            button_zoom(but[4].Right + d, yb, false);
            Button(but[6 - 1].Right + d, yb, "Закрыть");

            float xb = (float)(tabc.Width + 4.3 * d);
            float x0 = (float)2.5 * d;
            float dx = (float)2 * d;
            Button(xb, x0, "Закрасить белым", true);
            Button(xb, x0 + dx, "Закрасить черным", true);
            Button(xb, x0 + 2 * dx, "Левая граница", true);
            Button(xb, x0 + 3 * dx, "Правая граница", true);
            Button(xb, x0 + 4 * dx, "Верхняя граница", true);
            Button(xb, x0 + 5 * dx, "Нижняя граница", true);
            Button(xb, x0 + 6 * dx, "Точка A", true);
            Button(xb, x0 + 7 * dx, "Точка B", true);
            Button(xb, x0 + 8 * dx, "Найти путь", true);
            Button(xb, x0 + 9 * dx, "Очистить", true);
            Button(xb, x0 + 10 * dx, "Установить автомат", true);
            Button(xb, x0 + 11 * dx, "Старт", true);
            Button(xb, x0 + 12 * dx, "Стоп", true);
            Button(xb, x0 + 13 * dx, "Указать таблицу", true);

            if (!adjust)
            {
                paint_status(true);
            }
        }

        // включить/выключить подсветку кнопки
        void highlight(Rectangle r, bool on)
        {
            int g = 2;
            Rectangle r0 = new Rectangle(r.Left - g, r.Top - g, r.Width + 2 * g + 1, r.Height + 2 * g);
            Pen pen = new Pen(rgb(100, 100, 100), g);
            if (on) pen.Color = rgb(200, 200, 200);
            else pen.Color = BackColor;
            can.DrawRectangle(pen, r0);
            pen.Dispose();
        }

        // событие перемещение мышки над основным окном
        void move(object sen, MouseEventArgs e)
        {
            // подсветка кнопок
            int n = 0;
            if (select_plane)
            {
                status = "Выберите клетку, через которую проходит полуплоскость";
            }
            else if (select_a)
            {
                status = "Установите точку A";
            }
            else if (select_b)
            {
                status = "Установите точку B";
            }
            else if (select_automaton)
            {
                status = "Установите автомат";
            }
            else
            {
                status = "";
            }
            foreach (Rectangle r in but)
            {
                n++;
                bool above = r.Contains(e.X, e.Y);
                if (n == 5 || n == 6)
                {
                    highlight(r, above);
                }
                if (above)
                {
                    switch (n)
                    {
                        case 1: status = "Создать новый лабиринт"; break;
                        case 2: status = "Загрузить лабиринт из файла"; break;
                        case 3: status = "Сохранить текущий лабиринт в файл"; break;
                        case 4: status = "Переименовать текущую вкладку"; break;
                        case 5: status = "Увеличить"; break;
                        case 6: status = "Уменьшить"; break;
                        case 7: status = "Закрыть текущую вкладку"; break;
                        case 8:
                        case 9:
                        case 10:
                        case 11: return;
                        /*
                        case 12 : status = "Установить начальную точку пути"; break;
                        case 13 : status = "Установить конечную точку пути"; break;
                        */
                        case 16:
                            {
                                page tab = (page)tabc.SelectedTab;
                                if (tab.lab.check_path())
                                {
                                    status = "Нажмите для начала поиска пути";
                                }
                                else if (tab.lab.cell(tab.lab.a))
                                {
                                    status = "Начальная точка должна быть в пустой клетке!";
                                }
                                else if (tab.lab.cell(tab.lab.b))
                                {
                                    status = "Конечная точка должна быть в пустой клетке!";
                                }
                                else
                                {
                                    status = "Сначала установите точку A и точку B";
                                }
                                break;
                            }
                        case 17: status = "Очистить результат поиска"; break;
                        case 18: status = "Установить начальное положение"; break;
                        case 19:
                            {
                                page tab = (page)tabc.SelectedTab;
                                if (tab.lab.cell(tab.lab.start))
                                {
                                    status = "Установите старт в пустой клетке!";
                                }
                                else
                                {
                                    status = "Нажмите для старта";
                                }
                                break;
                            }
                        case 21: status = "Указать путь к файлу с таблицей .txt"; break;


                    }
                }
            }
            paint_status();
        }

        // щелчок мышкой по заголовку вкладки
        void tab_click(object sen, MouseEventArgs e)
        {
            remove_focus();

            drag = 0;
            for (int n = 1; n <= tabc.TabCount; n++)
            {
                if (tabc.GetTabRect(n - 1).Contains
                 (e.Location))
                {
                    drag = n;
                    break;
                }
            }

            status = "Перетащите на другую вкладку для объединения/пересечения";
            paint_status();
        }

        // конец перетаскивания вкладок
        void tab_mouse_up(object sen, MouseEventArgs e)
        {
            if (drag <= 0) return;
            int src = drag;
            drag = 0;
            status = "";
            paint_status();

            int dest = 0;
            for (int n = 1; n <= tabc.TabCount; n++)
            {
                if (tabc.GetTabRect(n - 1).Contains
                 (e.Location))
                {
                    dest = n;
                    break;
                }
            }

            if (dest == 0) return;
            if (src == dest) return;

            oper = 0;
            var q = new quest(this);
            q.ShowDialog();
            if (oper == 0) return;
            var tab_src = (page)tabc.TabPages[src - 1];
            var tab_dest = (page)tabc.TabPages[dest - 1];
            switch (oper)
            {
                case 1: tab_src.lab.unify(tab_dest.lab, this); break;
                case 2: tab_src.lab.intercept(tab_dest.lab, this); break;
            }
        }

        // щелчок мышкой по окну
        void click(object sen, MouseEventArgs e)
        {
            int n = 0;
            foreach (Rectangle r in but)
            {
                n++;
                if (r.Contains(e.X, e.Y))
                {
                    press(n);
                    return;
                }
            }
        }
        public Automaton savedAutomaton;
        public bool _switch = true;
        public System.Timers.Timer tmr = new System.Timers.Timer();


        // нажатие на кнопку
        void press(int n)
        {
            page tab = (page)tabc.SelectedTab;
            Tab = tab;
            Point s = tab.lab.start;
            savedAutomaton = tab.lab.savedAutomaton;

            List<Point> path_A = new List<Point>();
            switch (n)
            {
                case 1: tab.create(); break;
                case 2: tab.Open(); break;
                case 3: tab.save(); break;
                case 4: tab.rename(); break;
                case 5: tab.plus(true); break;
                case 6: tab.plus(false); break;
                case 7: tab.Close(); break;
                case 8: tab.lab.Clear(false); break;
                case 9: tab.lab.Clear(true); break;
                case 10:
                    {
                        select_plane = true;
                        kind = edge.Left;
                        status = "Выберите клетку, через которую проходит полуплоскость";
                        paint_status();
                        break;
                    }
                case 11:
                    {
                        select_plane = true;
                        kind = edge.Right;
                        status = "Выберите клетку, через которую проходит полуплоскость";
                        paint_status();
                        break;
                    }
                case 12:
                    {
                        select_plane = true;
                        kind = edge.Top;
                        status = "Выберите клетку, через которую проходит полуплоскость";
                        paint_status();
                        break;
                    }
                case 13:
                    {
                        select_plane = true;
                        kind = edge.Bottom;
                        status = "Выберите клетку, через которую проходит полуплоскость";
                        paint_status();
                        break;
                    }
                case 14:
                    {
                        select_a = true;
                        status = "Установите точку A";
                        paint_status();
                        break;
                    }
                case 15:
                    {
                        select_b = true;
                        status = "Установите точку B";
                        paint_status();
                        break;
                    }
                case 16: tab.lab.find_path(); break;
                case 17:
                    {
                        tab.lab.best_path =
                         new List<Point>();
                        tab.lab.all_path =
                         new List<Point>();
                        tab.lab.a.X = 10000;
                        tab.lab.a.Y = 10000;
                        tab.lab.b.X = 10000;
                        tab.lab.b.Y = 10000;
                        tab.lab.paint();
                        break;
                    }
                case 18:
                    {
                        select_automaton = true;
                        automaton_check = true;
                        tab.lab.Start = true;
                        status = "Установите автомат";
                        paint_status();
                        break;
                    }
                case 19:
                    {
                        if (!automaton_check) MessageBox.Show("Установите автомат на плоскости", "Ошибка", MessageBoxButtons.OK);
                        else if (!table_check) MessageBox.Show("Задайте таблицу!", "Ошибка", MessageBoxButtons.OK);
                        else
                        {
                            tmr.Interval = 500;
                            tmr.Elapsed += Tmr_Elapsed;
                            tmr.Start();
                        }
                        break;
                    }
                case 20:
                    {
                        tmr.Stop();
                        break;
                    }
                case 21:
                    {
                        if (!automaton_check) MessageBox.Show("Установите автомат на плоскости", "Ошибка", MessageBoxButtons.OK);
                        else
                        {
                            TableForm table = new TableForm(this);
                            table.Show();
                            this.table_check = table.table_check;
                        }

                        break;
                    }

            }
        }

        private page Tab;


        private void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Tab.lab.Start = false;
            bool loop = true;
            Point s = Tab.lab.start;
            Tab.lab.paint_path(s, 2);
            Point p;
            int input = 0;
            p = new Point(s.X, s.Y + 1);
            if (!Tab.lab.cell(p)) input += 4;
            p = new Point(s.X + 1, s.Y);
            if (!Tab.lab.cell(p)) input += 2;
            p = new Point(s.X, s.Y - 1);
            if (!Tab.lab.cell(p)) input += 1;
            p = new Point(s.X - 1, s.Y);
            if (!Tab.lab.cell(p)) input += 8;
            var output = Tab.lab.savedAutomaton.Move(input);
            Tab.lab.start = output.Item1;
            loop = output.Item2;
            s = Tab.lab.start;
            Tab.lab.paint_path(Tab.lab.start, 1);
        }

        public bool table_check = false;
        public bool automaton_check = false;
        public bool select_automaton = false;
        public static Color rgb
         (int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }



        // размеры окна
        int w = 1000;
        int h = 800;

        // размер шрифта
        public static int d = 15;
        //public static int d = 4;

        // кнопки
        List<Rectangle> but = new List<Rectangle>();
        List<string> but_name = new List<string>();

        // текущий текст (подсказка) в статусбаре
        public string status;
        string last_status;

        // номер перетаскиваемой вкладки
        int drag = 0;
        // вид операции, 1 - объединение, 2 - пересечение
        public int oper = 0;

        // выбор клетки, через которую проходит граница полуплоскости
        public bool select_plane = false;
        // вид полуплоскости
        public edge kind;

        // выбор точки A
        public bool select_a = false;
        // выбор точки B
        public bool select_b = false;

        // табы
        public TabControl tabc;
        page[] tab;
        // канвас для рисования на основном окне
        Graphics can;

        bool adjust = false;

        // конструктор окна
        public form()
        {
            Font = new Font(Font.Name, d);
            d = Font.Height;
            StartPosition =
             FormStartPosition.CenterScreen;
            MinimumSize = new Size(600, 600);
            ClientSize = new Size(w, h);
            BackColor = rgb(0, 0, 120);

            tabc = new TabControl();
            tabc.MouseDown += new MouseEventHandler(tab_click);
            tabc.MouseUp += new MouseEventHandler(tab_mouse_up);
            Controls.Add(tabc);

            tab = new page[2];
            for (int n = 1; n <= 2; n++)
            {
                tab[n - 1] = new page(this, n);
                //tab[n-1].BackColor = Color.Green;
                tab[n - 1].Text = "Лабиринт " + n.ToString();
                tabc.TabPages.Add(tab[n - 1]);
            }

            Resize += new System.EventHandler(resize);
            Paint += new PaintEventHandler(paint);
            MouseMove += new MouseEventHandler(move);
            MouseDown += new MouseEventHandler(click);

            can = CreateGraphics();
            resize(null, null);
        }

        // изменение размера окна мышкой
        public void resize(object sen, EventArgs e)
        {
            w = ClientSize.Width;
            h = ClientSize.Height;
            tabc.Width = w - 9 * d;
            tabc.Height = h - (3 * d + d / 4);
            tabc.Top = 2 * d;

            foreach (page tab in tabc.TabPages)
            {
                if (tab.lab.d == 0) tab.lab.d = d;
                int arrow = 2 * d;
                tab.arrow = arrow;
                int xm = (tabc.Width - d) / tab.lab.d;
                int ym = (tabc.Height - tabc.GetTabRect(0).Height - d - 2 * arrow) / tab.lab.d;
                tab.lab.Left = d / 2;
                tab.lab.Top = d / 2;
                tab.lab.xm = xm;
                tab.lab.ym = ym;
                tab.ClientSize = new Size(
                 d / 2 + xm * tab.lab.d,
                 d / 2 + ym * tab.lab.d + 2 * arrow);
                tab.a_left = new RectangleF(
                 d / 2 + xm * tab.lab.d / 2 - 3 * arrow / 2,
                 d / 2 + ym * tab.lab.d + arrow / 2,
                 arrow, arrow);
                tab.a_right = new RectangleF(
                 d / 2 + xm * tab.lab.d / 2 + arrow / 2,
                 d / 2 + ym * tab.lab.d + arrow / 2,
                 arrow, arrow);
                tab.a_up = new RectangleF(
                 d / 2 + xm * tab.lab.d / 2 - arrow / 2,
                 d / 2 + ym * tab.lab.d + 1,
                 arrow, arrow);
                tab.a_down = new RectangleF(
                 d / 2 + xm * tab.lab.d / 2 - arrow / 2,
                 d / 2 + ym * tab.lab.d + arrow + 1,
                 arrow, arrow);
                if (tab.lab.can != null)
                {
                    tab.lab.can.Dispose();
                }
                tab.lab.can = tab.CreateGraphics();
                tab.lab.f = this;
            }
            if (can != null) can.Dispose();
            can = CreateGraphics();

            adjust = true;
            paint(null, null);
            adjust = false;
        }

        // убрать рамку фокуса с заголовков табов
        void remove_focus()
        {
            tabc.SelectedTab.Focus();
        }

    }

    class quest : Form
    {

        form form;

        public quest(form f)
        {
            form = f;
            Font = new Font(form.Font.Name,
             form.Font.Size);
            int d = Font.Height;
            StartPosition =
             FormStartPosition.CenterScreen;

            var cap = new Label();
            cap.Text = "Выберите операцию над лабиринтами:";
            cap.AutoSize = true;
            cap.Top = d;
            cap.Left = d;
            Controls.Add(cap);

            int w = cap.Right + d;

            var but = new Button[3];
            for (int n = 1; n <= 3; n++)
            {
                but[n - 1] = new Button();
                but[n - 1].AutoSize = true;
                but[n - 1].Top = cap.Bottom + d;
                but[n - 1].Tag = n;
                but[n - 1].Click +=
                 new System.EventHandler(click);
                Controls.Add(but[n - 1]);
            }

            but[0].Text = "Объединение";
            but[1].Text = "Пересечение";
            but[2].Text = "Отмена";

            int s = 0;
            for (int n = 1; n <= 3; n++)
            {
                s += but[n - 1].Width;
            }
            but[0].Left = d / 2;
            but[1].Left = but[0].Right + (w - d - s) / 2;
            but[2].Left = but[1].Right + (w - d - s) / 2;

            ClientSize = new Size(w, but[1].Bottom + d / 2);
        }

        void click(object sen, EventArgs e)
        {
            int Tag = (int)((Button)(sen)).Tag;
            switch (Tag)
            {
                case 1: form.oper = 1; break;
                case 2: form.oper = 2; break;
                case 3: form.oper = 0; break;
            }
            Close();
        }

    }

}
