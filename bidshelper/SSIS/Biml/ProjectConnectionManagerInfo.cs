using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDSHelper.SSIS.Biml
{
    class ProjectConnectionManagerInfo
    {
        private string _objectName;
        public string ObjectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        private string _DTSID;
        public string DTSID
        {
            get { return _DTSID; }
            set { _DTSID = value; }
        }

        private string _fileFullPath;
        public string FileFullPath
        {
            get { return _fileFullPath; }
            set { _fileFullPath = value; }
        }

        public ProjectConnectionManagerInfo(string fileFullPath, string objectName, string DTSID)
        {
            _fileFullPath = fileFullPath;
            _objectName = objectName;
            _DTSID = DTSID;
        }
    }
}
