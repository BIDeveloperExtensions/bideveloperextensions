#if !DENALI && !SQL2014
extern alias localAdomdClient;
using AdomdLocal = localAdomdClient.Microsoft.AnalysisServices.AdomdClient;
#endif

using BIDSHelper.Core;
using System.Collections.Generic;
using System.Data;

namespace BIDSHelper.SSAS
{
    public class DataModelingSandboxWrapper
    {
        private Microsoft.AnalysisServices.BackEnd.DataModelingSandbox _sandbox;

        public DataModelingSandboxWrapper(BIDSHelperPluginBase plugin)
        {
            _sandbox = TabularHelpers.GetTabularSandboxFromBimFile(plugin, true);
        }
        public DataModelingSandboxWrapper(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            _sandbox = sandbox;
        }

        public Microsoft.AnalysisServices.BackEnd.DataModelingSandbox GetSandbox()
        {
            return _sandbox;
        }

        public bool IsTabularMetadata
        {
            get
            {
                if (GetSandbox() == null) throw new System.Exception("Can't get Sandbox!");
                return GetSandbox().IsTabularMetadata;
            }
        }

        public int DatabaseCompatibilityLevel
        {
            get
            {
                if (GetSandbox() == null) throw new System.Exception("Can't get Sandbox!");
                return GetSandbox().DatabaseCompatibilityLevel;
            }
        }

#if DENALI || SQL2014
        public Microsoft.AnalysisServices.AdomdClient.AdomdConnection GetAdomdConnection()
        {
            return _sandbox.AdomdConnection;
        }
#else
        public AdomdLocal.AdomdConnection GetAdomdConnection()
        {
            return _sandbox.AdomdConnection;
        }
#endif

#if !DENALI && !SQL2014
        public Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo GetSandboxAmo()
        {

            if (!_sandbox.IsTabularMetadata)
                return (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)_sandbox.Impl;

            return null;

        }
#else
        public Microsoft.AnalysisServices.BackEnd.DataModelingSandbox GetSandboxAmo()
        {
            return _sandbox;
        }
#endif

        public Microsoft.AnalysisServices.Cube Cube
        {
            get
            {
#if !DENALI && !SQL2014
                return ((Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)_sandbox.Impl).Cube;
#else
                return _sandbox.Cube;
#endif
            }
        }

        public Microsoft.AnalysisServices.BackEnd.IDataModelingMeasureCollection Measures
        {
            get
            {
#if !DENALI && !SQL2014
                return _sandbox.Measures;
#else
                return _sandbox.Measures;
#endif
            }
        }

    public DataSet GetSchemaDataSet(string schemaName, Dictionary<string,string> restrictions )
        {

#if DENALI || SQL2014
            var res = new Microsoft.AnalysisServices.AdomdClient.AdomdRestrictionCollection();
#else
            var res = new localAdomdClient.Microsoft.AnalysisServices.AdomdClient.AdomdRestrictionCollection();
#endif

            foreach (string key in restrictions.Keys)
            {
                res.Add(key, restrictions[key]);
            }
            //System.Guid schemaId = new System.Guid();
            //switch (schemaName)
            //{
            //    case "MDSCHEMA_MEMBERS":
            //        schemaId = AdomdLocal.AdomdSchemaGuid.Members;
            //        break;
            //    case "MDSCHEMA_MEASURES":
            //        schemaId = AdomdLocal.AdomdSchemaGuid.Measures;
            //        break;
            //}
            //if (schemaId == System.Guid.Empty) throw new System.Exception("Unknown schemaName");
            
            return _sandbox.AdomdConnection.GetSchemaDataSet(schemaName, res);
        }

    }
}