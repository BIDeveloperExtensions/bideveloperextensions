using System;
using System.Collections.Generic;
using System.Text;
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace BIDSHelper
{
    internal class VisualStudioHelpers
    {
        public static UIHierarchyItem[] GetAllItemsFromSolutionExplorer(UIHierarchy solExplorer)
        {
            return RecurseUIHierarchyItems(solExplorer.UIHierarchyItems);
        }

        private static UIHierarchyItem[] RecurseUIHierarchyItems(UIHierarchyItems items)
        {
            List<UIHierarchyItem> list = new List<UIHierarchyItem>();
            if (items != null)
            {
                foreach (UIHierarchyItem hierItem in items)
                {
                    list.Add(hierItem);
                    list.AddRange(RecurseUIHierarchyItems(hierItem.UIHierarchyItems));
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Returns true if we are in VS2012 or greater. Useful for deciding on certain behaviors between VS2012 and VS2010.
        /// </summary>
        /// <param name="win"></param>
        /// <returns></returns>
        public static bool IsMetroOrGreater(Microsoft.DataWarehouse.Design.EditorWindow win)
        {
            try
            {
                if (win.EnvironmentService != null)
                {
                    bool bIsMetroOrGreater = (bool)win.EnvironmentService.GetType().InvokeMember("IsAppMetroOrGreater", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, win.EnvironmentService, null);
                    return bIsMetroOrGreater;
                }
            }
            catch { }
            return false;
        }
    }
}
