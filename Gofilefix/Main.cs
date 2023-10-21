// Paulo Oliveira
// 21/10/2023
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Gofilefix
{
    public partial class Main : Form
    {

        string[] prefixes = { "GH", "GX", "GP", "GL"};
        string currentPath = "";
        TreeNode currentNode = null;

        public Main()
        {
            InitializeComponent();
            PopulateTreeView(Assembly.GetEntryAssembly().Location);
            this.treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
        }


        private void Main_Activated(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void PopulateTreeView(string path)
        {
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents();            
            currentPath = path;
            statusLabel.Text = $"Searching for Gopro files in {currentPath} ...";
            try
            {
                treeView1.Nodes.Clear();
                TreeNode actualNode;
                string actualPath = Path.GetDirectoryName(path);
                DirectoryInfo dir = new DirectoryInfo(actualPath);
                if (dir.Exists)
                {
                    actualNode = new TreeNode(actualPath);
                    actualNode.Tag = dir;
                    GetDirectories(dir.GetDirectories(), actualNode);
                    treeView1.Nodes.Add(actualNode);
                }
                if (treeView1.Nodes.Count > 0)
                {
                    if (currentNode != null)
                    {
                        PopulateFiles(currentNode);
                    }
                    else
                    {
                        PopulateFiles(treeView1.Nodes[0]);
                    }                    
                }
            }
            finally 
            {                
                Cursor.Current = Cursors.Default;
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                try
                {
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0)
                    {
                        GetDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Nodes.Add(aNode);
                }
                catch (Exception e)
                { 
                    //got an error, probably permissions, but we don't care about those
                    Console.WriteLine(e.ToString());
                }
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {            
            PopulateFiles(e.Node);
        }

        public void PopulateFiles(TreeNode rNode)
        {
            currentNode = rNode;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)rNode.Tag;
            currentPath = nodeDirInfo.FullName;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            if (null != nodeDirInfo)
            {
                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    if (!IsGoProFile(file.Name))
                    { 
                        continue; 
                    }
                    item = new ListViewItem(file.Name, 1);
                    item.Tag = file;
                    string fileTimestamp = $"{file.LastWriteTime.ToShortDateString()} {file.LastWriteTime.ToLongTimeString()}";
                    subItems = new ListViewItem.ListViewSubItem[]
                        {
                            new ListViewItem.ListViewSubItem(item, BytesToString(file.Length)),
                            new ListViewItem.ListViewSubItem(item, fileTimestamp)
                        };
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
            }
            if (listView1.Items.Count > 0)
            {
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                statusLabel.Text = $"Found {listView1.Items.Count} GoPro files in {currentPath}";
            } 
            else
            {
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                statusLabel.Text = $"No GoPro files found in {currentPath}";
            }
        }

        private void btnChooseDir_Click(object sender, EventArgs e)
        {
            if (currentPath == "")
            {
                currentPath = Assembly.GetEntryAssembly().Location;
            }
            browseForFolder.SelectedPath = Path.GetDirectoryName(currentPath);
            DialogResult result = browseForFolder.ShowDialog();
            if (result == DialogResult.OK)
            {
                currentNode = null;
                PopulateTreeView(browseForFolder.SelectedPath + "//");
            }
        }


        private bool IsGoProFile(string fileName)
        {
            return fileName.Length == 12 && prefixes.Where(p => fileName.StartsWith(p)).Any();
        }

        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        private String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                return;
            }
            DialogResult result = MessageBox.Show($"Are you sure that you want to rename {listView1.Items.Count} GoPro files?", "Plase confirm", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                RenameFiles();
                PopulateFiles(currentNode);
            }
        }

        private void RenameFiles()
        {
            foreach(ListViewItem item in listView1.Items)
            {
                FileInfo file = (FileInfo)item.Tag;
                if (file.Exists)
                {
                    string newName = ChangeFileName(file.Name);
                    file.MoveTo(Path.Combine(file.DirectoryName, newName));
                }
            }
        }

        private string ChangeFileName(string oldFileName)
        {
            string ext = oldFileName.Substring(oldFileName.LastIndexOf('.') + 1);
            string ret = oldFileName.Substring(0, 2) + oldFileName.Substring(4, 4) + oldFileName.Substring(2,2) + "." + ext;
            return ret;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
