using System;
using System.Collections.Generic;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;
using System.DirectoryServices;

//helps work around this error: "No mapping between account names and security IDs was done."

namespace BIDSHelper
{
    public class RolesReportPlugin : BIDSHelperPluginBase
    {
        private string _deploymentTargetServer;

        public RolesReportPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "RolesReportPlugin"; }
        }

        public override int Bitmap
        {
            get { return 3709; }
        }

        public override string ButtonText
        {
            get { return "Roles Report..."; }
        }

        public override string FriendlyName
        {
            get { return "Roles Reports"; }
        }

        public override string ToolTip
        {
            get { return ""; /*doesn't show anywhere*/ }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        public override string MenuName
        {
            get { return "Folder Node"; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                
                // this figures out if this is the roles node without using the name
                // by checking the type of the first child item. 
                return (hierItem.UIHierarchyItems.Count >= 1
                    && ((ProjectItem)hierItem.UIHierarchyItems.Item(1).Object).Object is Role);
            }
            catch
            {
                return false;
            }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Database db = projItem.ContainingProject.Object as Database;
                DeploymentSettings _deploymentSettings = new DeploymentSettings(projItem);

                //get the target server name
                _deploymentTargetServer = _deploymentSettings.TargetServer;
                if (_deploymentTargetServer.IndexOf(':') >= 0)
                    _deploymentTargetServer = _deploymentTargetServer.Substring(0, _deploymentTargetServer.IndexOf(':') - 1);
                if (_deploymentTargetServer.IndexOf('\\') >= 0)
                    _deploymentTargetServer = _deploymentTargetServer.Substring(0, _deploymentTargetServer.IndexOf('\\') - 1);

                List<RoleMemberInfo> listRoleMembers = new List<RoleMemberInfo>();
                List<RoleDataSourceInfo> listRoleDataSources = new List<RoleDataSourceInfo>();
                List<RoleCubeInfo> listRoleCubes = new List<RoleCubeInfo>();
                List<RoleDimensionInfo> listRoleDimensions = new List<RoleDimensionInfo>();
                List<RoleMiningInfo> listRoleMining = new List<RoleMiningInfo>();

                foreach (Role r in db.Roles)
                {
                    if (r.Members.Count == 0)
                    {
                        RoleMemberInfo infoMember = new RoleMemberInfo();
                        infoMember.database = db;
                        infoMember.role = r;
                        listRoleMembers.Add(infoMember);
                    }
                    foreach (RoleMember rm in r.Members)
                    {
                        RoleMemberInfo infoMember = new RoleMemberInfo();
                        infoMember.TargetServerName = _deploymentTargetServer;
                        infoMember.database = db;
                        infoMember.role = r;
                        infoMember.directoryEntry = GetDirectoryEntry(rm.Name);
                        infoMember.MemberName = GetDomainAndUserForDirectoryEntry(infoMember.directoryEntry);
                        if (string.IsNullOrEmpty(infoMember.MemberName))
                            infoMember.MemberName = rm.Name;
                        listRoleMembers.Add(infoMember);

                        listRoleMembers.AddRange(GetGroupMembers(infoMember.directoryEntry, db, r, 1));
                    }

                    foreach (DataSource ds in db.DataSources)
                    {
                        RoleDataSourceInfo info = new RoleDataSourceInfo();
                        info.role = r;
                        info.dataSource = ds;
                        listRoleDataSources.Add(info);
                    }

                    foreach (Cube c in db.Cubes)
                    {
                        RoleCubeInfo info = new RoleCubeInfo();
                        info.role = r;
                        info.cube = c;
                        listRoleCubes.Add(info);
                    }

                    foreach (MiningStructure ms in db.MiningStructures)
                    {
                        RoleMiningInfo info = new RoleMiningInfo();
                        info.role = r;
                        info.miningStructure = ms;
                        listRoleMining.Add(info);

                        foreach (MiningModel mm in ms.MiningModels)
                        {
                            info = new RoleMiningInfo();
                            info.role = r;
                            info.miningStructure = ms;
                            info.miningModel = mm;
                            listRoleMining.Add(info);
                        }
                    }

                    foreach (Dimension d in db.Dimensions)
                    {
                        RoleDimensionInfo info = new RoleDimensionInfo();
                        info.role = r;
                        info.dimension = d;
                        listRoleDimensions.Add(info);

                        foreach (DimensionAttribute da in d.Attributes)
                        {
                            RoleDimensionInfo info2 = new RoleDimensionInfo();
                            info2.role = r;
                            info2.dimension = d;
                            info2.dimensionAttribute = da;
                            if (!string.IsNullOrEmpty(info2.AllowedMemberSet) || !string.IsNullOrEmpty(info2.DeniedMemberSet) || !string.IsNullOrEmpty(info2.DefaultMember) || !string.IsNullOrEmpty(info2.VisualTotals))
                                listRoleDimensions.Add(info2);
                        }
                    }

                    foreach (Cube c in db.Cubes)
                    {
                        foreach (CubeDimension cd in c.Dimensions)
                        {
                            RoleDimensionInfo info = new RoleDimensionInfo();
                            info.role = r;
                            info.dimension = cd.Dimension;
                            info.cubeDimension = cd;
                            if (info.ReadAccess != "Inherited")
                                listRoleDimensions.Add(info);

                            foreach (CubeAttribute ca in cd.Attributes)
                            {
                                RoleDimensionInfo info2 = new RoleDimensionInfo();
                                info2.role = r;
                                info2.dimension = cd.Dimension;
                                info2.cubeDimension = cd;
                                info2.dimensionAttribute = ca.Attribute;
                                if (!string.IsNullOrEmpty(info2.AllowedMemberSet) || !string.IsNullOrEmpty(info2.DeniedMemberSet) || !string.IsNullOrEmpty(info2.DefaultMember) || !string.IsNullOrEmpty(info2.VisualTotals))
                                    listRoleDimensions.Add(info2);
                            }
                        }
                    }
                }

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = listRoleMembers;
                frm.Report = "SSAS.RolesReport.rdlc";

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "BIDSHelper_RoleMemberInfo";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource2 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource2.Name = "BIDSHelper_RoleDataSourceInfo";
                reportDataSource2.Value = listRoleDataSources;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource2);

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource3 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource3.Name = "BIDSHelper_RoleCubeInfo";
                reportDataSource3.Value = listRoleCubes;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource3);

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource4 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource4.Name = "BIDSHelper_RoleDimensionInfo";
                reportDataSource4.Value = listRoleDimensions;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource4);

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource5 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource5.Name = "BIDSHelper_RoleMiningInfo";
                reportDataSource5.Value = listRoleMining;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource5);

                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                frm.Caption = "Roles Report";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private List<RoleMemberInfo> GetGroupMembers(DirectoryEntry group, Database db, Role r, int Indent)
        {
            List<RoleMemberInfo> listRoleMembers = new List<RoleMemberInfo>();
            try
            {
                if (group != null && group.Invoke("Members") != null)
                {
                    foreach (object member in (System.Collections.IEnumerable)group.Invoke("Members"))
                    {
                        DirectoryEntry memberEntry = new DirectoryEntry(member);

                        RoleMemberInfo infoMember = new RoleMemberInfo();
                        infoMember.TargetServerName = _deploymentTargetServer;
                        infoMember.database = db;
                        infoMember.role = r;
                        infoMember.MemberName = GetDomainAndUserForDirectoryEntry(memberEntry);
                        try
                        {
                            if (memberEntry.Path.ToUpper().StartsWith("WINNT:"))
                            {
                                DirectoryEntry directoryEntryLDAP = GetDirectoryEntryFromSID(memberEntry);
                                if (directoryEntryLDAP != null) memberEntry = directoryEntryLDAP;
                            }
                        }
                        catch { }
                        infoMember.directoryEntry = memberEntry;
                        infoMember.MemberIndent = Indent;
                        listRoleMembers.Add(infoMember);

                        if (Indent < 20) //prevent infinite recursion, though I'm not sure that's possible
                            listRoleMembers.AddRange(GetGroupMembers(memberEntry, db, r, Indent + 1));
                    }
                }
            }
            catch { }
            return listRoleMembers;
        }

        //adapted from Edward Melomed's ASValidateUsers code at http://sqlsrvanalysissrvcs.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=13572
        private DirectoryEntry GetDirectoryEntry(string MemberName)
        {
            // Parse the string to check if domain name is present.
            int idx = MemberName.IndexOf('\\');
            if (idx == -1)
            {
                idx = MemberName.IndexOf('@');
            }

            string strDomain;
            string strName;

            if (idx != -1)
            {
                strDomain = MemberName.Substring(0, idx);
                if (strDomain == "BUILTIN" || strDomain == "NT AUTHORITY")
                {
                    strDomain = _deploymentTargetServer; // Environment.MachineName; //improve this????? TODO
                }
                strName = MemberName.Substring(idx + 1);
            }
            else
            {
                strDomain = _deploymentTargetServer; //improve this????? TODO
                strName = MemberName;
            }

            if (string.Compare(strDomain, _deploymentTargetServer, true) != 0)
            {
                try
                {
                    DirectoryEntry adRoot = new DirectoryEntry("LDAP://" + strDomain, null, null, AuthenticationTypes.Secure);
                    DirectorySearcher searcher = new DirectorySearcher(adRoot);
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.ReferralChasing = ReferralChasingOption.All;
                    //searcher.PropertiesToLoad.AddRange(new string[] { "fullname" }); //TODO
                    searcher.Filter = string.Format("(sAMAccountName={0})", strName);

                    SearchResult result = searcher.FindOne();
                    DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                    return directoryEntry;
                }
                catch { }
            }

            DirectoryEntry directoryEntryWinNT = new DirectoryEntry("WinNT://" + strDomain + "/" + strName);
            return directoryEntryWinNT;

        }

        private string GetDomainAndUserForDirectoryEntry(DirectoryEntry directoryEntry)
        {
            try
            {
                System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier((byte[])directoryEntry.Properties["objectSid"].Value, 0);

                System.Security.Principal.NTAccount account = (System.Security.Principal.NTAccount)sid.Translate(typeof(System.Security.Principal.NTAccount));
                return account.ToString(); // This give the DOMAIN\User format for the account
            }
            catch
            {
                string sName = "";
                try
                {
                    sName = Convert.ToString(directoryEntry.Properties["displayName"].Value);
                    if (string.IsNullOrEmpty(sName)) throw new Exception();
                    return sName;
                }
                catch
                {
                    try
                    {
                        sName = Convert.ToString(directoryEntry.Properties["fullName"].Value);
                        if (string.IsNullOrEmpty(sName)) throw new Exception();
                        return sName;
                    }
                    catch
                    {
                        try
                        {
                            return directoryEntry.Name;
                        }
                        catch
                        {
                            try
                            {
                                System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier((byte[])directoryEntry.Properties["objectSid"].Value, 0);
                                return sid.Value;
                            }
                            catch
                            {
                                return null;
                            }
                        }
                    }
                }
            }
        }

        private DirectoryEntry GetDirectoryEntryFromSID(DirectoryEntry directoryEntry)
        {
            try
            {
                System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier((byte[])directoryEntry.Properties["objectSid"].Value, 0);
                System.Security.Principal.NTAccount acct = (System.Security.Principal.NTAccount)sid.Translate(typeof(System.Security.Principal.NTAccount));
                DirectoryEntry directoryEntryLDAP = GetDirectoryEntry(acct.Value);
                if (directoryEntryLDAP.Name != null)
                    return directoryEntryLDAP;
            }
            catch { }
            return null;
        }


        #region RoleMemberInfo classes
        public class RoleMemberInfo
        {
            public Role role;
            public Database database;
            public DirectoryEntry directoryEntry;

            private string _TargetServerName;
            public string TargetServerName
            {
                get { return _TargetServerName; }
                set { _TargetServerName = value; }
            }

            public string DatabaseName
            {
                get { return database.Name; }
            }

            public string RoleName
            {
                get { return role.Name; }
            }

            public string RoleDescription
            {
                get { return role.Description; }
            }

            public bool DatabasePermissionAdminister
            {
                get
                {
                    DatabasePermission permDB = database.DatabasePermissions.GetByRole(role.ID);
                    if (permDB != null)
                    {
                        return permDB.Administer;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public bool DatabasePermissionProcess
            {
                get
                {
                    DatabasePermission permDB = database.DatabasePermissions.GetByRole(role.ID);
                    if (permDB != null)
                    {
                        return permDB.Process;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public string DatabasePermissionReadDefinition
            {
                get
                {
                    DatabasePermission permDB = database.DatabasePermissions.GetByRole(role.ID);
                    if (permDB != null)
                    {
                        return permDB.ReadDefinition.ToString();
                    }
                    else
                    {
                        return ReadDefinitionAccess.None.ToString();
                    }
                }
            }

            private string _MemberName;
            public string MemberName
            {
                get { return _MemberName; }
                set { _MemberName = value; }
            }

            private int _MemberIndent;
            public int MemberIndent
            {
                get { return _MemberIndent; }
                set { _MemberIndent = value; }
            }

            public string MemberDescription
            {
                get
                {
                    if (directoryEntry == null) return null;
                    return Convert.ToString(directoryEntry.Properties["description"].Value);
                }
            }

            private static List<System.Security.Principal.NTAccount> _listWellKnownAccounts = new List<System.Security.Principal.NTAccount>();
            public bool IsBuiltIn
            {
                get
                {
                    if (_listWellKnownAccounts.Count == 0)
                    {
                        foreach (System.Security.Principal.WellKnownSidType sidType in System.Enum.GetValues(typeof(System.Security.Principal.WellKnownSidType)))
                        {
                            try
                            {
                                System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(sidType, null);
                                System.Security.Principal.NTAccount account = sid.Translate(typeof(System.Security.Principal.NTAccount)) as System.Security.Principal.NTAccount;
                                if (account != null) _listWellKnownAccounts.Add(account);
                            }
                            catch { }
                        }
                    }
                    foreach (System.Security.Principal.NTAccount account in _listWellKnownAccounts)
                    {
                        if (string.Compare(account.ToString(), MemberName, true) == 0)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            internal enum ADS_GROUP_TYPE_ENUM : uint
            {
                ADS_GROUP_TYPE_BUILT_IN = 0x00000001,
                ADS_GROUP_TYPE_GLOBAL_GROUP = 0x00000002,
                ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP = 0x00000004,
                ADS_GROUP_TYPE_UNIVERSAL_GROUP = 0x00000008,
                ADS_GROUP_TYPE_SECURITY_ENABLED = 0x80000000
            }

            public string GroupType
            {
                get
                {
                    try
                    {
                        if (directoryEntry == null) return "Not Found!";
                        if (IsBuiltIn) return "Built-in";

                        int groupType = 0;
                        try
                        {
                            if (!directoryEntry.Properties.Contains("groupType"))
                            {
                                if (directoryEntry.Properties.Contains("objectClass"))
                                    return Microsoft.VisualBasic.Strings.StrConv(Convert.ToString(directoryEntry.Properties["objectClass"][directoryEntry.Properties["objectClass"].Count - 1]), Microsoft.VisualBasic.VbStrConv.ProperCase, 0);
                                else
                                    return "User";
                            }

                            groupType = (int)directoryEntry.Properties["groupType"].Value;
                        }
                        catch
                        {
                            return "Invalid!";
                        }

                        try
                        {
                            if (!directoryEntry.Properties.Contains("groupType")) return "User";
                        }
                        catch { }

                        string sGroupType = "";
                        if ((groupType & (uint)ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP) != 0)
                        {
                            sGroupType = "Global";
                        }
                        else if ((groupType & (uint)ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP) != 0)
                        {
                            sGroupType = "Local";
                        }
                        else if ((groupType & (uint)ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_UNIVERSAL_GROUP) != 0)
                        {
                            sGroupType = "Universal";
                        }

                        if (directoryEntry.Path.ToUpper().StartsWith("WINNT:"))
                        {
                            return sGroupType; //no such thing for local computer accounts
                        }
                        else if ((groupType & (uint)ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED) != 0)
                        {
                            sGroupType += "/Security";
                        }
                        else
                        {
                            sGroupType += "/Distribution";
                        }
                        return sGroupType;
                    }
                    catch (Exception ex)
                    {
                        return ex.Message + "\r\n" + ex.StackTrace;
                    }
                }
            }

            public string MemberDisabled
            {
                get
                {
                    if (IsBuiltIn) return "Enabled";
                    int userStatus = 0;
                    try
                    {
                        if (directoryEntry.Properties["userAccountControl"].Value != null)
                            userStatus = (int)directoryEntry.Properties["userAccountControl"].Value; //for LDAP
                        else if (directoryEntry.Properties["userFlags"].Value != null)
                            userStatus = (int)directoryEntry.Properties["userFlags"].Value; //for local computer accounts via WinNT
                    }
                    catch
                    {
                        return "Unknown";
                    }

                    if ((int)(((int)userStatus) & (int)UserAccountControl.AccountDisabled) == (int)UserAccountControl.AccountDisabled)
                    {
                        return "Disabled";
                    }
                    return "Enabled";
                }
            }

            public string MemberIsValid
            {
                get
                {
                    if (IsBuiltIn) return null;
                    string sGroupType = GroupType;
                    if (sGroupType.EndsWith("!"))
                        return "Member not found!";
                    else if (sGroupType.EndsWith("/Distribution"))
                        return "SSAS ignores distribution groups";
                    else if (MemberDisabled == "Disabled" && GroupType != "User" && GroupType != "Computer")
                        return "Member is disabled!";
                    return null;
                }
            }

            internal enum UserAccountControl
            {
                Script = 1,
                AccountDisabled = 2,
                HomeDirectoryRequired = 8,
                AccountLockedOut_DEPRECATED = 16,
                PasswordNotRequired = 32,
                PasswordCannotChange_DEPRECATED = 64,
                EncryptedTextPasswordAllowed = 128,
                TempDuplicateAccount = 256,
                NormalAccount = 512,
                InterDomainTrustAccount = 2048,
                WorkstationTrustAccount = 4096,
                ServerTrustAccount = 8192,
                PasswordDoesNotExpire = 65536,
                MnsLogonAccount = 131072,
                SmartCardRequired = 262144,
                TrustedForDelegation = 524288,
                AccountNotDelegated = 1048576,
                UseDesKeyOnly = 2097152,
                DontRequirePreauth = 4194304,
                PasswordExpired_DEPRECATED = 8388608,
                TrustedToAuthenticateForDelegation = 16777216,
                NoAuthDataRequired = 33554432
            }
        }
        #endregion

        #region RoleDataSourceInfo class
        public class RoleDataSourceInfo
        {
            public Role role;
            public DataSource dataSource;

            public string DataSourceName
            {
                get { return dataSource.Name; }
            }

            public string RoleName
            {
                get { return role.Name; }
            }

            public string ReadAccess
            {
                get
                {
                    DatabasePermission dbPerm = dataSource.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    DataSourcePermission perm = dataSource.DataSourcePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return perm.Read.ToString();
                }
            }

            public string ReadDefinitionAccess
            {
                get
                {
                    DatabasePermission dbPerm = dataSource.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.ReadDefinition == Microsoft.AnalysisServices.ReadDefinitionAccess.Allowed) return "Allowed";
                    DataSourcePermission perm = dataSource.DataSourcePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return perm.ReadDefinition.ToString();
                }
            }
        }
        #endregion

        #region RoleCubeInfo class
        public class RoleCubeInfo
        {
            public Role role;
            public Cube cube;

            public string CubeName
            {
                get { return cube.Name; }
            }

            public string RoleName
            {
                get { return role.Name; }
            }

            public string ReadAccess
            {
                get
                {
                    DatabasePermission dbPerm = cube.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    CubePermission perm = cube.CubePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed && perm.Write == Microsoft.AnalysisServices.WriteAccess.Allowed)
                        return "Read/Write";
                    else if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed)
                        return "Read";
                    else
                        return "None";
                }
            }

            public string ReadDefinitionAccess
            {
                get
                {
                    DatabasePermission dbPerm = cube.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.ReadDefinition == Microsoft.AnalysisServices.ReadDefinitionAccess.Allowed) return "Allowed";
                    CubePermission perm = cube.CubePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    if (perm.ReadDefinition == Microsoft.AnalysisServices.ReadDefinitionAccess.Basic) return "Basic (Local Cube)";
                    return perm.ReadDefinition.ToString();
                }
            }

            public string ProcessAccess
            {
                get
                {
                    DatabasePermission dbPerm = cube.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.Process) return "Process";
                    CubePermission perm = cube.CubePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return (perm.Process ? "Process" : "None");
                }
            }

            public string DrillthroughAccess
            {
                get
                {
                    CubePermission perm = cube.CubePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return perm.ReadSourceData.ToString();
                }
            }

            public string CellSecurityReadPermissions
            {
                get
                {
                    return GetCellSecurityPermission(CellPermissionAccess.Read);
                }
            }

            public string CellSecurityReadContingentPermissions
            {
                get
                {
                    return GetCellSecurityPermission(CellPermissionAccess.ReadContingent);
                }
            }

            public string CellSecurityReadWritePermissions
            {
                get
                {
                    return GetCellSecurityPermission(CellPermissionAccess.ReadWrite);
                }
            }

            private string GetCellSecurityPermission(CellPermissionAccess type)
            {
                CubePermission perm = cube.CubePermissions.FindByRole(role.ID);
                if (perm == null) return null;
                foreach (CellPermission cp in perm.CellPermissions)
                {
                    if (cp.Access == type)
                    {
                        return cp.Expression;
                    }
                }
                return null;
            }
        }
        #endregion

        #region RoleDimensionInfo class
        public class RoleDimensionInfo
        {
            public Role role;
            public Dimension dimension;
            public CubeDimension cubeDimension;
            public DimensionAttribute dimensionAttribute;

            public string DimensionName
            {
                get { return dimension.Name; }
            }

            public string CubeDimensionName
            {
                get
                {
                    if (cubeDimension == null) return dimension.Name;
                    if (cubeDimension.Name == dimension.Name) return cubeDimension.Name + " (" + cubeDimension.Parent.Name + ")";
                    else return cubeDimension.Name + " (" + dimension.Name + " - " + cubeDimension.Parent.Name + ")";
                }
            }

            public string RoleName
            {
                get { return role.Name; }
            }

            public string ReadAccess
            {
                get
                {
                    if (cubeDimension != null && cubeDimension.Parent.CubePermissions != null)
                    {
                        CubePermission cubePerm = cubeDimension.Parent.CubePermissions.FindByRole(role.ID);
                        if (cubePerm != null && cubePerm.DimensionPermissions != null)
                        {
                            CubeDimensionPermission cubeDimPerm = cubePerm.DimensionPermissions.Find(cubeDimension.ID);
                            if (cubeDimPerm != null)
                            {
                                if (cubeDimPerm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed && cubeDimPerm.Write == Microsoft.AnalysisServices.WriteAccess.Allowed)
                                    return "Read/Write";
                                else if (cubeDimPerm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed)
                                    return "Read";
                                else
                                    return "None";
                            }
                            else
                            {
                                return "Inherited";
                            }
                        }
                        else
                        {
                            return "Inherited";
                        }
                    }
                    else
                    {
                        DatabasePermission dbPerm = dimension.Parent.DatabasePermissions.FindByRole(role.ID);
                        if (dbPerm != null && dbPerm.Administer) return "Admin";
                        if (dimension.DimensionPermissions == null) return "None";
                        DimensionPermission perm = dimension.DimensionPermissions.FindByRole(role.ID);
                        if (perm == null) return "Read";
                        if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed && perm.Write == Microsoft.AnalysisServices.WriteAccess.Allowed)
                            return "Read/Write";
                        else if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed)
                            return "Read";
                        else
                            return "None";
                    }
                }
            }

            public string ReadDefinitionAccess
            {
                get
                {
                    if (cubeDimension != null)
                    {
                        return null;
                    }
                    else
                    {
                        DatabasePermission dbPerm = dimension.Parent.DatabasePermissions.FindByRole(role.ID);
                        if (dbPerm != null && dbPerm.Administer) return "Admin";
                        if (dbPerm != null && dbPerm.ReadDefinition == Microsoft.AnalysisServices.ReadDefinitionAccess.Allowed) return "Allowed";
                        if (dimension.DimensionPermissions == null) return "None";
                        DimensionPermission perm = dimension.DimensionPermissions.FindByRole(role.ID);
                        if (perm == null) return "None";
                        return perm.ReadDefinition.ToString();
                    }
                }
            }

            public string ProcessAccess
            {
                get
                {
                    DatabasePermission dbPerm = dimension.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.Process) return "Process";
                    if (dimension.DimensionPermissions == null) return "None";
                    DimensionPermission perm = dimension.DimensionPermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return (perm.Process ? "Process" : "None");
                }
            }

            public string AttributeName
            {
                get
                {
                    if (dimensionAttribute == null) return null;
                    else return dimensionAttribute.Name;
                }
            }

            public string VisualTotals
            {
                get
                {
                    AttributePermission perm = GetDimensionAttributeSecurityPermission();
                    return (perm == null || perm.VisualTotals != "1") ? null : "Visual Totals Enabled";
                }
            }

            public string AllowedMemberSet
            {
                get
                {
                    AttributePermission perm = GetDimensionAttributeSecurityPermission();
                    return (perm == null || string.IsNullOrEmpty(perm.AllowedSet)) ? null : perm.AllowedSet;
                }
            }

            public string DeniedMemberSet
            {
                get
                {
                    AttributePermission perm = GetDimensionAttributeSecurityPermission();
                    return (perm == null || string.IsNullOrEmpty(perm.DeniedSet)) ? null : perm.DeniedSet;
                }
            }

            public string DefaultMember
            {
                get
                {
                    AttributePermission perm = GetDimensionAttributeSecurityPermission();
                    return (perm == null || string.IsNullOrEmpty(perm.DefaultMember)) ? null : perm.DefaultMember;
                }
            }


            private AttributePermission GetDimensionAttributeSecurityPermission()
            {
                if (dimensionAttribute == null) return null;
                if (cubeDimension != null && cubeDimension.Parent.CubePermissions != null)
                {
                    CubePermission cubePerm = cubeDimension.Parent.CubePermissions.FindByRole(role.ID);
                    if (cubePerm != null)
                    {
                        CubeDimensionPermission cubeDimPerm = cubePerm.DimensionPermissions.Find(cubeDimension.ID);
                        if (cubeDimPerm != null && cubeDimPerm.AttributePermissions != null)
                        {
                            return cubeDimPerm.AttributePermissions.Find(dimensionAttribute.ID);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    DimensionPermission perm = dimension.DimensionPermissions.FindByRole(role.ID);
                    if (perm == null || perm.AttributePermissions == null) return null;
                    return perm.AttributePermissions.Find(dimensionAttribute.ID);
                }
            }
        }
        #endregion

        #region RoleMiningInfo class
        public class RoleMiningInfo
        {
            public Role role;
            public MiningStructure miningStructure;
            public MiningModel miningModel;

            public string MiningStructureName
            {
                get { return miningStructure.Name; }
            }

            public string MiningModelName
            {
                get { return miningModel == null ? null : miningModel.Name; }
            }

            public string RoleName
            {
                get { return role.Name; }
            }

            public string ReadAccess
            {
                get
                {
                    DatabasePermission dbPerm = miningStructure.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    Permission perm;
                    if (miningModel != null)
                        perm = miningModel.MiningModelPermissions.FindByRole(role.ID);
                    else
                        perm = miningStructure.MiningStructurePermissions.FindByRole(role.ID);

                    if (perm == null) return "None";
                    if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed && perm.Write == Microsoft.AnalysisServices.WriteAccess.Allowed)
                        return "Read/Write";
                    else if (perm.Read == Microsoft.AnalysisServices.ReadAccess.Allowed)
                        return "Read";
                    else
                        return "None";
                }
            }

            public string ReadDefinitionAccess
            {
                get
                {
                    DatabasePermission dbPerm = miningStructure.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.ReadDefinition == Microsoft.AnalysisServices.ReadDefinitionAccess.Allowed) return "Allowed";
                    Permission perm = null;
                    if (miningModel != null)
                        perm = miningModel.MiningModelPermissions.FindByRole(role.ID);
                    else
                        perm = miningStructure.MiningStructurePermissions.FindByRole(role.ID);

                    if (perm == null) return "None";
                    return perm.ReadDefinition.ToString();
                }
            }

            public string ProcessAccess
            {
                get
                {
                    DatabasePermission dbPerm = miningStructure.Parent.DatabasePermissions.FindByRole(role.ID);
                    if (dbPerm != null && dbPerm.Administer) return "Admin";
                    if (dbPerm != null && dbPerm.Process) return "Process";
                    Permission perm = null;
                    if (miningModel != null)
                        perm = miningModel.MiningModelPermissions.FindByRole(role.ID);
                    else
                        perm = miningStructure.MiningStructurePermissions.FindByRole(role.ID);
                    if (perm == null) return "None";
                    return (perm.Process ? "Process" : "None");
                }
            }

            public string DrillthroughBrowseAccess
            {
                get
                {
                    if (miningModel != null)
                    {
                        MiningModelPermission perm = miningModel.MiningModelPermissions.FindByRole(role.ID);
                        if (perm == null) return "None";
                        return perm.AllowBrowsing ? "Browse" : "None";
                    }
                    else
                    {
                        MiningStructurePermission perm = miningStructure.MiningStructurePermissions.FindByRole(role.ID);
                        if (perm == null) return "None";
#if KATMAI
                        return perm.AllowDrillThrough ? "Drillthrough" : "None";
#else
                        return "None";
#endif
                    }
                }
            }
        }
        #endregion
    }
}