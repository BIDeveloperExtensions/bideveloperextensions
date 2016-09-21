extern alias localAdomdClient;

using BIDSHelper.Core;
using System.Collections.Generic;
using System.Data;
using AdomdLocal = localAdomdClient.Microsoft.AnalysisServices.AdomdClient;

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

        public Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo GetSandboxAmo()
        {
#if !DENALAI && !SQL2014
            if (!_sandbox.IsTabularMetadata)
                return (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)_sandbox.Impl;
#endif
            return null;

        }

        public DataSet GetSchemaDataSet(string schemaName, Dictionary<string,string> restrictions )
        {
            var res = new localAdomdClient.Microsoft.AnalysisServices.AdomdClient.AdomdRestrictionCollection();
            foreach (string key in restrictions.Keys) {
                res.Add(key, restrictions[key]);
            }

            System.Guid schemaId = new System.Guid();
            switch (schemaName)
            {
                case "MDSCHEMA_MEMBERS":
                    schemaId = AdomdLocal.AdomdSchemaGuid.Members;
                    break;
                case "MDSCHEMA_MEASURES":
                    schemaId = AdomdLocal.AdomdSchemaGuid.Measures;
                    break;
            }
            if (schemaId == System.Guid.Empty) throw new System.Exception("Unknown schemaName");
            return _sandbox.AdomdConnection.GetSchemaDataSet(schemaName, res );// .GetSchemaDataSet(schemaId, restrictions);
        }

    }
}