 using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;
using System.Xml;
using System.IO;

namespace BIDSHelper.SSAS
{
    public class EnhancedDeployEngine
    {
        public static void Deploy(Database database, PartitionEnhancedDeployModes partMode, RoleEnhancedDeployModes roleMode)
        {
            string script = BuildDeployScript(database, partMode, roleMode);
            
            // TODO execute Script

            //FileStream fs = new FileStream("c:\\data\\projects\\bidshelper\\bidshelper\\enhancedDeployTest.xmla",FileMode.Append);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Write(script);
            //sw.Flush();
            //fs.Close();
        }

        public static string BuildDeployScript(Database database,PartitionEnhancedDeployModes partMode,RoleEnhancedDeployModes roleMode)
        {

            //string script = "";
            Database dbCopy = database.Clone();
            Scripter scr = new Scripter();

            if (partMode == PartitionEnhancedDeployModes.NoPartitionsDeployed ||
                partMode == PartitionEnhancedDeployModes.DeployButKeepExisting)
                {
                    foreach (Cube c in  dbCopy.Cubes)
                    {
                        foreach(MeasureGroup mg in c.MeasureGroups)
                        {
                            mg.Partitions.Clear();
                        }
                    }
                }

            if (roleMode == RoleEnhancedDeployModes.NoRolesDeployed ||
                roleMode == RoleEnhancedDeployModes.DeployButKeepExisting)
                {
                    dbCopy.Roles.Clear();
                }

            StringBuilder sb = new StringBuilder();

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.Encoding = Encoding.UTF8;
            xws.OmitXmlDeclaration = true;

            //XmlWriter xw = XmlWriter.Create(sb,xws);
            XmlWriter xw = XmlWriter.Create("c:\\data\\EnhancedDeploy.xmla",xws);

            Scripter.WriteStartBatch(xw, true);
            // script roles first as the other objects may have a dependency on 
            // a new role.
            if (roleMode == RoleEnhancedDeployModes.DeployButKeepExisting)
            {
                foreach (Role r in database.Roles)
                {
                    Scripter.WriteAlter(xw,r,true,true);
                }
            }

            // script out dbcopy
            Scripter.WriteAlter(xw, dbCopy, true, true);

            // script out partitions
            if (partMode == PartitionEnhancedDeployModes.DeployButKeepExisting)
            {
                foreach (Cube c in database.Cubes)
                {
                    foreach (MeasureGroup mg in c.MeasureGroups)
                    {
                        foreach (Partition p in mg.Partitions)
                        {
                            //script partition as create/AlterIfExists
                        }
                    }
                }
            }
            Scripter.WriteEndBatch(xw);
            xw.Flush();
            //sb.ToString();
            xw.Close();

            //TODO - need to replace object expansion tag if we are keeping existing

            return sb.ToString();
        }
    }


    public enum PartitionEnhancedDeployModes
    {
        NoPartitionsDeployed,
        DeployButKeepExisting,
        DeployOverwriteExisting
    }

    public enum RoleEnhancedDeployModes
    {
        NoRolesDeployed,
        DeployButKeepExisting,
        DeployAndOverwriteExisting
    }
}
