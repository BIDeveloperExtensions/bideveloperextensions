namespace BIDSHelper.SSIS
{
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.SqlServer.Dts.Runtime;
    
    public partial class VariablesMove : Form
    {
        private string selectedContainerId;

        public VariablesMove(Package package, string selectedContainerId, int selectedVariablesCount)
        {
            this.selectedContainerId = selectedContainerId;
            
            InitializeComponent();

            IterateContainers(package, this.treeView.Nodes, selectedContainerId);

            // Expand root node, the package
            this.treeView.Nodes[0].Expand();

            this.Icon = BIDSHelper.Resources.Common.Copy;

            // Change caption based on count of selected variables,
            // makes form less clutered and easier to read.
            if (selectedVariablesCount > 1)
            {
                this.radCopy.Text = "Copy variables to...";
                this.radMove.Text = "Move variables to...";
            }
            else
            {
                this.radCopy.Text = "Copy variable to...";
                this.radMove.Text = "Move variable to...";
            }
        }

        /// <summary>
        /// Gets the target container for the move or copy operation.
        /// </summary>
        /// <value>The target container.</value>
        public DtsContainer TargetContainer
        {
            get
            {
                DtsContainer container = null;
                TreeNode node = this.treeView.SelectedNode;
                if (node != null)
                {
                    container = node.Tag as DtsContainer;
                }

                return container;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to move the variable.
        /// </summary>
        /// <value><c>true</c> to move the variable; otherwise <c>false</c> indicates it will be copied.</value>
        public bool IsMove
        {
            get { return this.radMove.Checked; }
        }

        private void IterateContainers(DtsContainer parent, TreeNodeCollection nodes, string selectedContainerId)
        {
            TreeNode node = new TreeNode();
            node.Name = parent.Name;
            node.Text = parent.Name;
            node.Tag = parent;
            SetNodeIcon(parent, node);
            nodes.Add(node);

            if (parent.ID == selectedContainerId)
            {
                node.TreeView.SelectedNode = node;
            }

            IDTSSequence seq = parent as IDTSSequence;
            if (seq != null)
            {
                foreach (Executable e in seq.Executables)
                {
                    if (e is IDTSSequence || e is EventsProvider)
                    {
                        IterateContainers((DtsContainer)e, node.Nodes, selectedContainerId);
                    }
                    else
                    {
                        DtsContainer task = (DtsContainer)e;
                        TreeNode childNode = new TreeNode();
                        childNode.Name = task.Name;
                        childNode.Text = task.Name;
                        childNode.Tag = task;
                        SetNodeIcon(task, childNode);
                        node.Nodes.Add(childNode);

                        if (task.ID == selectedContainerId)
                        {
                            node.TreeView.SelectedNode = childNode;
                        }
                    }
                }
            }

            EventsProvider prov = parent as EventsProvider;
            if (prov != null)
            {
                foreach (DtsEventHandler p in prov.EventHandlers)
                {
                    DtsContainer task = (DtsContainer)p;
                    TreeNode childNode = new TreeNode();
                    childNode.Name = string.Format(CultureInfo.InvariantCulture, "{0} Event", p.Name);
                    childNode.Text = string.Format(CultureInfo.InvariantCulture, "{0} Event", p.Name);
                    childNode.Tag = task;
                    SetNodeIcon(task, childNode);
                    node.Nodes.Add(childNode);

                    if (task.ID == selectedContainerId)
                    {
                        node.TreeView.SelectedNode = childNode;
                    }
                }
            }
            return;
        }

        private void SetNodeIcon(DtsContainer container, TreeNode childNode)
        {
            string key = PackageHelper.GetContainerKey(container);
            if (!this.imageList.Images.ContainsKey(key))
            {
                if (PackageHelper.ControlFlowInfos.ContainsKey(key))
                {
                    this.imageList.Images.Add(key, PackageHelper.ControlFlowInfos[key].Icon);
                }
                else
                {
                    MessageBox.Show(container.Name);
                }
            }

            childNode.ImageKey = key;
            childNode.SelectedImageKey = key;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            DtsContainer container = e.Node.Tag as DtsContainer;
            if (container == null)
            {
                return;
            }

            // Enable or disable OK button, when a valid move or copy target has been selected
            this.btnOK.Enabled = !(this.selectedContainerId == container.ID);
        }
    }
}
