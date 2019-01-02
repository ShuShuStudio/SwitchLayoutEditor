﻿using SwitchThemes.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SwitchThemes.Common.Custom;

namespace BflytPreview
{
    public partial class EditorView : Form
    {
        public BFLYT layout;

        double zoomFactor = 1.5;
        int prevZoom;
        int _bWidth;
        int _bHeight;

        public EditorView()
        {
            TypeDescriptor.AddAttributes(typeof(Vector3), new TypeConverterAttribute(typeof(Vector3Converter)));
            TypeDescriptor.AddAttributes(typeof(Vector2), new TypeConverterAttribute(typeof(Vector2Converter)));

            InitializeComponent();
            
            _bWidth = pictureBox1.Width;
            _bHeight = pictureBox1.Height;
            prevZoom = zoomSlider.Value;
        }

        /*private void openBFLYTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog() { Filter = "Binary layout file (*.bflyt)|*.bflyt" };
            if (opn.ShowDialog() != DialogResult.OK) return;
            layout = new BFLYT(File.ReadAllBytes(opn.FileName));
            UpdateView();
        }*/

        public void UpdateView()
        {
            treeView1.Nodes.Clear();
            RecursiveAddNode(layout.RootPane, treeView1.Nodes);
            RenderImg();
        }

        Bitmap b = new Bitmap(2000, 1000);

        void RenderImg()
        {
            using (Graphics gfx = Graphics.FromImage(b))
            {
                gfx.Clear(Color.LightGray);

                gfx.DrawRectangle(new Pen(Brushes.Red, 2), new Rectangle(0, 0, 1280, 720));

                Stack<Matrix> CurMatrix = new Stack<Matrix>();
                Random r = new Random();
                void RecursiveRenderPane(BFLYT.EditablePane p)
                {
                    if (!p.ParentVisibility)
                        return;
                    CurMatrix.Push(gfx.Transform.Clone());
                    gfx.TranslateTransform(p.Position.X, p.Position.Y);
                    gfx.RotateTransform(p.Rotation.Z);
                    gfx.ScaleTransform(p.Scale.X, p.Scale.Y);

                    Rectangle transformedRect = new Rectangle(p.transformedRect.x, p.transformedRect.y, p.transformedRect.width, p.transformedRect.height);

                    var pen = new Pen(Brushes.Black, 2);
                    var HighlightedPen = new Pen(Brushes.Red, 4);

                    if (p.ViewInEditor)
                    {
                        if (treeView1.SelectedNode != null && p == treeView1.SelectedNode.Tag as BFLYT.EditablePane)
                            pen = HighlightedPen;
                        gfx.DrawRectangle(pen, transformedRect);
                    }

                    foreach (var c in p.Children.Where(x => x is BFLYT.EditablePane))
                        RecursiveRenderPane((BFLYT.EditablePane)c);
                    gfx.Transform = CurMatrix.Pop();
                }

                gfx.ScaleTransform(1, -1);
                gfx.TranslateTransform(640, -360);
                RecursiveRenderPane((BFLYT.EditablePane)layout.RootPane);

            }
            pictureBox1.Image = b;
        }

        void RecursiveAddNode(BFLYT.BasePane p, TreeNodeCollection node)
        {
            var TargetNode = node.Add(p.ToString().Split(' ').Last());
            TargetNode.Tag = p;
            foreach (var c in p.Children)
                RecursiveAddNode(c, TargetNode.Nodes);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            RenderImg();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = treeView1.SelectedNode.Tag as BFLYT.EditablePane;
            RenderImg();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H)
            {
                var target = treeView1.SelectedNode.Tag as BFLYT.EditablePane;
                if (target == null) return;
                target.ViewInEditor = !target.ViewInEditor;
                RenderImg();
            }
        }

        private void saveBFLYTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog() { Filter = "Binary cafe layout (*.bflyt)|*.bflyt" };
            if (sav.ShowDialog() != DialogResult.OK) return;

            foreach (var p in layout.Panes.Where(x => x is BFLYT.EditablePane))
                ((BFLYT.EditablePane)p).ApplyChanges(layout.FileByteOrder);

            File.WriteAllBytes(sav.FileName, layout.SaveFile());
        }

        private void EditorView_Load(object sender, System.EventArgs e)
        {
        }

        private void bringToFront()
        {
            this.Activate();
            this.BringToFront();
            this.Focus();
        }

        private void EditorView_Click(object sender, System.EventArgs e)
        {
            bringToFront();
        }

        private void EditorView_Resize(object sender, System.EventArgs e)
        {
            pictureBox1.Size = panel1.Size;
            zoomSlider.Value = 5;
            bringToFront();
        }

        private void zoomSlider_Scroll(object sender, EventArgs e)
        {
            _bWidth = pictureBox1.Width;
            _bHeight = pictureBox1.Height;
            if (zoomSlider.Value > prevZoom)
            {
                _bWidth = (int)(_bWidth * zoomFactor);
                _bHeight = (int)(_bHeight * zoomFactor);
            }
            else if (zoomSlider.Value < prevZoom)
            {
                _bWidth = (int)(_bWidth / zoomFactor);
                _bHeight = (int)(_bHeight / zoomFactor);
            }
            pictureBox1.Width = _bWidth;
            pictureBox1.Height = _bHeight;
            prevZoom = zoomSlider.Value;
        }
    }
}