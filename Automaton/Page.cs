using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automaton
{
    [Serializable]
    class page : TabPage
    {

        // лабиринт, отображаемый на вкладке
        public lab lab;

        // размер стрелок навикации
        public int arrow = 2 * form.d;
        // область стрелок навигации
        public RectangleF a_left, a_right, a_up, a_down;
        System.Windows.Forms.Timer time;
        RectangleF arrow_kind;


        int last_x = 0;
        int last_y = 0;

        // создать новую вкладку
        public void create()
        {
            TabControl tabc = form.tabc;
            page tab = new page(form);
            tabc.TabPages.Insert(tabc.TabCount, tab);
            tabc.SelectedTab = tab;
            tab.Text = "Лабиринт";

            tab.lab.f = form;
            tab.lab.init();
            form.resize(null, null);
        }

        // переименовать текущую вкладку
        public void rename()
        {
            string name = input("Введите новое имя: ");
            if (name == "") return;
            form.tabc.SelectedTab.Text = name;
        }

        // открыть файл с лабиринтом
        public void Open() // открыть файл с лабиринтом
        {
            var Open = new OpenFileDialog();
            if (Open.ShowDialog() != DialogResult.OK) return;
            string path = Open.FileName;
            string name = Path.GetFileName(path);

            TabControl tabc = form.tabc;
            page tab = new page(form);
            tabc.TabPages.Insert(tabc.TabCount, tab);
            tabc.SelectedTab = tab;
            tab.Text = name;

            var bin = new BinaryFormatter();
            var file = new FileStream(path, FileMode.Open);
            tab.lab = (lab)bin.Deserialize(file);
            file.Close();

            tab.lab.f = form;
            tab.lab.init();
            form.resize(null, null);
        }

        // сохранить лабиринт в файл
        public void save()
        {
            var save = new SaveFileDialog();
            if (save.ShowDialog() != DialogResult.OK) return;
            string path = save.FileName;
            string name = Path.GetFileName(path);
            Text = name;

            var bin = new BinaryFormatter();
            var file = new FileStream(path, FileMode.Create);
            bin.Serialize(file, lab);
            file.Close();
        }

        // увеличить/уменьшить размер клеток в лабиринте
        public void plus(bool increase)
        {
            if (increase)
            {
                if (lab.d <= 100) lab.d += 4;
            }
            else
            {
                if (lab.d >= 10) lab.d -= 4;
            }
            form.resize(null, null);
            Refresh();
        }

        public void Close()
        {
            if (form.tabc.TabCount >= 2)
            {
                form.tabc.TabPages.Remove(form.tabc.SelectedTab);
            }
        }

        public string input(string Text)
        {
            Form f = new Form();
            f.Font = new Font(form.Font.Name,
             form.Font.Size);
            int d = f.Font.Height;
            f.StartPosition =
             FormStartPosition.CenterScreen;

            var cap = new Label();
            cap.Text = Text;
            cap.AutoSize = true;
            cap.Top = d;
            cap.Left = d;
            f.Controls.Add(cap);

            int w = cap.Right + d;

            var box = new TextBox();
            box.AutoSize = true;
            box.Left = cap.Left + d / 2;
            box.Top = cap.Bottom + d;
            box.Width = 7 * d;
            f.Controls.Add(box);

            var but = new Button[2];
            for (int n = 1; n <= 2; n++)
            {
                but[n - 1] = new Button();
                but[n - 1].AutoSize = true;
                but[n - 1].Top = box.Bottom + d;
                f.Controls.Add(but[n - 1]);
            }

            but[0].Text = "ОК";
            but[1].Text = "Отмена";

            but[0].DialogResult = DialogResult.OK;
            but[1].DialogResult = DialogResult.Cancel;

            int s = 0;
            for (int n = 1; n <= 2; n++)
            {
                s += but[n - 1].Width;
            }
            but[0].Left = d;
            but[1].Left = w - d - but[1].Width;

            f.ClientSize = new Size(w, but[1].Bottom + d / 2);

            f.AcceptButton = but[0];
            f.CancelButton = but[1];

            DialogResult r = f.ShowDialog();
            if (r == DialogResult.OK)
            {
                return box.Text;
            }
            else
            {
                return "";
            }
        }

        // отрисовка вкладки
        public void paint(object sen, EventArgs e)
        {
            if (ClientSize.Height != form.d / 2 + lab.ym * form.d) form.resize(null, null);

            lab.paint();
            paint_arrows();
        }

        // отрисовка стрелок навигации
        void paint_arrows()
        {
            var can = lab.can;
            var brush = new SolidBrush(Color.Green);
            RectangleF a;
            a = a_left;
            can.FillPolygon(brush, new PointF[] {
                 new PointF(
                  a.Left,
                  a.Top+a.Height/2),
                 new PointF(
                  a.Right-a.Width/4,
                  a.Top),
                 new PointF(
                  a.Right-a.Width/4,
                  a.Top+a.Height)
                 });
            a = a_right;
            can.FillPolygon(brush, new PointF[] {
                 new PointF(
                  a.Right,
                  a.Top+a.Height/2),
                 new PointF(
                  a.Left+a.Width/4,
                  a.Top),
                 new PointF(
                  a.Left+a.Width/4,
                  a.Top+a.Height)
                 });
            a = a_up;
            can.FillPolygon(brush, new PointF[] {
                 new PointF(
                  a.Left+a.Width/2,
                  a.Top),
                 new PointF(
                  a.Left,
                  a.Bottom-a.Height/4),
                 new PointF(
                  a.Right,
                  a.Bottom-a.Height/4)
                 });
            a = a_down;
            can.FillPolygon(brush, new PointF[] {
                 new PointF(
                  a.Left+a.Width/2,
                  a.Bottom),
                 new PointF(
                  a.Left,
                  a.Top+a.Height/4),
                 new PointF(
                  a.Right,
                  a.Top+a.Height/4)
                 });

            brush.Dispose();
        }

        void Tick(object sen, EventArgs e)
        {
            int dx, dy;
            if (lab.xm <= 20) dx = 1;
            else dx = lab.xm / 10;
            if (lab.ym <= 20) dy = 1;
            else dy = lab.ym / 10;

            if (arrow_kind == a_left)
            {
                lab.x0 -= dx;
            }
            else if (arrow_kind == a_right)
            {
                lab.x0 += dx;
            }
            else if (arrow_kind == a_up)
            {
                lab.y0 -= dy;
            }
            else if (arrow_kind == a_down)
            {
                lab.y0 += dy;
            }
            lab.paint();
        }

        // щелчок мышкой по вкладке
        void click(object sen, MouseEventArgs e)
        {
            if (a_left.Contains(e.Location))
            {
                time.Enabled = true;
                arrow_kind = a_left;
                Tick(null, null);
                return;
            }
            if (a_right.Contains(e.Location))
            {
                time.Enabled = true;
                arrow_kind = a_right;
                Tick(null, null);
                return;
            }
            if (a_up.Contains(e.Location))
            {
                time.Enabled = true;
                arrow_kind = a_up;
                Tick(null, null);
                return;
            }
            if (a_down.Contains(e.Location))
            {
                time.Enabled = true;
                arrow_kind = a_down;
                Tick(null, null);
                return;
            }

            if (!form.select_plane && !form.select_a && !form.select_b && !form.select_automaton)
            {
                start = true;
            }

            int x = 1 + (e.X - lab.Left) / lab.d;
            int y = 1 + (e.Y - lab.Top) / lab.d;
            last_x = x;
            last_y = y;

            if (x < 1 || x > lab.xm ||
             y < 1 || y > lab.ym) return;

            if (form.select_plane)
            {
                if (form.kind == edge.Left || form.kind == edge.Right)
                {
                    lab.add_plane(x + lab.x0, form.kind);
                }
                if (form.kind == edge.Top || form.kind == edge.Bottom)
                {
                    lab.add_plane(y + lab.y0, form.kind);
                }
                form.select_plane = false;
                lab.paint();
                form.status = "";
                form.paint_status();
                form.select_plane = false;
                lab.paint();
                form.status = "";
                form.paint_status();
            }
            else if (form.select_a)
            {
                lab.a = new Point(x + lab.x0, y + lab.y0);
                form.select_a = false;
                lab.paint();
                form.status = "";
                form.paint_status();
            }
            else if (form.select_b)
            {
                lab.b = new Point(x + lab.x0, y + lab.y0);
                form.select_b = false;
                lab.paint();
                form.status = "";
                form.paint_status();
            }
            else if (form.select_automaton) //создали точку старта с координатами
            {
                lab.start = new Point(x + lab.x0, y + lab.y0);
                lab.paint_path(lab.start, 3);
                form.select_automaton = false;
                lab.automaton();
                lab.savedAutomaton.SetPosition(lab.start);
                lab.paint();
                form.status = "";
                form.paint_status();
            }
            else
            {
                lab.setcell(x + lab.x0, y + lab.y0, !lab.cell(x + lab.x0, y + lab.y0));
                lab.paint_cell(x, y);
                //form.status = "Для отмены ...";
                //form.paint_status();
            }
        }

        // событие отпустить кнопку мышки над вкладкой
        void mouse_up(object sen, MouseEventArgs e)
        {
            start = false;
            time.Enabled = false;
        }

        // событие перемещение мышки над вкладкой
        void move(object sen, MouseEventArgs e)
        {
            if (!start) return;
            int x = 1 + (e.X - lab.Left) / lab.d;
            int y = 1 + (e.Y - lab.Top) / lab.d;
            if (x == last_x && y == last_y) return;
            last_x = x;
            last_y = y;

            if (x < 1 || x > lab.xm ||
             y < 1 || y > lab.ym) return;
            lab.setcell(x + lab.x0, y + lab.y0, !lab.cell(x + lab.x0, y + lab.y0));
            lab.paint_cell(x, y);
        }

        // размеры вкладки
        int w, h;

        // канвас для рисования на вкладке
        Graphics can;
        // ссылка на основное окно
        public form form;

        // начало рисования стенок лабиринта
        bool start = false;

        // конструктор вкладки
        public page(form f, int j = 0)
        {
            form = f;

            Paint += new PaintEventHandler(paint);
            MouseDown += new MouseEventHandler(click);
            MouseUp += new MouseEventHandler(mouse_up);
            MouseMove += new MouseEventHandler(move);

            time = new System.Windows.Forms.Timer();
            time.Interval = 100;
            time.Enabled = false;
            time.Tick += Tick;

            lab = new lab();
            if (j == 0) return;

            if (j == 1)
            {
                for (int x = 3; x <= 11; x++)
                    for (int y = 3; y <= 10; y++)
                    {
                        lab.setcell(x, y, false);
                    }
                for (int y = 4; y <= 9; y++)
                {
                    lab.setcell(7, y, true);
                }
                lab.a = new Point(3, 6);
                lab.b = new Point(11, 6);
            }
            else if (j == 2)
            {
                for (int x = 3; x <= 11; x++)
                    for (int y = 6; y <= 10; y++)
                    {
                        lab.setcell(x, y, false);
                    }
                for (int y = 4; y <= 8; y++)
                {
                    lab.setcell(4, y, true);
                }
                for (int y = 4; y <= 8; y++)
                {
                    lab.setcell(6, y, true);
                }
                for (int y = 3; y <= 8; y++)
                {
                    lab.setcell(8, y, true);
                }
                for (int y = 4; y <= 8; y++)
                {
                    lab.setcell(10, y, true);
                }
                lab.setcell(3, 7, true);
                lab.setcell(7, 8, true);
                lab.setcell(11, 7, true);
                lab.a = new Point(3, 6);
                lab.b = new Point(11, 6);
            }

        }

    }
}
