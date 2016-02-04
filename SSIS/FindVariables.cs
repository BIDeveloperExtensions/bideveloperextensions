using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ExecutePackageTask;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BIDSHelper.SSIS
{
    internal class FindVariables
    {
        private string[] expressionMatches = null;
        private string[] properytMatches = null;

        // Define string constants we use for specicific object types
        // Type of MainPipe
        private const string ObjectTypeDataFlowTask = "DataFlowTask";


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
            FindReferences(package, new Variable[] { variable });
        }

        public void FindReferences(Package package, Variable[] variables)
        {
            // Reset cancel pending flag
            this.CancellationPending = false;

            List<string> expressions = new List<string>();
            List<string> properties = new List<string>();

            foreach (Variable variable in variables)
            {
                // Variable formats in an expression - @Variable, @[Variable], @[Namespace::Variable]
                expressions.Add(string.Format("@{0}", variable.Name));
                expressions.Add(string.Format("@[{0}]", variable.Name));
                expressions.Add(string.Format("@[{0}]", variable.QualifiedName));
                
                // Clean qualified name for property match
                properties.Add(variable.QualifiedName);
            }

            // Convert interim lists to arrays
            this.expressionMatches = expressions.ToArray();
            this.properytMatches = properties.ToArray();

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

            foreach (ConnectionManager connection in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                // TODO; Fix - Cheat and hard code creation name as icon routines cannot get the correct connection icon

                VariableFoundEventArgs foundArgument = new VariableFoundEventArgs();
                foundArgument.ContainerID = container.ID;
                foundArgument.ObjectID = connection.ID;
                foundArgument.ObjectType = connection.GetType().Name;
                foundArgument.Type = typeof(ConnectionManager);
                foundArgument.Icon = PackageHelper.ControlFlowInfos[PackageHelper.ConnectionCreationName].Icon;
                foundArgument.ObjectPath = path + ".Connections[" + connection.Name + "].";
                foundArgument.ObjectName = connection.Name;

                //private void ScanProperties(string objectPath, Type objectType, string objectTypeName, string containerID, string objectID, string objectName, IDTSPropertiesProvider provider, string containerKey)
                ScanProperties((IDTSPropertiesProvider)connection, foundArgument);
            }
        }

        private void CheckProperties(IDTSPropertiesProvider propProvider, string path)
        {
            if (this.CancellationPending) return;

            if (propProvider is DtsContainer)
            {
                DtsContainer container = (DtsContainer)propProvider;

                VariableFoundEventArgs foundArgument = new VariableFoundEventArgs();
                foundArgument.ContainerID = container.ID;
                foundArgument.ObjectID = container.ID;                
                
                foundArgument.ObjectPath = path;

                string containerKey = PackageHelper.GetContainerKey(container);
                foundArgument.Icon = PackageHelper.ControlFlowInfos[containerKey].Icon;

                TaskHost taskHost = container as TaskHost;
                if (taskHost != null)
                {
                    CheckTask(taskHost, foundArgument);
                }
                else
                {
                    foundArgument.ObjectType = container.GetType().Name;

                    switch (foundArgument.ObjectType)
                    {
                        case "ForLoop" :
                            CheckForLoop(propProvider, foundArgument);
                            break;
                        case "ForEachLoop":
                            CheckForEachLoop(container as ForEachLoop, foundArgument);
                            break;
                        default:
                            foundArgument.Type = container.GetType();
                            ScanProperties(propProvider, foundArgument);
                            break;
                    }
                }

                ScanVariables(container.Variables, foundArgument);
            }
        }

        private void CheckTask(TaskHost taskHost, VariableFoundEventArgs foundArgument)
        {
            MainPipe pipeline = taskHost.InnerObject as MainPipe;
            if (pipeline != null)
            {
                foundArgument.ObjectType = ObjectTypeDataFlowTask;
                foundArgument.Type = typeof(MainPipe);
                //foundArgument.Icon = BIDSHelper.Resources.Versioned.DataFlow;

                ScanPipeline(pipeline, foundArgument);
                ScanProperties(taskHost, foundArgument);
            }
            else
            {
                foundArgument.Type = taskHost.InnerObject.GetType();
                foundArgument.ObjectType = foundArgument.Type.Name;

                // Task specific checks.
                switch (foundArgument.ObjectType)
                {
                    case "ExecuteSQLTask":
                        CheckExecuteSQLTask(taskHost, foundArgument);
                        break;
                }

                // TODO: Is this specific to 2014? Maybe use task creation name as standard way of differentiating tasks.
                if (taskHost.CreationName == "SSIS.ExecutePackageTask.4")
                {
                    CheckExecutePackageTask(taskHost, foundArgument);
                }

                ScanProperties(taskHost, foundArgument);
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

        private void ScanPipeline(MainPipe pipeline, VariableFoundEventArgs foundArgument)
        {
            // Careful if trying to use //Parallel.ForEach(pipeline.ComponentMetaDataCollection.OfType<IDTSComponentMetaData100>(), componentMetadata =>
            // Causes issues with twp threads using the same common foundArgument. Need to clone it first.
            foreach (IDTSComponentMetaData100 componentMetadata in pipeline.ComponentMetaDataCollection)
            {
                foundArgument.ObjectID = componentMetadata.ID.ToString();
                foundArgument.ObjectName = componentMetadata.Name;
                string componentPath = foundArgument.ObjectPath + "\\" + componentMetadata.Name;
                string componentKey = PackageHelper.GetComponentKey(componentMetadata);
                foundArgument.ObjectType = PackageHelper.ComponentInfos[componentKey].Name;

                ScanCustomPropertiesCollection(componentMetadata.CustomPropertyCollection, foundArgument, componentPath);// containerId, objectId, objectName, componentPath, objectType);

                #region Inputs, Outputs, Columns
                // Scan inputs and input columns
                foreach (IDTSInput100 input in componentMetadata.InputCollection)
                {
                    string localPath = componentPath + ".Input[" + input.Name + "]";
                    ScanCustomPropertiesCollection(input.CustomPropertyCollection, foundArgument, localPath);

                    foreach (IDTSInputColumn100 column in input.InputColumnCollection)
                    {
                        string columnPath = localPath + ".Columns[" + column.Name + "]";
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, foundArgument, localPath);
                    }
                }

                // Scan outputs and output columns
                foreach (IDTSOutput100 output in componentMetadata.OutputCollection)
                {
                    string localPath = componentPath + ".Output[" + output.Name + "]";
                    ScanCustomPropertiesCollection(output.CustomPropertyCollection, foundArgument, localPath);

                    foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
                    {
                        string columnPath = localPath + ".Columns[" + column.Name + "]";
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, foundArgument, localPath);
                    }
                }
                #endregion

                #region Derived Column Transformation
                if (componentKey == "{18E9A11B-7393-47C5-9D47-687BE04A6B09}")
                {
                    // Component specific logic - TBC
                }
                #endregion
            }
        }

        private void ScanCustomPropertiesCollection(IDTSCustomPropertyCollection100 properties, VariableFoundEventArgs foundArgument, string pathOverride)
        {
            // string containerId, string objectId, string objectName, string path, string objectType
            // First check if we have a "FriendlyExpression". We use the value from FriendlyExpression, because it is CPET_NOTIFY
            // The related Expression will always be CPET_NONE, and less readable.
            bool friendlyExpressionValid = GetIsFriendlyExpression(properties);

            foreach (IDTSCustomProperty100 property in properties)
            {
                string propertyName = property.Name;
                string value = property.Value as string;
                if (string.IsNullOrEmpty(value))
                    continue;

                string match;
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

                        VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                        info.ObjectPath = pathOverride;
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

                    PropertyMatch(foundArgument, pathOverride, propertyName, value);
                }
            }
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

        private void ScanVariables(Variables variables, VariableFoundEventArgs foundArgument)
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
                    if (!variable.GetPackagePath().StartsWith(foundArgument.ObjectPath + ".Variables["))
                    {
                        continue;
                    }

                    string match;
                    if (ExpressionMatch(variable.Expression, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                        info.ObjectID = variable.ID;
                        info.ObjectPath = foundArgument.ObjectPath + ".Variables[" + variable.QualifiedName + "]";
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

        private void ScanProperties(IDTSPropertiesProvider provider, VariableFoundEventArgs foundArgument)
        {
            if (this.CancellationPending)
            {
                return;
            }

            bool isPipeline = (foundArgument.ObjectType == ObjectTypeDataFlowTask);

            foreach (DtsProperty property in provider.Properties)
            {
                // Skip any expressuon properties on the Data Flow task, we deal with then in ScanPipeline explicitly
                if (isPipeline && property.Name.StartsWith("["))
                {
                    continue;
                }
                
                TypeCode propertyType = property.Type;
                string propertyName = property.Name;

                #region Check property value
                string match;
                if (property.Type == TypeCode.String && property.Get)
                {
                    string value = property.GetValue(provider) as string;
                    if (string.IsNullOrEmpty(value))
                        continue;

                    string pathOverride;
                    if (property.Name.StartsWith("["))
                    {
                        pathOverride = foundArgument.ObjectPath + ".Properties" + property.Name + "";
                    }
                    else
                    {
                        pathOverride = foundArgument.ObjectPath + ".Properties[" + property.Name + "]";
                    }

                    PropertyMatch(foundArgument, pathOverride, propertyName, value);
                }
                #endregion

                #region Check property expression
                // Check expression

                // TODO can we use IDTSPropertiesProviderEx.HasExpressions Property
                //enumerator.HasExpressions


                string expression = provider.GetExpression(property.Name);
                if (expression == null)
                {
                    continue;
                }

                if (ExpressionMatch(expression, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    if (property.Name.StartsWith("["))
                    {
                        info.ObjectPath = foundArgument.ObjectPath + ".Properties" + property.Name + "";
                    }
                    else
                    {
                        info.ObjectPath = foundArgument.ObjectPath + ".Properties[" + property.Name + "]";
                    }

                    info.PropertyName = property.Name;
                    info.Value = expression;
                    info.IsExpression = true;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
                #endregion

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

        private void PropertyMatch(VariableFoundEventArgs foundArgument, string pathOverride, string propertyName, string value)
        {
            string match;
            if (propertyName == "ReadOnlyVariables" || propertyName == "ReadWriteVariables")
            {
                // Comma delimited list of variable names, split and then search
                foreach (string item in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (PropertyMatch(item, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                        info.ObjectPath = pathOverride;
                        info.PropertyName = propertyName;
                        info.Value = value;
                        info.IsExpression = false;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                    }
                }
            }
            else
            {
                if (PropertyMatch(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    info.ObjectPath = pathOverride;
                    info.PropertyName = propertyName;
                    info.Value = value;
                    info.IsExpression = false;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
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

        private void CheckExecuteSQLTask(TaskHost taskHost, VariableFoundEventArgs foundArgument)
        {
            ExecuteSQLTask task = taskHost.InnerObject as ExecuteSQLTask;

            foreach (IDTSParameterBinding binding in task.ParameterBindings)
            {
                string match;
                string value = binding.DtsVariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatch(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    info.ObjectPath = foundArgument.ObjectPath + ".ParameterBindings[" + binding.ParameterName + "]";
                    info.PropertyName = binding.ParameterName.ToString();
                    info.Value = value;
                    info.IsExpression = false;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }

            foreach (IDTSResultBinding binding in task.ResultSetBindings)
            {
                string match;
                string value = binding.DtsVariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatch(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    info.ObjectPath = foundArgument.ObjectPath + ".ResultSetBindings[" + binding.ResultName + "]";
                    info.PropertyName = binding.ResultName.ToString();
                    info.Value = value;
                    info.IsExpression = false;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
        }

        private void CheckExecutePackageTask(TaskHost taskHost, VariableFoundEventArgs foundArgument)
        {
            ExecutePackageTask task = taskHost.InnerObject as ExecutePackageTask;

            // ParameterAssignments doesn't support foreach enumeration, so use for loop instead.
            for (int i = 0; i < task.ParameterAssignments.Count; i++)
            {
                IDTSParameterAssignment assignment = task.ParameterAssignments[i];

                string match;                
                string value = assignment.BindedVariableOrParameterName;
                if (!string.IsNullOrEmpty(value) && PropertyMatch(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    info.ObjectPath = foundArgument.ObjectPath + ".ParameterAssignments[" + assignment.ParameterName + "]";
                    info.PropertyName = assignment.ParameterName.ToString();
                    info.Value = value;
                    info.IsExpression = false;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
        }

        private void CheckForEachLoop(ForEachLoop forEachLoop, VariableFoundEventArgs foundArgument)
        {
            // Check properties of loop itself
            foundArgument.Type = typeof(ForEachLoop);
            ScanProperties(forEachLoop, foundArgument);

            // Check properties of enumerator
            ForEachEnumeratorHost enumerator = forEachLoop.ForEachEnumerator;
            VariableFoundEventArgs foundEnumerator = new VariableFoundEventArgs(foundArgument);
            foundEnumerator.ObjectPath = foundArgument.ObjectPath + "\\" + foundEnumerator.Type.Name + ".";
            foundEnumerator.Type = enumerator.GetType();
            ScanProperties(enumerator, foundEnumerator);

            foreach (ForEachVariableMapping mapping in forEachLoop.VariableMappings)
            {
                string match;
                string value = mapping.VariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatch(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                    info.ObjectPath = foundArgument.ObjectPath + ".VariableMappings[" + mapping.ValueIndex.ToString() + "]";
                    info.PropertyName = mapping.ValueIndex.ToString();
                    info.Value = value;
                    info.IsExpression = false;
                    info.Match = match;
                    OnRaiseVariableFound(info);
                }
            }
        }

        private void CheckForLoop(IDTSPropertiesProvider forLoop, VariableFoundEventArgs foundArgument)
        {
            // Check regular properties of the loop for variables and regular property expressions 
            foundArgument.Type = typeof(ForEachLoop);
            ScanProperties(forLoop, foundArgument);

            // Check explicit expression properties as expressions, missed if we are looking for literal variables.
            DtsProperty property;

            property = forLoop.Properties["AssignExpression"];
            PropertyExpressionMatch(property.Name, property.GetValue(forLoop).ToString(), foundArgument);

            property = forLoop.Properties["EvalExpression"];
            PropertyExpressionMatch(property.Name, property.GetValue(forLoop).ToString(), foundArgument);

            property = forLoop.Properties["InitExpression"];
            PropertyExpressionMatch(property.Name, property.GetValue(forLoop).ToString(), foundArgument);

            

        }

        private void PropertyExpressionMatch(string propertyName, string expression, VariableFoundEventArgs foundArgument)
        {
            string match;
            if (ExpressionMatch(expression, out match))
            {
                VariableFoundEventArgs info = new VariableFoundEventArgs(foundArgument);
                info.ObjectPath = foundArgument.ObjectPath + ".Properties[" + propertyName + "]";
                info.PropertyName = propertyName;
                info.Value = expression;
                info.IsExpression = true;
                info.Match = match;
                OnRaiseVariableFound(info);
            }
        }
    }


    public class VariableFoundEventArgs : EventArgs
    {
        public VariableFoundEventArgs()
        {
        }

        public VariableFoundEventArgs(VariableFoundEventArgs variableFoundEventArgs)
        {
            this.Icon = variableFoundEventArgs.Icon;
            this.Type = variableFoundEventArgs.Type;
            this.IsExpression = variableFoundEventArgs.IsExpression;
            this.ContainerID = variableFoundEventArgs.ContainerID;
            this.Match = variableFoundEventArgs.Match;
            this.ObjectID = variableFoundEventArgs.ObjectID;
            this.ObjectName = variableFoundEventArgs.ObjectName;
            this.ObjectPath = variableFoundEventArgs.ObjectPath;
            this.ObjectType = variableFoundEventArgs.ObjectType;
            this.PropertyName = variableFoundEventArgs.PropertyName;
            this.Value = variableFoundEventArgs.Value;
        }

        public Icon Icon;
        public Type Type;
        public bool IsExpression;
        public string ContainerID;
        public string Match;
        public string ObjectID;
        public string ObjectName;
        public string ObjectPath;
        public string ObjectType;
        public string PropertyName;
        public string Value;
    }
}
