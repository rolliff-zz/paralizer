/*************************************************************************
   Date  : 1/17/2015 12:00:00 AM  
   Author: Robert A. Olliff
   Email : robert.olliff@gmail.com

   This file probably has code in it and does stuff.
************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Paralizer.UI
{
    public partial class LameUI : Form
    {
        void Error(string fmt, params object[] args)
        {
            MessageBox.Show(String.Format(fmt, args), "Error of mystery");
        }

        ParadoxDataElement Root { get; set; }
        TreeNode RootNode { get; set; }
        public LameUI()
        {
            InitializeComponent();

            nodeRefresh.WorkerReportsProgress = false;
            nodeRefresh.DoWork+=nodeRefresh_DoWork;
            nodeRefresh.RunWorkerCompleted += nodeRefresh_RunWorkerCompleted;

        }

        

        TreeNode Init(ParadoxDataElement root)
        {
            Root = root;
            
            Console.WriteLine("Loading UI...");
            TreeNode rootNode = new TreeNode()
            {
                Text = "<root>",
                Tag = Root
            };
            RootNode = rootNode;
            TreeNode current = rootNode;
            Iterate(rootNode, Root,1);
            return rootNode;
        }

        void IterateArray(TreeNode root, ParadoxDataElement element, int depth = -1)
        {
            if (depth == 0)
                return;

            int n = 0;
            foreach (var e in element.Children)
            {
                TreeNode node = new TreeNode();
                node.Text = n.ToString();
                node.Tag = e;
                n++;
                root.Nodes.Add(node);
            }
        }
        void AddNode(TreeNode root, ParadoxDataElement element, int depth = -1)
        {
            TreeNode node = new TreeNode();
            node.Text = element.Name;
            node.Tag = element;
            switch (element.Type)
            {
                case ObjectType.Array:
                    IterateArray(node, element, depth - 1);
                    break;
                case ObjectType.Associative:
                case ObjectType.AssociativeAnonymous:
                case ObjectType.DuplicatesArray:
                case ObjectType.Root:
                    Iterate(node, element, depth - 1);
                    break;
                default:
                    break;
            }
           // Debug.WriteLine("Adding Node: {0} to {1}", element.Name, (root.Tag as ParadoxDataElement).Name);
            root.Nodes.Add(node);
        }
        void Iterate(TreeNode root, ParadoxDataElement element, int depth = -1)
        {
            if (depth == 0)
                return;
            foreach (var e in element.Children)
            {
                AddNode(root, e, depth);
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if ((e.Node.Tag as ParadoxDataElement).Content != null)
                textBox.Text = (e.Node.Tag as ParadoxDataElement).Content.ToString();
            else
                textBox.Text = "<null>";

            typeLabel.Text = (e.Node.Tag as ParadoxDataElement).Type.ToString();
            buttonUpdate.Tag = e.Node;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            var node = (buttonUpdate.Tag as TreeNode);
            if(node != null)
            {
                try
                {
                    var element = node.Tag as ParadoxDataElement;
                    switch (element.Type)
                    {
                        case ObjectType.Date:
                            element.Content = textBox.Text;
                            break;
                        case ObjectType.Name:
                            element.Content = textBox.Text;
                            break;
                        case ObjectType.Float:
                            element.Content = decimal.Parse(textBox.Text);
                            break;
                        case ObjectType.Integer:
                            element.Content = int.Parse(textBox.Text);
                            break;
                        case ObjectType.Boolean:
                            element.Content = bool.Parse(textBox.Text);
                            break;
                        case ObjectType.String:
                            element.Content = textBox.Text;
                            break;
                        case ObjectType.DuplicatesArray:
                        case ObjectType.Array:
                        case ObjectType.Associative:
                        case ObjectType.AssociativeAnonymous:
                        default:
                            throw new Exception("Impossible!");
                    }
                }
                catch (FormatException f)
                {
                    MessageBox.Show("Crap: " + f.Message);
                }
            }
        }

        private void btnJSON_Click(object sender, EventArgs e)
        {
            var obj = (treeView.SelectedNode.Tag as ParadoxDataElement);
            var s = ParalizerCore.ToJson(obj);

            

            textRaw.Text = s.ToString();
        }

        private void btnWeird_Click(object sender, EventArgs e)
        {
            var obj = (treeView.SelectedNode.Tag as ParadoxDataElement);
            var s = ParalizerCore.ToParadox(obj);

            textRaw.Text = s;
        }
        BackgroundWorker w = new BackgroundWorker();    
        private void loadParadoxFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var res = openFileDialog1.ShowDialog();
            if (res != DialogResult.OK)
                return;


            
            w.WorkerReportsProgress = false;
            w.DoWork+=w_DoWork;
            w.RunWorkerCompleted += w_RunWorkerCompleted;
            w.RunWorkerAsync(openFileDialog1.OpenFile());
        }

        void w_DoWork(object a, DoWorkEventArgs b)
        {
            try
            {
                statusLabel.Text = "parsing...";
                var obj = ParalizerCore.FromParadox(new StreamReader(b.Argument as Stream));

                statusLabel.Text = "loading ui...";
                b.Result = Init(obj);
            }
            catch (Exception x)
            {
                b.Result = x;
            }
        }


        void w_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            treeView.Nodes.Clear();
            if (e.Result is Exception)
            {
                statusLabel.Text = "ready";
                Error("Damned Error: {0}", e.Result);
                return;
            }
            statusLabel.Text = "ready";
            treeView.Nodes.Add(e.Result as TreeNode);
        }

        private void spendTimeWithKidsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            w.CancelAsync();
            Close();
        }

        private void btnNodeUpdate_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textRaw.Text))
            {
                Error("No text");
                return;
            }

            ParadoxDataElement obj;
            if (textRaw.Text.StartsWith("{"))
            {
                obj = ParalizerCore.FromJson(JObject.Parse(textRaw.Text));
            }
            else
            {
                obj = ParalizerCore.FromParadox(new StringReader(textRaw.Text));
            }

            var update = obj.Children.First();
            (treeView.SelectedNode.Tag as ParadoxDataElement).SwapParent(update);

            treeView.SelectedNode.Tag = update;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParalizerCore.ToParadox(RootNode.Tag as ParadoxDataElement, openFileDialog1.FileName);
        }

        BackgroundWorker nodeRefresh = new BackgroundWorker();
        class NodeRefreshHelper : BackgroundWorker
        {
            public NodeRefreshHelper() { }
            
        };
        void nodeRefresh_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void nodeRefresh_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var data = (e.Node.Tag as ParadoxDataElement);
            statusLabel.Text = "loading...";
            Application.DoEvents();
            e.Node.Nodes.Clear();
            switch (data.Type)
            {
                case ObjectType.Array:
                    IterateArray(e.Node, data, 2);
                    break;
                case ObjectType.Associative:
                case ObjectType.AssociativeAnonymous:
                case ObjectType.DuplicatesArray:
                case ObjectType.Root:
                    Iterate(e.Node, data, 2);
                    break;
                default:
                    break;
            }
            statusLabel.Text = "ready";
        }

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParalizerCore.TestEncoding();
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParalizerCore.TestEncoding2();
        }

        
    }
}
