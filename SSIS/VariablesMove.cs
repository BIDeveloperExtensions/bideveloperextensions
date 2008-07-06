﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS
{
    public partial class VariablesMove : Form
    {
        Package package;

        public VariablesMove(Package pkg, string CurrentlySelectedID)
        {
            this.package = pkg;
            
            InitializeComponent();

            IterateContainers(pkg, this.treeView1.Nodes, CurrentlySelectedID);
        }

        private void IterateContainers(DtsContainer parent, TreeNodeCollection nodes, string CurrentlySelectedID)
        {
            TreeNode node = new TreeNode();
            node.Name = parent.Name;
            node.Text = parent.Name;
            node.Tag = parent;
            nodes.Add(node);

            if (parent.ID == CurrentlySelectedID)
                node.TreeView.SelectedNode = node;

            IDTSSequence seq = (IDTSSequence)parent;

            foreach (Executable e in seq.Executables)
            {
                if (e is IDTSSequence)
                {
                    IterateContainers((DtsContainer)e, node.Nodes, CurrentlySelectedID);
                }
                else
                {
                    DtsContainer task = (DtsContainer)e;
                    TreeNode childNode = new TreeNode();
                    childNode.Name = task.Name;
                    childNode.Text = task.Name;
                    childNode.Tag = task;
                    node.Nodes.Add(childNode);

                    if (task.ID == CurrentlySelectedID)
                        node.TreeView.SelectedNode = childNode;
                }
            }

            return;
        }

    }
}