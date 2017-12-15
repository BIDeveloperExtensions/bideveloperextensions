using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDSHelper.Core
{
    
    public class DirtyMonitoredDictionary<TKey, TValue> : Dictionary<int, string>
    {
        private bool _dirty = false;
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
        }

        public new string this[int key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else
                    return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;

                if (!base.ContainsKey(key) || base[key] != value)
                {
                    _dirty = true;
                }

                base[key] = value;
            }
        }
    }
}
