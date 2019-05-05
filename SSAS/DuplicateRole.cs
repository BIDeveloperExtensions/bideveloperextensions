using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    class DuplicateRole : BIDSHelperPluginBase
    {
           public DuplicateRole(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DuplicateRoleId, typeof(Role));
        }

        public override string ShortName
        {
            get { return "DuplicateRole"; }
        }

        //public override int Bitmap
        //{
        //    get { return 19; } //Copy Icon
        //}

        public override string FeatureName
        {
            get { return "Duplicate Role"; }
        }

        public override string ToolTip
        {
            get { return "Duplicates a Role and all the relate permissions"; }
        }
        

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you to Duplicate a Role including all the related permissions."; }
        }

        
        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Role r = (Role)projItem.Object;

                DuplicateRoleAndPermissions(r);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DuplicateRoleAndPermissions( Role r)
        {
            Role newRole = r.Clone();
            newRole.ID = NewId(r);
            newRole.Name = r.Name + " - Copy";
            ((Database)r.Parent).Roles.Add(newRole);
            System.Collections.Hashtable hashRefs = new System.Collections.Hashtable();
            r.GetDependents(hashRefs);
            foreach (MajorObject mo in hashRefs.Keys)
            {
                 ClonePermissions(mo, newRole);
            }
        }

        // Helper Functions
        // ================

        // returns a new id with a GUID so that it is guaranteed to be unique
        private string NewId(MajorObject mo)
        {
            return mo.GetType().Name + " " + System.Guid.NewGuid().ToString();
        }

        // this routine clones the permissions for the various major permission types
        private void ClonePermissions(MajorObject mo, Role r)
        {
            DimensionPermission dimPerm = mo as DimensionPermission;
            CubePermission cubePerm = mo as CubePermission;
            DatabasePermission dbPerm = mo as DatabasePermission;
            DataSourcePermission dsPerm = mo as DataSourcePermission;

            if (dimPerm != null)
                ClonePermissions(dimPerm, r);
            else if (cubePerm != null)
                ClonePermissions(cubePerm, r);
            else if (dbPerm != null)
                ClonePermissions(dbPerm, r);
            else if (dsPerm != null)
                ClonePermissions(dsPerm, r);
            else throw new System.Exception("BIDSHelper: unhandled permission type");            

        }

        private void ClonePermissions(CubePermission cp, Role r)
        {
            CubePermission newCp = cp.Clone();
            newCp.ID = NewId(newCp);
            newCp.Name = newCp.ID; // cp.Name + " - Copy";
            newCp.RoleID = r.ID;
            cp.Parent.CubePermissions.Add(newCp);
        }

        private void ClonePermissions(DatabasePermission dbp, Role r)
        {
            DatabasePermission newDp = dbp.Clone();
            newDp.ID = NewId(newDp);
            newDp.Name = newDp.ID; //dbp.Name + " - Copy";
            newDp.RoleID = r.ID;
            dbp.Parent.DatabasePermissions.Add(newDp);
        }

        private void ClonePermissions(DimensionPermission dimp, Role r)
        {
            DimensionPermission newPerm =  dimp.Clone();
            newPerm.ID = NewId(newPerm);
            newPerm.Name = newPerm.ID; // dimp.Name + " - Copy";
            newPerm.RoleID = r.ID;
            dimp.Parent.DimensionPermissions.Add(newPerm);
        }

        private void ClonePermissions(DataSourcePermission dsp, Role r)
        {
            DataSourcePermission newPerm = dsp.Clone();
            newPerm.ID = NewId(newPerm);
            newPerm.RoleID = r.ID;
            newPerm.Name = newPerm.ID; //dsp.Name + " - Copy";
            dsp.Parent.DataSourcePermissions.Add(newPerm);
        }
    }
}
