using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public class TabularDisplayFolder : System.IComparable
    {
        private bool _dirty = false;
        private IModelComponent _object;
        public TabularDisplayFolder(string table, IModelComponent obj)
        {
            _Table = table;
            _object = obj;
            if (obj is DimensionAttribute)
            {
                _DisplayFolder = ((DimensionAttribute)obj).AttributeHierarchyDisplayFolder;
                _ObjectType = TabularDisplayFolderType.Column;
                _ObjectName = ((DimensionAttribute)obj).Name;
            }
            else if (obj is Hierarchy)
            {
                _DisplayFolder = ((Hierarchy)obj).DisplayFolder;
                _ObjectType = TabularDisplayFolderType.Hierarchy;
                _ObjectName = ((Hierarchy)obj).Name;
            }
            else if (obj is CalculationProperty)
            {
                _DisplayFolder = ((CalculationProperty)obj).DisplayFolder;
                _ObjectType = TabularDisplayFolderType.Measure;
                _ObjectName = ((CalculationProperty)obj).CalculationReference;

                if (_ObjectName.StartsWith("[") && _ObjectName.EndsWith("]"))
                {
                    _ObjectName = _ObjectName.Substring(1, _ObjectName.Length - 2);
                }
            }
            else
            {
                throw new Exception("Unexpected object type: " + obj.GetType().Name);
            }
        }

        public void SaveDisplayFolder()
        {
            if (Dirty)
            {
                if (_object is DimensionAttribute)
                {
                    ((DimensionAttribute)_object).AttributeHierarchyDisplayFolder = _DisplayFolder;
                }
                else if (_object is Hierarchy)
                {
                    ((Hierarchy)_object).DisplayFolder = _DisplayFolder;
                }
                else if (_object is CalculationProperty)
                {
                    ((CalculationProperty)_object).DisplayFolder = _DisplayFolder;
                }
                else
                {
                    throw new Exception("Unexpected object type: " + _object.GetType().Name);
                }
            }
        }

        private string _Table;
        public string Table
        {
            get { return _Table; }
        }

        private TabularDisplayFolderType _ObjectType;
        public TabularDisplayFolderType ObjectType
        {
            get
            {
                return _ObjectType;
            }
        }

        private string _ObjectName;
        public string ObjectName
        {
            get
            {
                return _ObjectName;
            }
        }

        internal string ObjectID
        {
            get
            {
                if (_object is NamedComponent)
                {
                    return ((NamedComponent)_object).ID;
                }
                else
                {
                    return _ObjectName;
                }
            }
        }


        private string _DisplayFolder;
        public string DisplayFolder
        {
            get
            {
                return _DisplayFolder;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;

                if (_DisplayFolder != value)
                {
                    _dirty = true;
                }
                _DisplayFolder = value;
            }
        }

        internal bool Dirty
        {
            get { return _dirty; }
        }

        public int CompareTo(object a)
        {
            TabularDisplayFolder x = (TabularDisplayFolder)this;
            TabularDisplayFolder y = (TabularDisplayFolder)a;
            int iTableCompare = x._Table.CompareTo(y._Table);
            if (iTableCompare != 0)
            {
                return iTableCompare;
            }
            else
            {
                return x._ObjectName.CompareTo(y._ObjectName);
            }
        }


    }


    public enum TabularDisplayFolderType
    {
        Column,
        Measure,
        Hierarchy
    }
}
