﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
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
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;xsl:stylesheet version=&quot;1.0&quot; 
        ///	xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot;
        ///	xmlns:as=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///	&lt;xsl:param name=&quot;TargetDatabase&quot;&gt;&lt;/xsl:param&gt;
        ///
        ///	&lt;xsl:output indent=&quot;yes&quot; omit-xml-declaration=&quot;yes&quot; /&gt;
        ///	&lt;xsl:template match=&quot;/&quot;&gt;
        ///		&lt;Alter ObjectExpansion=&quot;ExpandFull&quot; xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///			&lt;Object&gt;
        ///				&lt;DatabaseID&gt;
        ///					&lt;xsl:value-of select=&quot;$TargetDatabase&quot;&gt;&lt;/xsl:value-of&gt;
        ///				&lt;/DatabaseID&gt;
        ///				&lt;C [rest of string was truncated]&quot;;.
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
        
        internal static System.Drawing.Icon EditAggregations {
            get {
                object obj = ResourceManager.GetObject("EditAggregations", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon EstimatedCounts {
            get {
                object obj = ResourceManager.GetObject("EstimatedCounts", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        internal static System.Drawing.Icon Stop {
            get {
                object obj = ResourceManager.GetObject("Stop", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
    }
}
