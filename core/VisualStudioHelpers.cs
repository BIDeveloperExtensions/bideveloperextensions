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
    }
}
