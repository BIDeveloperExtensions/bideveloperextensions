using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.SSIS
{
    internal class FindVariables
    {
        private string[] expressionMatches = null;
        private string[] properytMatches = null;

        public event EventHandler<VariableFoundEventArgs> VariableFound;

        public bool CancellationPending { get; private set; }

        public bool Cancel()
        {
            if (this.CancellationPending)
            {
                return false;
            }

            this.CancellationPending = true;
            return true;
        }

        public void FindReferences(Package package, Variable variable)
        {
            // Reset cancel pending flag
            this.CancellationPending = false;

            List<string> expressions = new List<string>();

            // Variable formats in an expression - @Variable, @[Variable], @[Namespace::Variable]
            expressions.Add(string.Format("@{0}", variable.Name));
            expressions.Add(string.Format("@[{0}]", variable.Name));
            expressions.Add(string.Format("@[{0}]", variable.QualifiedName));
            this.expressionMatches = expressions.ToArray();

            List<string> properties = new List<string>();
            properties.Add(variable.QualifiedName);
            this.properytMatches = properties.ToArray();

            ProcessObject(package, string.Empty);

            // Reset cancel pending in case we have cancelled, as about to exit, so no longer pending
            this.CancellationPending = false;
        }

        public void FindReferences(Package package, Variable[] variables)
        {
            // Reset cancel pending flag
            this.CancellationPending = false;

            List<string> expressions = new List<string>();

            foreach (Variable variable in variables)
            {
                // Variable formats in an expression - @Variable, @[Variable], @[Namespace::Variable]
                expressions.Add(string.Format("@{0}", variable.Name));
                expressions.Add(string.Format("@[{0}]", variable.Name));
                expressions.Add(string.Format("@[{0}]", variable.QualifiedName));
                this.expressionMatches = expressions.ToArray();

                List<string> properties = new List<string>();
                properties.Add(variable.QualifiedName);
                this.properytMatches = properties.ToArray();
            }

            ProcessObject(package, string.Empty);

            // Reset cancel pending in case we have cancelled, as about to exit, so no longer pending
            this.CancellationPending = false;
        }

        private void ProcessObject(object component, string path)
        {
            if (this.CancellationPending)
            {
                return;
            }

            DtsContainer container = component as DtsContainer;

            // Should only get package as we call GetPackage up front. Could make scope like, but need UI indicator that this is happening
            Package package = component as Package;
            if (package != null)
            {
                path = "\\Package";
                CheckConnectionManagers(package, path);
            }
            else if (!(component is DtsEventHandler))
            {
                path = path + "\\" + container.Name;
            }

            IDTSPropertiesProvider propertiesProvider = component as IDTSPropertiesProvider;
            if (propertiesProvider != null)
            {
                CheckProperties(propertiesProvider, path);
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    ProcessObject(eventhandler, path + ".EventHandlers[" + eventhandler.Name + "]");
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                ProcessSequence(container, sequence, path);
                ScanPrecedenceConstraints(path, container.ID, sequence.PrecedenceConstraints);
            }
        }

        private void ProcessSequence(DtsContainer container, IDTSSequence sequence, string path)
        {
            if (this.CancellationPending)
            {
                return;
            }

            foreach (Executable executable in sequence.Executables)
            {
                ProcessObject(executable, path);
            }
        }

        private void CheckConnectionManagers(Package package, string path)
        {
            if (this.CancellationPending) return;

            foreach (ConnectionManager cm in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                // TODO; Fix - Cheat and hard code creation name as icon routines cannot get the correct connection icon
                ScanProperties(path + ".Connections[" + cm.Name + "].", typeof(ConnectionManager), cm.GetType().Name, package.ID, cm.ID, cm.Name, (IDTSPropertiesProvider)cm, PackageHelper.ConnectionCreationName);
            }
        }

        private void CheckProperties(IDTSPropertiesProvider propProvider, string path)
        {
            if (this.CancellationPending) return;

            if (propProvider is DtsContainer)
            {
                DtsContainer container = (DtsContainer)propProvider;
                string containerKey = PackageHelper.GetContainerKey(container);
                string objectTypeName = container.GetType().Name;

                TaskHost taskHost = container as TaskHost;
                if (taskHost != null)
                {
                    MainPipe pipeline = taskHost.InnerObject as MainPipe;
                    if (pipeline != null)
                    {
                        ScanPipeline(path, container.ID, pipeline);
                        objectTypeName = typeof(MainPipe).Name;
                        ScanProperties(path, typeof(MainPipe), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                    }
                    else
                    {
                        objectTypeName = ((TaskHost)container).InnerObject.GetType().Name;
                        ScanProperties(path, typeof(TaskHost), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                    }
                }
                else
                {
                    ForEachLoop loop = container as ForEachLoop;
                    if (loop != null)
                    {
                        ScanProperties(path, typeof(ForEachLoop), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                        ScanProperties(path + "\\ForEachEnumerator.", typeof(ForEachEnumerator), objectTypeName, container.ID, loop.ForEachEnumerator.ID, container.Name, loop.ForEachEnumerator, containerKey);
                    }
                    else
                    {
                        ScanProperties(path, container.GetType(), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                    }
                }
                ScanVariables(path, objectTypeName, container.ID, container.Variables);
            }
        }

        private static bool GetIsFriendlyExpression(IDTSCustomPropertyCollection100 properties)
        {
            bool expression = false;
            bool friendly = false;

            foreach (IDTSCustomProperty100 property in properties)
            {
                string propertyName = property.Name;
                if (propertyName == "FriendlyExpression" && property.ExpressionType == DTSCustomPropertyExpressionType.CPET_NOTIFY)
                {
                    friendly = true;
                }
                else if (propertyName == "Expression" && property.ExpressionType == DTSCustomPropertyExpressionType.CPET_NONE)
                {
                    expression = true;
                }
            }

            if (expression && friendly)
            {
                return true;
            }

            return false;
        }

        private void ScanCustomPropertiesCollection(IDTSCustomPropertyCollection100 properties, string containerId, string objectId, string objectName, string path, string objectType)
        {
            // First check if we have a "FriendlyExpression". We use the value from FriendlyExpression, because it is CPET_NOTIFY
            // The related Expression will always be CPET_NONE
            bool friendlyExpressionValid = GetIsFriendlyExpression(properties);

            foreach (IDTSCustomProperty100 property in properties)
            {
                string value = property.Value as string;
                if (string.IsNullOrEmpty(value))
                    continue;

                string match;
                string propertyName = property.Name;
                if (property.ExpressionType == DTSCustomPropertyExpressionType.CPET_NOTIFY)
                {
                    // Check the expression string for our matching variable name
                    // We ignore the Task level properties derived from these expressions, because here we have much more context. 
                    // Could have expression properties (CPET_NOTIFY) entirely, call it Darren's OCD in action.
                    if (ExpressionMatch(value, out match))
                    {
                        // For the "FriendlyExpression" property, rename to be Expression
                        if (friendlyExpressionValid && property.Name == "FriendlyExpression")
                        {
                            propertyName = "Expression";
                        }

                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.ContainerID = containerId;
                        info.ObjectID = objectId;
                        info.ObjectName = objectName;
                        info.ObjectPath = path;
                        info.Type = typeof(MainPipe);
                        info.ObjectType = objectType;
                        info.PropertyName = propertyName;
                        info.Value = value;
                        info.IsExpression = true;
                        info.Icon = BIDSHelper.Resources.Versioned.DataFlow;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                    }
                }
                else
                {
                    if (property.Name == "Expression" && friendlyExpressionValid)
                    {
                        continue;
                    }

                    // HACK - Check the property value, its it a matching variable name?
                    // TODO: Removed this and add component specific logic


                    if (PropertyMatch(value, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.ContainerID = containerId;
                        info.ObjectID = objectId;
                        info.ObjectName = objectName;
                        info.ObjectPath = path;
                        info.Type = typeof(MainPipe);
                        info.ObjectType = objectType;
                        info.PropertyName = property.Name;
                        info.Value = value;
                        info.IsExpression = false;
                        info.Icon = BIDSHelper.Resources.Versioned.DataFlow;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                    }

                }
            }
        }

        private void ScanPipeline(string objectPath, string containerId, MainPipe pipeline)
        {
            //foreach (IDTSComponentMetaData100 componentMetadata in pipeline.ComponentMetaDataCollection)
            Parallel.ForEach(pipeline.ComponentMetaDataCollection.OfType<IDTSComponentMetaData100>(), componentMetadata =>
            {
                string objectId = componentMetadata.ID.ToString();
                string objectName = componentMetadata.Name;
                string componentPath = objectPath + "\\" + componentMetadata.Name;
                string componentKey = PackageHelper.GetComponentKey(componentMetadata);
                string objectType = PackageHelper.ComponentInfos[componentKey].Name;

                ScanCustomPropertiesCollection(componentMetadata.CustomPropertyCollection, containerId, objectId, objectName, componentPath, objectType);

                #region Inputs, Outputs, Columns
                // Scan inputs and input columns
                foreach (IDTSInput100 input in componentMetadata.InputCollection)
                {
                    string localPath = componentPath + ".Input[" + input.Name + "]";
                    ScanCustomPropertiesCollection(input.CustomPropertyCollection, containerId, objectId, objectName, localPath, objectType);

                    foreach (IDTSInputColumn100 column in input.InputColumnCollection)
                    {
                        string columnPath = localPath + ".Columns[" + column.Name + "]";
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, containerId, objectId, objectName, columnPath, objectType);
                    }
                }

                // Scan outputs and output columns
                foreach (IDTSOutput100 output in componentMetadata.OutputCollection)
                {
                    string localPath = componentPath + ".Output[" + output.Name + "]";
                    ScanCustomPropertiesCollection(output.CustomPropertyCollection, containerId, objectId, objectName, localPath, objectType);

                    foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
                    {
                        string columnPath = localPath + ".Columns[" + column.Name + "]";
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, containerId, objectId, objectName, columnPath, objectType);
                    }
                }
                #endregion

                #region Derived Column Transformation
                if (componentKey == "{18E9A11B-7393-47C5-9D47-687BE04A6B09}")
                {
                    // Component specific logic - TBC
                }
                #endregion
            });
        }

        private void ScanPrecedenceConstraints(string objectPath, string containerID, PrecedenceConstraints constraints)
        {
            if (this.CancellationPending)
            {
                return;
            }

            foreach (PrecedenceConstraint constraint in constraints)
            {
                // Check properties

                // Check expressions
                if (constraint.EvalOp == DTSPrecedenceEvalOp.Constraint)
                {
                    // Continue, when no expression used
                    continue;
                }

                if (string.IsNullOrEmpty(constraint.Expression))
                {
                    // Continue, when expression is empty
                    continue;
                }

                string match;
                if (ExpressionMatch(constraint.Expression, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.ContainerID = containerID;
                    info.ObjectID = constraint.ID;
                    info.ObjectName = ((DtsContainer)constraint.PrecedenceExecutable).Name;
                    info.ObjectPath = objectPath + ".PrecedenceConstraints[" + constraint.Name + "]";
                    info.Type = typeof(PrecedenceConstraint);
                    info.ObjectType = constraint.GetType().Name;
                    info.PropertyName = constraint.Name;
                    info.Value = constraint.Expression;
                    info.IsExpression = true;
                    info.Icon = BIDSHelper.Resources.Common.Path;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
        }

        private void ScanVariables(string objectPath, string objectName, string containerID, Variables variables)
        {
            if (this.CancellationPending)
            {
                return;
            }

            foreach (Variable variable in variables)
            {
                try
                {
                    if (!variable.EvaluateAsExpression)
                    {
                        continue;
                    }

                    // Check path to ensure variable is parented by current scope 
                    // only, not by child containers that inherit the variable
                    if (!variable.GetPackagePath().StartsWith(objectPath + ".Variables["))
                    {
                        continue;
                    }

                    string match;
                    if (ExpressionMatch(variable.Expression, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.ContainerID = containerID;
                        info.ObjectID = variable.ID;
                        info.ObjectName = objectName;
                        info.Type = typeof(Variable);
                        info.ObjectPath = objectPath + ".Variables[" + variable.QualifiedName + "]";
                        info.ObjectType = variable.GetType().Name;
                        info.PropertyName = variable.QualifiedName;
                        info.Value = variable.Expression;
                        info.IsExpression = variable.EvaluateAsExpression;
                        info.Icon = BIDSHelper.Resources.Versioned.Variable;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                    }
                }
                catch { }
            }
        }

        private void ScanProperties(string objectPath, Type objectType, string objectTypeName, string containerID, string objectID, string objectName, IDTSPropertiesProvider provider, string containerKey)
        {
            if (this.CancellationPending)
            {
                return;
            }

            bool isPipeline = (objectType == typeof(MainPipe));

            foreach (DtsProperty property in provider.Properties)
            {
                // Skip any expressuon properties on the Data Flow task, we deal with then in ScanPipeline explicitly
                if (isPipeline && property.Name.StartsWith("["))
                {
                    continue;
                }

                // Check property XXXXXXXXX
                String propertyName;
                DTSPropertyKind propertyKind;
                String packagePath;
                TypeCode propertyType;

                propertyType = property.Type;
                propertyName = property.Name;
                if (property.PropertyKind != DTSPropertyKind.Other)
                {
                    propertyKind = property.PropertyKind;
                }
                packagePath = objectPath;
                //Console.WriteLine("Property Type: {0}, Property Name: {1}, Property Kind: {2}, Package Path: {3} ",
                //propertyType, propertyName, propertyKind, packagePath);

                string match;
                if (property.Type == TypeCode.String && property.Get)
                {
                    string value = property.GetValue(provider) as string;
                    if (!string.IsNullOrEmpty(value) && PropertyMatch(value, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.ContainerID = containerID;
                        info.ObjectID = objectID;
                        info.Type = objectType;
                        info.ObjectName = objectName;

                        if (property.Name.StartsWith("["))
                        {
                            info.ObjectPath = objectPath + ".Properties" + property.Name + "";
                        }
                        else
                        {
                            info.ObjectPath = objectPath + ".Properties[" + property.Name + "]";
                        }

                        info.ObjectType = objectTypeName;
                        info.PropertyName = property.Name;
                        info.Value = value;
                        info.IsExpression = false;
                        info.Icon = PackageHelper.ControlFlowInfos[containerKey].Icon;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                    }
                }

                // Check expression
                string expression = provider.GetExpression(property.Name);
                if (expression == null)
                {
                    continue;
                }

                // Que? DG?
                System.Diagnostics.Debug.Assert(PackageHelper.ControlFlowInfos.ContainsKey(containerKey));

                if (ExpressionMatch(expression, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.ContainerID = containerID;
                    info.ObjectID = objectID;
                    info.Type = objectType;
                    info.ObjectName = objectName;

                    if (property.Name.StartsWith("["))
                    {
                        info.ObjectPath = objectPath + ".Properties" + property.Name + "";
                    }
                    else
                    {
                        info.ObjectPath = objectPath + ".Properties[" + property.Name + "]";
                    }

                    info.ObjectType = objectTypeName;
                    info.PropertyName = property.Name;
                    info.Value = expression;
                    info.IsExpression = true;
                    info.Icon = PackageHelper.ControlFlowInfos[containerKey].Icon;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
        }

        private bool ExpressionMatch(string expression, out string match)
        {
            //return this.expressionMatches.Any(expression.Contains);
            foreach (string test in this.expressionMatches)
            {
                if (expression.Contains(test))
                {
                    match = test;
                    return true;
                }
            }

            match = null;
            return false;
        }

        private bool PropertyMatch(string value, out string match)
        {            
            foreach (string test in this.properytMatches)
            {
                if (test == value)
                {
                    match = test;
                    return true;
                }
            }

            match = null;
            return false;
        }

        protected virtual void OnRaiseVariableFound(VariableFoundEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<VariableFoundEventArgs> handler = VariableFound;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }

    public class VariableFoundEventArgs : EventArgs
    {
        public VariableFoundEventArgs()
        {
            //this.type = type;
            //this.containerID = containerID;
            //this.path = objectPath;
            //this.expression = expression;
            //this.property = property;
            //this.objectID = objectID;
            //this.objectType = objectType;
        }

        public Type Type;
        public string ObjectType;
        public string ObjectName;
        public string ContainerID;
        public string ObjectID;
        public string ObjectPath;
        public string PropertyName;
        public string Value;
        public bool IsExpression;
        public Icon Icon;
        public string Match;
    }
}
