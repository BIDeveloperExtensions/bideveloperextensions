using BIDSHelper.SSIS;
using Microsoft.DataWarehouse.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core
{
    public class EditorWindowTag
    {
        public bool ExpressionHighlighter
        {
            get;
            set;
        }

        public ParametersWindowManager ParametersWindowManager
        {
            get;
            set;
        }
    }

    public static class EditorWindowTagExtensions
    {
        public static bool SetTagExpressionHighlighterPlugin(this EditorWindow editorWindow)
        {
            EditorWindowTag tag = editorWindow.Tag as EditorWindowTag;
            if (tag == null)
            {
                if (editorWindow.Tag == null)
                {
                    tag = new EditorWindowTag();
                    tag.ExpressionHighlighter = true;
                    editorWindow.Tag = tag;

                    // Property has been set True, so return result of True to indicate all was well
                    return true;
                }
                else
                {
                    // Tag is set, but not the expected type. Safety check to see if anyone else is using the Tag on the DtsPackageView
                    throw new Exception(string.Format("DtsPackageView tag is unexpected type, {0}", editorWindow.Tag));
                }
            }
            else
            {
                if (tag.ExpressionHighlighter)
                {
                    // Tag property ExpressionHighlighter was already set to True, so retunr False to indicate that it did not need settings.
                    return false;
                }
                else
                {
                    tag.ExpressionHighlighter = true;

                    // Property has been set True, so return result of True to indicate all was well
                    return true;
                }
            }
        }

        public static bool SetTagParametersWindowManager(this EditorWindow editorWindow)
        {
            EditorWindowTag tag = editorWindow.Tag as EditorWindowTag;
            if (tag == null)
            {
                if (editorWindow.Tag == null)
                {
                    tag = new EditorWindowTag();
                    editorWindow.Tag = tag;
                }
                else
                {
                    // Tag is set, but not the expected type. Safety check to see if anyone else is using the Tag on the DtsPackageView
                    throw new Exception(string.Format("DtsPackageView tag is unexpected type, {0}", editorWindow.Tag));
                }
            }


            if (tag.ParametersWindowManager == null)
            {
                tag.ParametersWindowManager = new ParametersWindowManager(editorWindow);

                // Property has been set True, so return result of True to indicate all was well
                return true;
            }
            else
            {
                // Tag property ParametersWindowManager was already set to True, so return False to indicate that it did not need settings.
                return false;
            }
        }
    }
}
