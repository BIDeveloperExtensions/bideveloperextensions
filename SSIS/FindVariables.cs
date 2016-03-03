using Microsoft.DataTransformationServices.Design;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ExecutePackageTask;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    internal class FindVariables
    {
        public const string IconKeyFolder = "Folder";
        public const string IconKeyProperties = "Properties";
        public const string IconKeyInput = "Input";
        public const string IconKeyOutput = "Output";
        public const string IconKeyColumn = "Column";
        public const string IconKeyPrecedenceConstraint = "PrecedenceConstraint";
        public const string IconKeyVariable = "Variable";
        public const string IconKeyVariableExpression = "VariableExpression";
        public const string IconKeyProperty = "Property";
        public const string IconKeyPropertyExpression = "PropertyExpression";
        
        private string[] expressionMatches = null;
        private string[] properytMatches = null;

        // Define string constants we use for specicific object types
        // Type of MainPipe
        private const string ObjectTypeDataFlowTask = "DataFlowTask";
        private const string ObjectTypeExecutePackageTask = "ExecutePackageTask";

        private TreeView treeView;

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

        public void FindReferences(Package package, Variable variable, TreeView treeView)
        {
            FindReferences(package, new Variable[] { variable }, treeView);
        }

        public void FindReferences(Package package, Variable[] variables, TreeView treeView)
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

            this.treeView = treeView;

            if (treeView != null)
            {
                SetupImageList();

                // Create package node, the top level node
                int imageIndex = GetControlFlowImageIndex(PackageHelper.PackageCreationName);
                TreeNode packageNode = new TreeNode(package.Name, imageIndex, imageIndex);
                packageNode.Tag = package;

                ProcessObject(package, packageNode);

                // Hide nodes that don't have any matches
                PruneNodes(packageNode);

                // Add node graph to tree view
                AddRootNode(packageNode);
            }
            else
            {
                ProcessObject(package, null);
            }


            // Reset cancel pending in case we have cancelled, as about to exit, so no longer pending
            this.CancellationPending = false;
        }

        private void PruneNodes(TreeNode parent)
        {
            for (int index = parent.Nodes.Count - 1; index >= 0; index--)
            {
                TreeNode node = parent.Nodes[index];

                if (node.IsExpanded || node.Checked)
                {
                    PruneNodes(node);
                }
                else
                {
                    node.Remove();
                }
            }
        }

        delegate void AddRootNodeCallback(TreeNode node);

        private void AddRootNode(TreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (this.treeView.InvokeRequired)
            {
                AddRootNodeCallback callback = new AddRootNodeCallback(AddRootNode);
                this.treeView.Invoke(callback, new object[] { node });
            }
            else
            {
                this.treeView.Nodes.Add(node);
            }
        }

        delegate void AddNodeCallback(TreeNode treeView, TreeNode node);

        private TreeNode AddNode(TreeNode parentNode, string text, int imageIndex, object tag)
        {
            return AddNode(parentNode, text, imageIndex, tag, false);
        }

        private TreeNode AddNode(TreeNode parentNode, string text, int imageIndex, object tag, bool found)
        {
            if (parentNode == null)
                return null;

            TreeNode node = new TreeNode(text, imageIndex, imageIndex);
            node.Name = text;
            node.Tag = tag;
            node.Checked = found;

            AddNodeSafe(parentNode, node);

            return node;
        }


        private void AddNodeSafe(TreeNode parentNode, TreeNode childNode)
        {
            if (parentNode == null)
                return;

            if (childNode == null)
                return;

            if (treeView.InvokeRequired)
            {
                AddNodeCallback callback = new AddNodeCallback(AddNodeSafe);
                treeView.Invoke(callback, new object[] { parentNode, childNode });
            }
            else
            {
                parentNode.Nodes.Add(childNode);

                if (childNode.Checked)
                {
                    TreeNode target = childNode;
                    while (target != null)
                    {
                        target.Expand();

                        target = target.Parent;
                    }
                }
            }
        }

        private int GetControlFlowImageIndex(string key)
        {
            if (treeView == null)
                return -1;

            int imageIndex = treeView.ImageList.Images.IndexOfKey(key);
            if (imageIndex == -1)
            {
                AddImageListItem(key, PackageHelper.ControlFlowInfos[key].Icon);
                imageIndex = treeView.ImageList.Images.Count - 1;
            }

            return imageIndex;
        }

        private int GetComponentImageIndex(string key)
        {
            if (treeView == null)
                return -1;

            int imageIndex = treeView.ImageList.Images.IndexOfKey(key);
            if (imageIndex == -1)
            {
                AddImageListItem(key, PackageHelper.ComponentInfos[key].Icon);
                imageIndex = treeView.ImageList.Images.Count - 1;
            }

            return imageIndex;
        }

        private int GetImageIndex(string iconKey)
        {
            if (treeView == null)
                return -1;

            return treeView.ImageList.Images.IndexOfKey(iconKey);
        }

        private void SetupImageList()
        {
            // Add some standard icons to our image list. Order is fixed, since we have hardcoded index values in GetImageIndex function
            AddImageListItem(IconKeyFolder, SharedIcons.FolderOpened);
            AddImageListItem(IconKeyProperties, SharedIcons.AllProperties);
            AddImageListItem(IconKeyInput, SharedIcons.FolderOpened);
            AddImageListItem(IconKeyOutput, SharedIcons.FolderOpened);
            AddImageListItem(IconKeyColumn, SharedIcons.FolderOpened);
            AddImageListItem(IconKeyPrecedenceConstraint, SharedIcons.PrecedenceConstraint);
            AddImageListItem(IconKeyVariable, SharedIcons.Variable_properties);
            AddImageListItem(IconKeyVariableExpression, SharedIcons.VariableExpressionIcon);
            AddImageListItem(IconKeyProperty, BIDSHelper.Resources.Versioned.Variable);
            AddImageListItem(IconKeyPropertyExpression, SharedIcons.VariableExpressionIcon);
        }

        delegate void AddImageListItemCallback(string creationName, Icon image);

        private void AddImageListItem(string creationName, Icon image)
        {
            if (image == null)
                return;

            if (treeView.InvokeRequired)
            {
                AddImageListItemCallback callback = new AddImageListItemCallback(AddImageListItem);
                treeView.Invoke(callback, new object[] { creationName, image });
            }
            else
            {
                this.treeView.ImageList.Images.Add(creationName, image);
            }
        }

        private void ProcessObject(object component, TreeNode parentNode)
        {
            if (this.CancellationPending)
            {
                return;
            }

            Package package = component as Package;
            if (package != null)
            {
                // Package node is created in calling function.
                CheckConnectionManagers(package, parentNode);
            }

            DtsContainer container = component as DtsContainer;
            if (container != null)
            {
                string containerKey = PackageHelper.GetContainerKey(container);
                TaskHost taskHost = null;

                if (package == null)
                {
                    int imageIndex = GetControlFlowImageIndex(containerKey);
                    parentNode = AddNode(parentNode, container.Name, imageIndex, component);
                    taskHost = container as TaskHost;
                }
                
                if (taskHost != null)
                {
                    CheckTask(taskHost, parentNode);
                }
                else if (containerKey == PackageHelper.ForLoopCreationName)
                {
                    CheckForLoop(container as IDTSPropertiesProvider, parentNode);
                }
                else if (containerKey == PackageHelper.ForEachLoopCreationName)
                {
                    CheckForEachLoop(container as ForEachLoop, parentNode);
                }
                else if (containerKey == PackageHelper.SequenceCreationName)
                {
                    ScanProperties(container as IDTSPropertiesProvider, parentNode);
                }
                else
                {
                    // Package, Event Handlers etc
                    ScanProperties(container as IDTSPropertiesProvider, parentNode);
                }

                string currentPath = string.Empty;
                IDTSPackagePath packagePath = component as IDTSPackagePath;
                if (packagePath != null)
                {
                    currentPath = packagePath.GetPackagePath();
                }

                ScanVariables(container.Variables, parentNode, currentPath);
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                TreeNode eventsNode = AddFolder("EventHandlers", parentNode);
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    ProcessObject(eventhandler, eventsNode);
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                ProcessSequence(sequence, parentNode);
                ScanPrecedenceConstraints(container.ID, sequence.PrecedenceConstraints, parentNode);
            }
        }

        private void ProcessSequence(IDTSSequence sequence, TreeNode parentNode)
        {
            if (sequence == null)
                return;

            if (this.CancellationPending)
                return;

            foreach (Executable executable in sequence.Executables)
            {
                ProcessObject(executable, parentNode);
            }
        }

        private void CheckConnectionManagers(Package package, TreeNode parentNode)
        {
            if (this.CancellationPending)
                return;

            TreeNode folder = AddFolder("Connections", parentNode);

            int imageIndex = GetControlFlowImageIndex(PackageHelper.ConnectionCreationName);

            foreach (ConnectionManager connection in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                TreeNode node = AddNode(folder, connection.Name, imageIndex, connection);
                ScanProperties((IDTSPropertiesProvider)connection, node);
            }
        }

        private TreeNode AddFolder(string folder, TreeNode parentNode)
        {
            if (parentNode == null)
                return null;

            int imageIndex = treeView.ImageList.Images.IndexOfKey(IconKeyFolder);
            return AddNode(parentNode, folder, imageIndex, null);
        }

        private void CheckTask(TaskHost taskHost, TreeNode parent)
        {
            string typeName = taskHost.InnerObject.GetType().Name;

            // Scan regular task properties and expressions
            // We may use the TreeView Properties folder in task specific checks, so do this first
            ScanProperties(taskHost, parent);

            // Task specific checks, split by native and managed
            if (typeName == "__ComObject")
            {
                // Translate creation name via PackageHelper, as that caters for both nice and GUID formats, e.g. For SQL 2012 a dataFLow task can be either "{5918251B-2970-45A4-AB5F-01C3C588FE5A}" or SSIS.Pipeline.3
                string creatioName = PackageHelper.ControlFlowInfos[taskHost.CreationName].CreationName;

                // Native code tasks, can't use type name, so use creation name.
                // Need to be wary of suffix, SSIS.ExecutePackageTask.3 for 2012, SSIS.ExecutePackageTask.4 for 2014 etc
                if (creatioName == string.Format("SSIS.{0}.{1}",  ObjectTypeExecutePackageTask, SSISHelpers.CreationNameIndex))
                {
                    CheckExecutePackageTask(taskHost, parent);
                }
                else if (creatioName == string.Format("SSIS.Pipeline.{0}", SSISHelpers.CreationNameIndex))
                {
                    MainPipe pipeline = taskHost.InnerObject as MainPipe;
                    ScanPipeline(pipeline, parent);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "Unrecognised native task - " + taskHost.CreationName);
                }
            }
            else
            {
                // For managed code tasks we can use type name. This means we don't have to have a 
                // full reference to the task assembly, but any properties we access must be simple 
                // ones accessible via IDTSPropertiesProvider, i.e. ExpressionTask. For more complex 
                // properties such as ParameterBiningds on the ExecuteSQLTask we need the reference.
                switch (typeName)
                {
                    case "ExecuteSQLTask":
                        CheckExecuteSQLTask(taskHost, parent);
	                    break;
                    case "ExpressionTask":
                        CheckExpressionTask(taskHost, parent);
	                    break;
                    case "ExecutePackageTask":
                        CheckExecutePackageTask(taskHost, parent);
	                    break;
                }
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

        private void ScanPipeline(MainPipe pipeline, TreeNode parent)
        {
            TreeNode folder = AddFolder("Components", parent);

            // Careful if trying to use //Parallel.ForEach(pipeline.ComponentMetaDataCollection.OfType<IDTSComponentMetaData100>(), componentMetadata =>
            // Causes issues with two threads using the same common foundArgument. Need to clone it first.
            foreach (IDTSComponentMetaData100 componentMetaData in pipeline.ComponentMetaDataCollection)
            {
                string componentKey = PackageHelper.GetComponentKey(componentMetaData);
                int imageIndex = GetComponentImageIndex(componentKey);
                TreeNode componentNode = AddNode(folder, componentMetaData.Name, imageIndex, componentMetaData); ;

                ScanCustomPropertiesCollection(componentMetaData.CustomPropertyCollection, componentNode);

                #region Inputs, Outputs, Columns
                // Scan inputs and input columns
                foreach (IDTSInput100 input in componentMetaData.InputCollection)
                {
                    TreeNode node = AddNode(componentNode, "Input [" + input.Name + "]", GetImageIndex(IconKeyInput), input);
                    ScanCustomPropertiesCollection(input.CustomPropertyCollection, node);

                    TreeNode columnsNode = AddFolder("Output Columns", node);
                    foreach (IDTSInputColumn100 column in input.InputColumnCollection)
                    {
                        TreeNode columnNode = AddNode(columnsNode, column.Name, GetImageIndex(IconKeyColumn), column);
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, columnNode);
                    }
                }

                // Scan outputs and output columns
                foreach (IDTSOutput100 output in componentMetaData.OutputCollection)
                {
                    TreeNode node = AddNode(componentNode, "Output [" + output.Name + "]", GetImageIndex(IconKeyOutput), output);
                    ScanCustomPropertiesCollection(output.CustomPropertyCollection, componentNode);

                    TreeNode columnsNode = AddFolder("Output Columns", node);
                    foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
                    {
                        TreeNode columnNode = AddNode(columnsNode, column.Name, GetImageIndex(IconKeyColumn), column);
                        ScanCustomPropertiesCollection(column.CustomPropertyCollection, columnNode);
                    }
                }
                #endregion

                // Derived Column Transformation
                if (componentKey == "{18E9A11B-7393-47C5-9D47-687BE04A6B09}")
                {
                    // Component specific logic - TBC
                    // Most seems to be covered by columns, as that is where the expressions are stored.
                }
                
            }
        }

        private void ScanCustomPropertiesCollection(IDTSCustomPropertyCollection100 properties, TreeNode parent)
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

                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.Match = match;
                        OnRaiseVariableFound(info);
                        AddNode(parent, propertyName, GetImageIndex(IconKeyPropertyExpression), new PropertyExpression(propertyName, value, property.Value.GetType()), true);
                    }
                }
                else
                {
                    if (property.Name == "Expression" && friendlyExpressionValid)
                    {
                        continue;
                    }


                    if (PropertyMatch(propertyName, value, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        info.Match = match;
                        OnRaiseVariableFound(info);
                        // new PropertyInfo(
                        AddNode(parent, propertyName, GetImageIndex(IconKeyProperty), property, true);
                    }
                }
            }
        }

        private void ScanPrecedenceConstraints(string containerID, PrecedenceConstraints constraints, TreeNode parent)
        {
            if (this.CancellationPending)
            {
                return;
            }

            TreeNode constraintsNode = AddFolder("PrecedenceConstraints", parent);

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
                    info.Match = match;
                    OnRaiseVariableFound(info);
                    AddNode(constraintsNode, "Expression", GetImageIndex(IconKeyPrecedenceConstraint), constraint, true);
                    //AddNode(constraintsNode, "Expression", GetImageIndex(IconKeyPrecedenceConstraint), new PropertyExpression(propertyName, expression, PackageHelper.GetTypeFromTypeCode(property.Type)), true);
                }
            }
        }

        private void ScanVariables(Variables variables, TreeNode parent, string currentPath)
        {
            if (this.CancellationPending)
            {
                return;
            }

            TreeNode variablesFolder = AddFolder("Variables", parent);
            int imageIndex = GetImageIndex(IconKeyVariableExpression);

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
                    if (!variable.GetPackagePath().StartsWith(currentPath + ".Variables["))
                    {
                        continue;
                    }

                    string match;
                    if (ExpressionMatch(variable.Expression, out match))
                    {
                        VariableFoundEventArgs info = new VariableFoundEventArgs();
                        //info.ObjectID = variable.ID;
                        //info.ObjectPath = foundArgument.ObjectPath + ".Variables[" + variable.QualifiedName + "]";
                        //info.ObjectType = variable.GetType().Name;
                        //info.PropertyName = variable.QualifiedName;
                        //info.Value = variable.Expression;
                        //info.IsExpression = variable.EvaluateAsExpression;
                        //info.Icon = BIDSHelper.Resources.Versioned.Variable;
                        info.Match = match;
                        OnRaiseVariableFound(info);
                        AddNode(variablesFolder, variable.QualifiedName, imageIndex, variable, true);
                        //AddNode(expressions, "Expression", GetImageIndex(IconKeyProperti1es), new PropertyExpression(propertyName, expression, PackageHelper.GetTypeFromTypeCode(property.Type)), true);
                    }
                }
                catch { }
            }
        }

        private void ScanProperties(IDTSPropertiesProvider provider, TreeNode parent)
        {
            if (this.CancellationPending)
            {
                return;
            }

            TreeNode properties = AddFolder("Properties", parent);
            TreeNode expressions = AddFolder("PropertyExpressions", parent);

            //bool isPipeline = (foundArgument.ObjectType == ObjectTypeDataFlowTask);

            // New 2012 + interface implemented by Package, Sequence, DtsEventHandler, ForLoop, ForEachLoop
            // There are other objects that implement IDTSPropertiesProvider, and therefore support expressions, e.g. ConnectionManager, Variable
            // However we can use it to skip objects that have no expressions set, by using HasExpressions property
            bool hasExpressions = true;
            IDTSPropertiesProviderEx providerEx = provider as IDTSPropertiesProviderEx;
            if (providerEx != null)
            {
                hasExpressions = providerEx.HasExpressions;
            }
            
            foreach (DtsProperty property in provider.Properties)
            {
                // Skip any expressuon properties on the Data Flow task, we deal with then in ScanPipeline explicitly
                if (property.Name.StartsWith("["))
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
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (PropertyMatch(propertyName, value, out match))
                        {
                            VariableFoundEventArgs foundArgument = new VariableFoundEventArgs();
                            foundArgument.Match = match;
                            OnRaiseVariableFound(foundArgument);
                            AddNode(properties, propertyName, GetImageIndex(IconKeyProperty), new PropertyInfo(property, value), true);
                        }
                    }
                }
                #endregion

                #region Check property expression
                string expression = provider.GetExpression(property.Name);
                if (expression == null)
                {
                    continue;
                }

                // Check this for a while, before we trust it, simce it is undocumented.
                System.Diagnostics.Debug.Assert(hasExpressions, "HasExpressions was false, but we have an expression.");

                if (ExpressionMatch(expression, out match))
                {
                    VariableFoundEventArgs foundArgument = new VariableFoundEventArgs();
                    foundArgument.Match = match;
                    OnRaiseVariableFound(foundArgument);
                    AddNode(expressions, propertyName, GetImageIndex(IconKeyVariableExpression), new PropertyExpression(propertyName, expression, PackageHelper.GetTypeFromTypeCode(property.Type)), true);
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

        private bool PropertyMatch(string propertyName, string value, out string match)
        {
            if (propertyName == "ReadOnlyVariables" || propertyName == "ReadWriteVariables")
            {
                // Comma delimited list of variable names, split and then search
                foreach (string item in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (PropertyMatchEval(item, out match))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (PropertyMatchEval(value, out match))
                {
                    return true;
                }
            }

            match = null;
            return false;
        }

        private bool PropertyMatchEval(string value, out string match)
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

        private void CheckExecuteSQLTask(TaskHost taskHost, TreeNode parent)
        {
            ExecuteSQLTask task = taskHost.InnerObject as ExecuteSQLTask;

            TreeNode parameterBindings = AddFolder("ParameterBindings", parent);

            foreach (IDTSParameterBinding binding in task.ParameterBindings)
            {
                string match;
                string value = binding.DtsVariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatchEval(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.Match = match;
                    OnRaiseVariableFound(info);
                    AddNode(parameterBindings, binding.ParameterName.ToString(), GetImageIndex(IconKeyProperty), binding, true);
                }
            }

            TreeNode resultSetBindings = AddFolder("ResultSetBindings", parent);

            foreach (IDTSResultBinding binding in task.ResultSetBindings)
            {
                string match;
                string value = binding.DtsVariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatchEval(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.Match = match;
                    OnRaiseVariableFound(info);
                    AddNode(resultSetBindings, binding.ResultName.ToString(), GetImageIndex(IconKeyProperty), binding, true);
                }
            }
        }

        private void CheckExpressionTask(TaskHost taskHost, TreeNode parent)
        {
            // Expression task has the Expression property which we need to treat as an expression rather than a literal value as we do for normal properties.
            // Get the Expression value and run an expression matct test
            DtsProperty property = taskHost.Properties["Expression"];
            string expression = property.GetValue(taskHost).ToString();
            PropertyAsExpressionMatch(property, expression, parent);
        }

        private void CheckExecutePackageTask(TaskHost taskHost, TreeNode parent)
        {
            ExecutePackageTask task = taskHost.InnerObject as ExecutePackageTask;

            TreeNode parameterAssignments = AddFolder("ParameterAssignments", parent);

            // ParameterAssignments doesn't support foreach enumeration, so use for loop instead.
            for (int i = 0; i < task.ParameterAssignments.Count; i++)
            {
                IDTSParameterAssignment assignment = task.ParameterAssignments[i];

                string match;                
                string value = assignment.BindedVariableOrParameterName;
                if (!string.IsNullOrEmpty(value) && PropertyMatchEval(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.Match = match;
                    OnRaiseVariableFound(info);
                    AddNode(parameterAssignments, assignment.ParameterName.ToString(), GetImageIndex(IconKeyProperty), assignment, true);
                }
            }
        }

        private void CheckForEachLoop(ForEachLoop forEachLoop, TreeNode parent)
        {
            // Check properties of loop itself
            //foundArgument.Type = typeof(ForEachLoop);
            ScanProperties(forEachLoop, parent);

            // Check properties of enumerator, when present
            ForEachEnumeratorHost enumerator = forEachLoop.ForEachEnumerator;
            if (enumerator == null)
                return;

            TreeNode enumeratorFolder = AddFolder(enumerator.GetType().Name, parent);
            ScanProperties(enumerator, enumeratorFolder);

            TreeNode variableMappings = AddFolder("VariableMappings", parent);
            foreach (ForEachVariableMapping mapping in forEachLoop.VariableMappings)
            {
                string match;
                string value = mapping.VariableName;
                if (!string.IsNullOrEmpty(value) && PropertyMatchEval(value, out match))
                {
                    VariableFoundEventArgs info = new VariableFoundEventArgs();
                    info.Match = match;
                    OnRaiseVariableFound(info);
                    AddNode(variableMappings, mapping.ValueIndex.ToString(), GetImageIndex(IconKeyProperty), mapping, true);
                }
            }
        }

        private void CheckForLoop(IDTSPropertiesProvider forLoop, TreeNode parent)
        {
            // Check regular properties of the loop for variables and regular property expressions 
            ScanProperties(forLoop, parent);

            // Check explicit expression properties as expressions, missed if we are looking for literal variables.
            DtsProperty property;

            property = forLoop.Properties["AssignExpression"];
            PropertyAsExpressionMatch(property, property.GetValue(forLoop).ToString(), parent);

            property = forLoop.Properties["EvalExpression"];
            PropertyAsExpressionMatch(property, property.GetValue(forLoop).ToString(), parent);

            property = forLoop.Properties["InitExpression"];
            PropertyAsExpressionMatch(property, property.GetValue(forLoop).ToString(), parent);
        }

        private void PropertyAsExpressionMatch(DtsProperty property, string expression, TreeNode parent)
        {
            string match;
            if (ExpressionMatch(expression, out match))
            {
                VariableFoundEventArgs info = new VariableFoundEventArgs();
                info.Match = match;
                OnRaiseVariableFound(info);
                
                if (parent != null)
                {
                    TreeNode propertiesNode = parent.Nodes["Properties"];
                    System.Diagnostics.Debug.Assert(!(parent != null && propertiesNode == null), "Properties node doesn't exist when it should already. We will lose this property match. Find the Properties node.");
                    AddNode(propertiesNode, property.Name, GetImageIndex(IconKeyVariableExpression), new PropertyInfo(property, expression), true);
                }
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
            this.Match = variableFoundEventArgs.Match;
        }

        public string Match { get; set; }
    }

    public class PropertyExpression
    {
        public PropertyExpression(string name, string expression, Type type)
        {
            this.PropertyName = name;
            this.Expression = expression;
            this.Type = type;
        }

        [ParenthesizePropertyName(), Browsable(true), Category("General"), Description("The name of the property hosting the expression.")]
        public string PropertyName { get; private set; }

        [Category("General"), Description("The expression text for the property expression.")]
        public object Expression { get; private set; }

        [Category("General")]
        public Type Type { get; private set; }
    }

    [DisplayName("Property")]
    public class PropertyInfo
    {
        // We don't need an IDTSCustomProperty100 version because that already does a good job of displaying both the property information AND the value.
        // DtsProperty doesn't include the value, hence we use this wrapper for the TreeView tag object

        public PropertyInfo(DtsProperty property, object value)
        {
            this.Name = property.Name;
            this.Value = value;
            this.Type =  PackageHelper.GetTypeFromTypeCode(property.Type);
            this.Get = property.Get;
            this.Set = property.Set;
        }

        [ParenthesizePropertyName(), Browsable(true), Category("General"), Description("The name of the property which contains the reference.")]
        public string Name { get; private set; }

        [Category("Accessors"), Description("Indicates whether the property value can be read.")]
        public bool Get { get; private set; }

        [Category("Accessors"), Description("Indicates whether the property value is changeable.")]
        public bool Set { get; private set; }

        [Category("General"), Description("THe property value, including the reference.")]
        public object Value { get; private set; }

        [Category("General"), Description("The data type of the property value.")]
        public Type Type { get; private set; }
    }
}
