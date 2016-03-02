using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AnalysisServices.Common;
using System.ComponentModel;

namespace BIDSHelper.SSAS.Tabular
{
    //this is the base class for a context menu item in the Tabular diagram editor
    internal abstract class ERDiagramActionBase : DiagramActionBasic
    {
        private static Type TYPE_DIAGRAM_ACTION_INSTANCE = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(typeof(Microsoft.AnalysisServices.Common.IDiagramActionInstance), "Microsoft.AnalysisServices.Common.DiagramActionInstance");

        // Fields
        private ERDiagram erDiagram;

        // Methods
        public ERDiagramActionBase(ERDiagram diagramInput)
            : base(diagramInput)
        {
            this.erDiagram = diagramInput;
        }

#if SQL2014 || (DENALI && DEBUG) || NEWER_DENALI
        //after SQL2012 RTM at some point this Microsoft.AnalysisServices.Common.DiagramActionBasic method changed to not include the second parameter?
        //so for SQL2014 compile without the second parameter
        //for SQL2012 on the build server (which is RTM) compile with the second parameter
        //for SQL2012 on our laptop (DEBUG) skip the second parameter
        //GG 20150112 changed this to use reflection so we could avoid having to build our own DiagramActionInstance class since the fact that SQL 2012 RTM has a ForceExecutionWithConfirmSkipped property and later doesn't was causing problems during the launch of visual studio... builds on my laptop would cause errors if run on RTM... the reflection below resolves that
        public override IDiagramActionInstance CreateActionInstance(IEnumerable<IDiagramObject> targets) //, bool forceExecutionWithConfirmSkipped)
        {
            System.Reflection.ConstructorInfo constructor = TYPE_DIAGRAM_ACTION_INSTANCE.GetConstructor(new Type[] { typeof(IDiagramAction) });
            IDiagramActionInstance instance = (IDiagramActionInstance)constructor.Invoke(new object[] { this });
            TYPE_DIAGRAM_ACTION_INSTANCE.InvokeMember("Targets", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, instance, new object[] { targets });
            return instance;
        }
#elif DENALI
        public override IDiagramActionInstance CreateActionInstance(IEnumerable<IDiagramObject> targets, bool forceExecutionWithConfirmSkipped)
        {
            System.Reflection.ConstructorInfo constructor = TYPE_DIAGRAM_ACTION_INSTANCE.GetConstructor(new Type[] { typeof(IDiagramAction) });
            IDiagramActionInstance instance = (IDiagramActionInstance)constructor.Invoke(new object[] { this });
            TYPE_DIAGRAM_ACTION_INSTANCE.InvokeMember("Targets", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, instance, new object[] { targets });
            return instance;
        }
#endif

        // Properties
        protected ERDiagram ERDiagram
        {
            get
            {
                return this.erDiagram;
            }
        }
    }

}
