namespace BIDSHelper.SSIS
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.SqlServer.Dts.Runtime;

    public class ComponentInfo
    {
        [DllImport("Shell32", CharSet = CharSet.Auto)]
        private static unsafe extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static unsafe extern int DestroyIcon(IntPtr hIcon);

        private DTSPipelineComponentType componentType;
        private string id;
        private string name;
        private string creationName;
        private Icon icon;

        public ComponentInfo(PipelineComponentInfo componentInfo)
        {
            this.componentType = componentInfo.ComponentType;
            this.id = componentInfo.ID;
            this.name = componentInfo.Name;
            this.creationName = componentInfo.CreationName;
        }

        public ComponentInfo(TaskInfo componentInfo)
        {
            this.id = componentInfo.ID;
            this.name = componentInfo.Name;
            this.creationName = componentInfo.CreationName;

            Assembly assembly = GetComponentAssembly(componentInfo);

            if (assembly != null)
            {
                Stream iconStream = assembly.GetManifestResourceStream(componentInfo.IconResource);
                if (iconStream != null)
                {
                    this.icon = new Icon(iconStream, new Size(16, 16));
                }
            }
            else
            {
                int index = 0;
                Int32.TryParse(componentInfo.IconResource, out index);
                this.icon = ExtractIcon(componentInfo.IconFile, index, false);
            }

            // Ensure we always have an icon
            if (this.icon == null)
            {
                this.icon = BIDSHelper.Resources.Common.NoIcon;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <remarks>Used to add special objects to collections.</remarks>
        public ComponentInfo(Icon icon)
        {
            if (icon.Height > 16)
            {
                this.icon = new Icon(icon, new Size(16, 16));
            }
            else
            {
                this.icon = icon;
            }
        }

        private Assembly GetComponentAssembly(IDTSName name)
        {
            Assembly assembly = null;
            try
            {
                string assemblyName = name.ID.Remove(0, 1 + name.ID.IndexOf(',')).Trim();
                
                // Check for GUID as string, and exit
                if (assemblyName.StartsWith("{"))
                {
                    return null;
                }

                assembly = Assembly.Load(assemblyName);
            }
            catch { }

            return assembly;
        }

        private static Icon ExtractIcon(string file, int index,  bool large)
        {
            // http://www.pinvoke.net/default.aspx/shell32.extracticonex

            unsafe
            {
                int readIconCount = 0;
                IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
                IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };

                try
                {
                    if (large)
                        readIconCount = ExtractIconEx(file, index, hIconEx, hDummy, 1);
                    else
                        readIconCount = ExtractIconEx(file, index, hDummy, hIconEx, 1);

                    if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero)
                    {
                        // GET FIRST EXTRACTED ICON
                        Icon extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();

                        return extractedIcon;
                    }
                    else // NO ICONS READ
                        return null;
                }
                catch (Exception ex)
                {
                    /* EXTRACT ICON ERROR */

                    // BUBBLE UP
                    throw new ApplicationException("Could not extract icon", ex);
                }
                finally
                {
                    // RELEASE RESOURCES
                    foreach (IntPtr ptr in hIconEx)
                    {
                        if (ptr != IntPtr.Zero)
                        {
                            DestroyIcon(ptr);
                        }
                    }

                    foreach (IntPtr ptr in hDummy)
                    {
                        if (ptr != IntPtr.Zero)
                        {
                            DestroyIcon(ptr);
                        }
                    }
                }
            }
        }
        
        public DTSPipelineComponentType ComponentType
        {
            get { return this.componentType; }
        }

        public string ID
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string CreationName
        {
            get { return this.creationName; }
        }

        public Icon Icon
        {
            get { return this.icon; }
        }
    }
}
