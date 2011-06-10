﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BIDSHelper.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BIDSHelper.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static System.Drawing.Bitmap arrowDown {
            get {
                object obj = ResourceManager.GetObject("arrowDown", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap arrowFlat {
            get {
                object obj = ResourceManager.GetObject("arrowFlat", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap arrowUp {
            get {
                object obj = ResourceManager.GetObject("arrowUp", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Icon BIDSHelper {
            get {
                object obj = ResourceManager.GetObject("BIDSHelper", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Description Resource.
        /// </summary>
        internal static string BIDSHelperDescription {
            get {
                return ResourceManager.GetString("BIDSHelperDescription", resourceCulture);
            }
        }
        
        internal static System.Drawing.Icon Biml {
            get {
                object obj = ResourceManager.GetObject("Biml", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon BimlFile {
            get {
                object obj = ResourceManager.GetObject("BimlFile", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.varigence.com/documentation/bidshelper/frameworkversion.
        /// </summary>
        internal static string BimlFrameworkVersionAlert {
            get {
                return ResourceManager.GetString("BimlFrameworkVersionAlert", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.varigence.com/documentation/bidshelper.
        /// </summary>
        internal static string BimlHelpUrl {
            get {
                return ResourceManager.GetString("BimlHelpUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.varigence.com/documentation/bidshelper/overwrite.
        /// </summary>
        internal static string BimlOverwriteConfirmationHelpUrl {
            get {
                return ResourceManager.GetString("BimlOverwriteConfirmationHelpUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.varigence.com/documentation/bidshelper/validation.
        /// </summary>
        internal static string BimlValidationHelpUrl {
            get {
                return ResourceManager.GetString("BimlValidationHelpUrl", resourceCulture);
            }
        }
        
        internal static System.Drawing.Icon CheckBiml {
            get {
                object obj = ResourceManager.GetObject("CheckBiml", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Connection {
            get {
                object obj = ResourceManager.GetObject("Connection", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Copy {
            get {
                object obj = ResourceManager.GetObject("Copy", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon DataFlow {
            get {
                object obj = ResourceManager.GetObject("DataFlow", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;xsl:stylesheet version=&quot;1.0&quot; 
        ///	xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot;
        ///	xmlns:as=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;
        ///	xmlns:dwd=&quot;http://schemas.microsoft.com/DataWarehouse/Designer/1.0&quot;&gt;
        ///	&lt;xsl:param name=&quot;TargetDatabase&quot;&gt;&lt;/xsl:param&gt;
        ///	&lt;xsl:param name=&quot;TargetCubeID&quot;&gt;&lt;/xsl:param&gt;
        ///
        ///	&lt;xsl:output indent=&quot;yes&quot; omit-xml-declaration=&quot;yes&quot; /&gt;
        ///
        ///	&lt;xsl:template match=&quot;/&quot;&gt;
        ///		&lt;Batch xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///			&lt;xsl:apply-templates/&gt;
        ///	 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DeployAggDesigns {
            get {
                return ResourceManager.GetString("DeployAggDesigns", resourceCulture);
            }
        }
        
        internal static System.Drawing.Icon DeployAggDesignsIcon {
            get {
                object obj = ResourceManager.GetObject("DeployAggDesignsIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;xsl:stylesheet version=&quot;1.0&quot; 
        ///	xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot;
        ///	xmlns:as=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///	&lt;xsl:param name=&quot;TargetDatabase&quot;&gt;&lt;/xsl:param&gt;
        ///
        ///	&lt;xsl:output indent=&quot;yes&quot; omit-xml-declaration=&quot;yes&quot; /&gt;
        ///	&lt;xsl:template match=&quot;/&quot;&gt;
        ///		&lt;Alter AllowCreate=&quot;true&quot; ObjectExpansion=&quot;ExpandFull&quot; xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///			&lt;Object&gt;
        ///				&lt;DatabaseID&gt;
        ///					&lt;xsl:value-of select=&quot;$TargetDatabase&quot;&gt;&lt;/xsl:value-of&gt;
        ///				&lt;/ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DeployMdxScript {
            get {
                return ResourceManager.GetString("DeployMdxScript", resourceCulture);
            }
        }
        
        internal static System.Drawing.Icon DeployMdxScriptIcon {
            get {
                object obj = ResourceManager.GetObject("DeployMdxScriptIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon DestComponent {
            get {
                object obj = ResourceManager.GetObject("DestComponent", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Edit {
            get {
                object obj = ResourceManager.GetObject("Edit", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon EditAggregations {
            get {
                object obj = ResourceManager.GetObject("EditAggregations", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon EditVariable {
            get {
                object obj = ResourceManager.GetObject("EditVariable", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon End {
            get {
                object obj = ResourceManager.GetObject("End", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon EstimatedCounts {
            get {
                object obj = ResourceManager.GetObject("EstimatedCounts", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Event {
            get {
                object obj = ResourceManager.GetObject("Event", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon ExpressionListIcon {
            get {
                object obj = ResourceManager.GetObject("ExpressionListIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon ForEachLoop {
            get {
                object obj = ResourceManager.GetObject("ForEachLoop", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon ForLoop {
            get {
                object obj = ResourceManager.GetObject("ForLoop", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Function {
            get {
                object obj = ResourceManager.GetObject("Function", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon M2MIcon {
            get {
                object obj = ResourceManager.GetObject("M2MIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon MinusSign {
            get {
                object obj = ResourceManager.GetObject("MinusSign", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon NoIcon {
            get {
                object obj = ResourceManager.GetObject("NoIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Package {
            get {
                object obj = ResourceManager.GetObject("Package", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Path {
            get {
                object obj = ResourceManager.GetObject("Path", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Performance {
            get {
                object obj = ResourceManager.GetObject("Performance", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon PlusSign {
            get {
                object obj = ResourceManager.GetObject("PlusSign", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon PrinterFriendlyDimensionUsageIcon {
            get {
                object obj = ResourceManager.GetObject("PrinterFriendlyDimensionUsageIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ProcessError {
            get {
                object obj = ResourceManager.GetObject("ProcessError", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ProcessProgress {
            get {
                object obj = ResourceManager.GetObject("ProcessProgress", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ProgressComplete {
            get {
                object obj = ResourceManager.GetObject("ProgressComplete", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Icon Question {
            get {
                object obj = ResourceManager.GetObject("Question", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon RefreshExpressions {
            get {
                object obj = ResourceManager.GetObject("RefreshExpressions", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Run {
            get {
                object obj = ResourceManager.GetObject("Run", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Sequence {
            get {
                object obj = ResourceManager.GetObject("Sequence", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon SmallBlueDiamond {
            get {
                object obj = ResourceManager.GetObject("SmallBlueDiamond", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xsl:stylesheet version=&quot;1.0&quot; xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:DTS=&quot;www.microsoft.com/SqlServer/Dts&quot;&gt;
        ///	&lt;xsl:output cdata-section-elements=&quot;ProjectItem arrayElement&quot;/&gt;
        ///
        ///	&lt;xsl:template match=&quot;node()&quot;&gt;
        ///		&lt;xsl:copy&gt;
        ///			&lt;!-- leave default sort order --&gt;
        ///			&lt;xsl:apply-templates select=&quot;@*|node()[name()!=&apos;DTS:LogProvider&apos; and name()!=&apos;DTS:Executable&apos; and name()!=&apos;DTS:ConnectionManager&apos; and name()!=&apos;DTS:PrecedenceConstraint&apos; and name()!=&apos;component&apos; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SmartDiffDtsx {
            get {
                return ResourceManager.GetString("SmartDiffDtsx", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xsl:stylesheet version=&quot;1.0&quot; xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:dwd=&quot;http://schemas.microsoft.com/DataWarehouse/Designer/1.0&quot; xmlns:SSAS=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot; xmlns:msprop=&quot;urn:schemas-microsoft-com:xml-msprop&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///
        ///	&lt;xsl:output cdata-section-elements=&quot;SSAS:Text&quot;/&gt;
        ///
        ///	&lt;xsl:template match=&quot;node()&quot;&gt;
        ///		&lt;xsl:copy&gt;
        ///			
        ///			&lt;!-- sort attributes by name --&gt;
        ///			&lt;xsl:apply [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SmartDiffSSAS {
            get {
                return ResourceManager.GetString("SmartDiffSSAS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xsl:stylesheet version=&quot;1.0&quot; xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:SSRS2005=&quot;http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition&quot; xmlns:SSRS2008=&quot;http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition&quot;&gt;
        ///
        ///  &lt;xsl:output cdata-section-elements=&quot;SSRS2005:Code SSRS2008:Code SSRS2005:CommandText SSRS2008:CommandText&quot;/&gt;
        ///  
        ///  &lt;xsl:template match=&quot;node()&quot;&gt;
        ///		&lt;xsl:copy&gt;
        ///
        ///      &lt;!-- sort attributes by name --&gt;
        ///   [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SmartDiffSSRS {
            get {
                return ResourceManager.GetString("SmartDiffSSRS", resourceCulture);
            }
        }
        
        internal static System.Drawing.Icon SourceComponent {
            get {
                object obj = ResourceManager.GetObject("SourceComponent", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Stop {
            get {
                object obj = ResourceManager.GetObject("Stop", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon TaskSmall {
            get {
                object obj = ResourceManager.GetObject("TaskSmall", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon TreeViewTab {
            get {
                object obj = ResourceManager.GetObject("TreeViewTab", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Variable {
            get {
                object obj = ResourceManager.GetObject("Variable", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
    }
}
