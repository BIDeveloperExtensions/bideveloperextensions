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
        // Fields
        private ERDiagram erDiagram;

        // Methods
        public ERDiagramActionBase(ERDiagram diagramInput)
            : base(diagramInput)
        {
            this.erDiagram = diagramInput;
        }

        public override IDiagramActionInstance CreateActionInstance(IEnumerable<IDiagramObject> targets, bool forceExecutionWithConfirmSkipped)
        {
            DiagramActionInstance instance = new DiagramActionInstance(this);
            instance.Targets = targets;
            instance.ForceExecutionWithConfirmSkipped = forceExecutionWithConfirmSkipped;
            return instance;
        }

        // Properties
        protected ERDiagram ERDiagram
        {
            get
            {
                return this.erDiagram;
            }
        }
    }


    internal class DiagramActionInstance : IDiagramActionInstance
    {
        // Fields
        private IDiagramAction actionType;
        private object confirmedOption;
        private bool isCancelledFromConfirm;
        private HashSet<IDiagramObject> targets = new HashSet<IDiagramObject>();

        // Methods
        public DiagramActionInstance(IDiagramAction actionTypeInput)
        {
            this.actionType = actionTypeInput;
        }

        public virtual bool Equals(IDiagramActionInstance actionInstanceToCompare)
        {
            if (actionInstanceToCompare.ActionType != this.ActionType)
            {
                return false;
            }
            int num = this.Targets.Count<IDiagramObject>();
            int num2 = actionInstanceToCompare.Targets.Count<IDiagramObject>();
            if (num != num2)
            {
                return false;
            }
            if (actionInstanceToCompare.Targets.Intersect<IDiagramObject>(this.Targets).Count<IDiagramObject>() != num)
            {
                return false;
            }
            return true;
        }

        public void SetConfirmationResult(bool isCancelled, object selectedOption)
        {
            this.isCancelledFromConfirm = isCancelled;
            this.confirmedOption = selectedOption;
        }

        // Properties
        public IDiagramAction ActionType
        {
            get
            {
                return this.actionType;
            }
        }

        public object ConfirmedOption
        {
            get
            {
                return this.confirmedOption;
            }
        }

        public bool ForceExecutionWithConfirmSkipped
        {
            get
            {
                return false;
            }
            set
            {
                bool x = value;
            }
        }

        public bool IsCancelledFromConfirmation
        {
            get
            {
                return this.isCancelledFromConfirm;
            }
        }

        public IEnumerable<IDiagramObject> Targets
        {
            get
            {
                return this.targets;
            }
            set
            {
                this.targets.Clear();
                foreach (IDiagramObject obj2 in value)
                {
                    this.targets.Add(obj2);
                }
            }
        }
    }

}
