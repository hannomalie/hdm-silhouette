﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Silhouette.Engine;
using Silhouette.GameMechs;
using System.IO;
using Silhouette.GameMechs.Events;
using System.Drawing.Design;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace SilhouetteEditor.Forms
{
    public partial class MainForm : Form
    {
        /* Sascha:
         * Hauptfenster der Anwendung. Enthält eine TreeView zur Anzeige und hierarchischen Ordnung von Ebenen und Objekten.
         * Ein PropertyGrid zeigt Eigenschaften an und stellt Funktionen zur Anpassung bereit.
         * In die MainForm integriert ist eine PictureView, in der die grafische Darstellung über XNA erfolgt.
        */
        public static MainForm Default;

        public MainForm()
        {
            Default = this;
            InitializeComponent();
            TypeDescriptor.AddAttributes(typeof(LevelObject), new EditorAttribute(typeof(LevelObjectUITypeEditor), typeof(UITypeEditor)));
        }
 
        public IntPtr getDrawSurface()
        {
            return GameView.Handle;
        }

        //---> Form-Steuerung <---//

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save the current level beforce closing?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                FileSave(sender, e);
                EditorLoop.EditorLoopInstance.Exit();
            }
            else if (result == DialogResult.No)
                EditorLoop.EditorLoopInstance.Exit();
        }

        //---> MenuBar-Steuerung <---//

        private void FileExit(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save the current level beforce closing?", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                FileSave(sender, e);
                EditorLoop.EditorLoopInstance.Exit();
            }
            else if (result == DialogResult.No)
                EditorLoop.EditorLoopInstance.Exit();
            else
                return;
        }

        private void FileNew(object sender, EventArgs e)
        {
            new NewLevel().ShowDialog();
        }

        private void HelpAbout(object sender, EventArgs e)
        {
            new About().Show();
        }

        private void HelpHelp(object sender, EventArgs e)
        {
            new Help().Show();
        }

        private void FileSaveAs(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Editor.Default.levelFileName = dialog.FileName;
                Editor.Default.SaveLevel(dialog.FileName);
            }
        }

        private void FileSave(object sender, EventArgs e)
        {
            if (Editor.Default.levelFileName != null)
            {
                Editor.Default.SaveLevel(Editor.Default.levelFileName);
            }
            else
            {
                FileSaveAs(sender, e);
            }
        }

        private void FileOpen(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Editor.Default.LoadLevel(dialog.FileName);
            }
        }

        private void ToolsEventManager(object sender, EventArgs e)
        {
            new ManageEvents().ShowDialog();
        }

        //---> TreeView-Steuerung <---//

        public void UpdateTreeView()
        {
            treeView1.Nodes.Clear();

            TreeNode levelTreeNode = treeView1.Nodes.Add(Editor.Default.level.name);
            levelTreeNode.Tag = Editor.Default.level;
            levelTreeNode.Checked = Editor.Default.level.isVisible;
            levelTreeNode.ContextMenuStrip = LevelContextMenu;

            foreach (Layer l in Editor.Default.level.layerList)
            {
                TreeNode layerTreeNode = levelTreeNode.Nodes.Add(l.name);
                layerTreeNode.Tag = l;
                layerTreeNode.Checked = l.isVisible;
                layerTreeNode.ContextMenuStrip = LayerContextMenu;

                foreach (LevelObject lo in l.loList)
                {
                    TreeNode loTreeNode = layerTreeNode.Nodes.Add(lo.name);
                    loTreeNode.Tag = lo;
                    loTreeNode.Checked = lo.isVisible;
                    if (lo is Event)
                        loTreeNode.ContextMenuStrip = EventContextMenü;
                    else
                        loTreeNode.ContextMenuStrip = ObjectContextMenu;
                }
            }
            levelTreeNode.Expand();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                Editor.Default.selectLevel();
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                Editor.Default.selectLayer(l);
            }
            if (e.Node.Tag is LevelObject)
            {
                LevelObject lo = (LevelObject)e.Node.Tag;
                Editor.Default.selectLevelObject(lo);
                Camera.Position = lo.position;
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);

                if (treeView1.SelectedNode == null)
                    return;

                if (treeView1.SelectedNode.Tag is Layer)
                {
                    Layer l = (Layer)treeView1.SelectedNode.Tag;
                    Editor.Default.selectLayer(l);
                }
                if (treeView1.SelectedNode.Tag is LevelObject)
                {
                    LevelObject lo = (LevelObject)treeView1.SelectedNode.Tag;
                    Editor.Default.selectLevelObject(lo);
                    Camera.Position = lo.position;
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null) return;

            TreeNode[] nodes = treeView1.Nodes.Find(e.Label, true);
            if (nodes.Length > 0)
            {
                MessageBox.Show("A layer or object with the name \"" + e.Label + "\" already exists in the level. Please use another name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.CancelEdit = true;
                return;
            }
            if (e.Node.Tag is Level)
            {
                Level l = (Level)e.Node.Tag;
                l.name = e.Label;
                e.Node.Name = e.Label;
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                l.name = e.Label;
                e.Node.Name = e.Label;
            }
            if (e.Node.Tag is LevelObject)
            {
                LevelObject i = (LevelObject)e.Node.Tag;
                i.name = e.Label;
                e.Node.Name = e.Label;
            }
            propertyGrid1.Refresh();
            GameView.Select();
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                Level level = (Level)e.Node.Tag;
                level.isVisible = e.Node.Checked;
                propertyGrid1.Refresh();
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                l.isVisible = e.Node.Checked;
                propertyGrid1.Refresh();
            }
            if (e.Node.Tag is LevelObject)
            {
                LevelObject lo = (LevelObject)e.Node.Tag;
                lo.isVisible = e.Node.Checked;
                propertyGrid1.Refresh();
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ActionDelete(sender, e);
            if (e.KeyCode == Keys.PageUp)
                Editor.Default.moveLayerUp(Editor.Default.selectedLayer);
            if (e.KeyCode == Keys.PageDown)
                Editor.Default.moveLayerDown(Editor.Default.selectedLayer);
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (((TreeNode)e.Item).Tag is Layer) return;
            if (((TreeNode)e.Item).Tag is Level) return;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            TreeNode sourcenode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (sourcenode == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            else e.Effect = DragDropEffects.Move;

            Point p = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode destnode = treeView1.GetNodeAt(p);

            if (destnode == null)
                return;

            if (destnode.Tag is Level) return;

            treeView1.SelectedNode = destnode;

            if (destnode != sourcenode)
            {
                LevelObject i1 = (LevelObject)sourcenode.Tag;
                if (destnode.Tag is LevelObject)
                {
                    LevelObject i2 = (LevelObject)destnode.Tag;
                    Editor.Default.moveObjectToLayer(i1, i2.layer, i2);
                    int delta = 0;
                    if (destnode.Index > sourcenode.Index && i1.layer == i2.layer) delta = 1;
                    sourcenode.Remove();
                    destnode.Parent.Nodes.Insert(destnode.Index + delta, sourcenode);
                }
                if (destnode.Tag is Layer)
                {
                    Layer l2 = (Layer)destnode.Tag;
                    Editor.Default.moveObjectToLayer(i1, l2, null);
                    sourcenode.Remove();
                    destnode.Nodes.Insert(0, sourcenode);
                }
                Editor.Default.selectLevelObject(i1);
                Application.DoEvents();
            }
        }

        //---> GameView-Steuerung <---//

        private void GameView_MouseEnter(object sender, EventArgs e)
        {
            GameView.Select();
        }

        private void GameView_MouseLeave(object sender, EventArgs e)
        {
            MenuBar.Select();
        }

        private void GameView_DragEnter(object sender, DragEventArgs e)
        {
            if (Editor.Default.selectedLayer == null)
                return;

            e.Effect = DragDropEffects.Move;

            ListViewItem lvi = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

            if (lvi == null)
                return;

            if (lvi.Tag == "TextureObject")
            {
                Editor.Default.createTextureObject(lvi.Name);
                Editor.Default.createCurrentObject(false);
                Editor.Default.startPositioning();
            }
            else if (lvi.Tag == "InteractiveObject")
            {
                Editor.Default.createInteractiveObject(lvi.Name);
                Editor.Default.createCurrentObject(false);
                Editor.Default.startPositioning();
            }
        }   

        private void GameView_Resize(object sender, EventArgs e)
        {
            if(EditorLoop.EditorLoopInstance != null) 
                EditorLoop.EditorLoopInstance.resizebackbuffer(GameView.Width, GameView.Height);

            Camera.updateViewport(GameView.Width, GameView.Height);
        }

        //---> TextureView-Steuerung <---//

        public void loadFolder(string path)
        {
            ImageList32.Images.Clear();
            TextureView.Clear();

            
            DirectoryInfo di = new DirectoryInfo(path);

            string filters = "*.jpg;*.png;*.bmp;";
            List<FileInfo> fileList = new List<FileInfo>();
            string[] extensions = filters.Split(';');
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();

            foreach (FileInfo file in files)
            {
                Bitmap bmp = new Bitmap(file.FullName);
                ImageList32.Images.Add(file.FullName, Editor.Default.getThumbNail(bmp, 32, 32));


                ListViewItem lvi = new ListViewItem();
                lvi.Name = file.FullName;
                lvi.Text = file.Name;
                lvi.ImageKey = file.FullName;
                lvi.Tag = "TextureObject";
                lvi.ToolTipText = file.Name + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                TextureView.Items.Add(lvi);
            }
        }

        private void TextureView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TextureView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TextureView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            Point p = GameView.PointToClient(new Point(e.X, e.Y));
            Editor.Default.SetMousePosition(p.X, p.Y);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK) loadFolder(d.SelectedPath);
        }

        private void TextureView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Editor.Default.selectedLayer == null)
            {
                DialogResult result = MessageBox.Show("There is no layer to add textures to it! Do you want to create one?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    new AddLayer().ShowDialog();
                else
                    return;
            }

            Editor.Default.createTextureObject(TextureView.FocusedItem.Name);
        }

        //---> InteractiveView-Steuerung <---//

        public void loadFolderInteractive(string path)
        {
            ImageListInteractive32.Images.Clear();
            InteractiveView.Clear();

            DirectoryInfo di = new DirectoryInfo(path);

            string filters = "*.jpg;*.png;*.bmp;";
            List<FileInfo> fileList = new List<FileInfo>();
            string[] extensions = filters.Split(';');
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();

            foreach (FileInfo file in files)
            {
                Bitmap bmp = new Bitmap(file.FullName);
                ImageListInteractive32.Images.Add(file.FullName, Editor.Default.getThumbNail(bmp, 32, 32));


                ListViewItem lvi = new ListViewItem();
                lvi.Name = file.FullName;
                lvi.Text = file.Name;
                lvi.ImageKey = file.FullName;
                lvi.Tag = "InteractiveObject";
                lvi.ToolTipText = file.Name + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                InteractiveView.Items.Add(lvi);
            }
        }

        private void InteractiveView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Editor.Default.selectedLayer == null)
            {
                DialogResult result = MessageBox.Show("There is no layer to add textures to it! Do you want to create one?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    new AddLayer().ShowDialog();
                else
                    return;
            }
            Editor.Default.createInteractiveObject(InteractiveView.FocusedItem.Name);
        }

        private void BrowseButton2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK) loadFolderInteractive(d.SelectedPath);
        }

        private void InteractiveView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            InteractiveView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void InteractiveView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            Point p = GameView.PointToClient(new Point(e.X, e.Y));
            Editor.Default.SetMousePosition(p.X, p.Y);
        }

        //---> Toolbar-Buttons <---//

        private void DeleteLayerButton_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Editor.Default.deleteLayer(l);
            }
        }

        private void LevelToolStrip_AddLayer(object sender, EventArgs e)
        {
            new AddLayer().ShowDialog();
        }

        //---> PrimitiveObject

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddPrimitive(PrimitiveType.Rectangle);
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddPrimitive(PrimitiveType.Circle);
        }

        private void pathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddPrimitive(PrimitiveType.Path);
        }

        //---> FixtureItem

        private void rectangleCollisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddFixture(FixtureType.Rectangle);
        }

        private void circleCollisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddFixture(FixtureType.Circle);
        }

        private void pathCollisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddFixture(FixtureType.Path);
        }

        //---> AddObjects

        private void soundObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog files = new OpenFileDialog();
            DialogResult result = files.ShowDialog();
          
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (files.FileNames.Length == 0)
                    return;

                foreach (string s in files.FileNames)
                {
                    Editor.Default.AddSoundObject(s);
                }
            }
        }

        //---> Events


        private void changeBodyTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.ChangeBodyType);
        }

        private void fadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Fade);
        }

        private void equalizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Equalizer);
        }

        private void modifyPlaybackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.ModifyPlayback);
        }

        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Mute);
        }

        private void setVolumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.SetVolume);
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Play);
        }

        private void reverbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Reverb);
        }

        private void crossfaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Crossfader);
        }

        private void deathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Death);
        }


        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Camera);
        }

        private void moveRotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.Move);
        }

        private void changeVisibilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.ChangeVisibility);
        }

        private void applyForceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.ApplyForce);
        }

        private void saveStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.AddEvents(EventType.SaveState);
        }

        //---> ToolStrips <---//

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.deleteLayer(Editor.Default.selectedLayer);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Editor.Default.deleteLevelObjects();
        }

        private void renameToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Editor.Default.copyLevelObjects(Editor.Default.selectedLayer);
        }

        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }

        private void renameToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }

        private void renameToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.copyLevelObjects(Editor.Default.selectedLayer);
        }

        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Editor.Default.deleteLevelObjects();
        }

        private void addObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ManageEvents().ShowDialog();
        }


        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Editor.Default.moveLayerUp(l);
            }
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Editor.Default.moveLayerDown(Editor.Default.selectedLayer);
            }
        }

        private void particleObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Default.createParticleObject();
        }

        //---> Actions <---//

        private void ActionDelete(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Editor.Default.deleteLayer(l);
            }
            else if (treeView1.SelectedNode.Tag is LevelObject)
            {
                Editor.Default.deleteLevelObjects();
            }
        }
    }
}
