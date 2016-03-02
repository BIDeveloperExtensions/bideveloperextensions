using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public partial class EnhancedDeployWindow : Form
    {
        public EnhancedDeployWindow()
        {
            InitializeComponent();
        }



        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            rb.Parent.Tag = rb.Tag;
        }

        private void btnDeploy_Click(object sender, EventArgs e)
        {
            //TODO 
            PartitionEnhancedDeployModes partMode = (PartitionEnhancedDeployModes)Enum.Parse(typeof(PartitionEnhancedDeployModes), this.grpPartitions.Tag.ToString());
            RoleEnhancedDeployModes roleMode = (RoleEnhancedDeployModes)Enum.Parse(typeof(RoleEnhancedDeployModes), this.grpRoles.Tag.ToString());
            EnhancedDeployEngine.Deploy((Database)srcObj, partMode, roleMode);
            this.Close();
        }

        

        public string TargetServer
        {
            get { return lblServer.Text; }
            set { this.lblServer.Text = value; }
        }

        
        public string TargetDatabase
        {
            get { return this.lblDatabase.Text; }
            set { this.lblDatabase.Text = value;  }
        }

        private MajorObject srcObj;
        public MajorObject SourceObject
        {
            get { return srcObj;}
            set { srcObj = value;}
        }
        
    }
}
