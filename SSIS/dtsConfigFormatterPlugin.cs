namespace BIDSHelper.SSIS
{
    using System;
    using System.Windows.Forms;
    using EnvDTE;
    using EnvDTE80;

    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class dtsConfigFormatterPlugin : BIDSHelperWindowActivatedPluginBase
    {
        //private WindowEvents windowEvents;
        private DocumentEvents docEvents;

        private Timer t = new Timer();
        private int mRetryCnt = 0;
        private const int MAX_RETRY = 5;

        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        
        //TODO: may be needed if we decide to capture the ActiveViewChanged event... see TODO below on this topic
        //private System.Collections.Generic.List<string> windowHandlesFixedPartitionsView = new System.Collections.Generic.List<string>();

        public dtsConfigFormatterPlugin(BIDSHelperPackage package)
            : base(package)
        {
        
        }

        public override void OnEnable()
        {
            try
            {
                base.OnEnable();
                this.ApplicationObject.Events.SolutionItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(SolutionItemsEvents_ItemAdded);
                //appObject.Events.MiscFilesEvents.ItemAdded += null;

                //windowEvents = appObject.Events.get_WindowEvents(null);
                //windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
                //windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
                docEvents = this.ApplicationObject.Events.get_DocumentEvents(null);
                docEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(docEvents_DocumentOpened);
                t.Tick += new EventHandler(t_Tick);
                t.Interval = 500;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace);
            }
        }

        public override bool ShouldHookWindowCreated
        {
            get
            {
                return true;
            }
        }

        public override void OnDisable()
        {
            try
            {
                base.OnDisable();
                // todo
                this.ApplicationObject.Events.SolutionItemsEvents.ItemAdded -= SolutionItemsEvents_ItemAdded;
                //appObject.Events.MiscFilesEvents.ItemAdded += null;


                //docEvents = appObject.Events.get_DocumentEvents(null);
                if (docEvents != null)
                { docEvents.DocumentOpened -= docEvents_DocumentOpened; }
                t.Tick -= t_Tick;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace);
            }
        }

        void SolutionItemsEvents_ItemAdded(ProjectItem ProjectItem)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void docEvents_DocumentOpened(Document doc)
        {
            //doc.Activate();
        }

        void windowEvents_WindowCreated(Window activeWindow)
        {
            try
            {
                if (activeWindow.ProjectItem.Document.Path.ToLower().EndsWith(".dtsconfig"))
                {
                    activeWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                //IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                //if (designer == null) return;
                ProjectItem pi = null;
                try
                {
                    pi = GotFocus.ProjectItem;
                }
                catch
                {
                    return;
                }
                
                if ((pi == null) || (!(pi.Name.ToLower().EndsWith(".dtsconfig")))) return;

                if ((t !=null) && (t.Enabled)) return;

                t.Enabled = true;
                t.Tag = pi;

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace);
            }
        }

        void t_Tick(object sender, EventArgs e)
        {
            try
            {
                t.Enabled = false;
                try
                {
                    FormatActiveDocument((ProjectItem)t.Tag);
                    mRetryCnt = 0;
                }
                catch
                {
                    mRetryCnt++;
                    if (mRetryCnt <= MAX_RETRY)
                    {
                        System.Media.SystemSounds.Beep.Play();
                        t.Enabled = true;
                    }
                    else
                    {
                        mRetryCnt = 0; // reset the retry count                
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace);
            }
        }


        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            try
            {
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace);
            }
        }

        public override string ShortName
        {
            get { return "dtsConfigFormatterPlugin"; }
        }
        

        public override string FeatureName
        {
            get { return "dtsConfig Formatter"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }
        

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        /// <remarks>Wiki page title is non-formatted string, differs from text.</remarks>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("dtsConfigFormatter"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Automatically applies easy to read formatting to your dtsConfig files as they are opened."; }
        }

        public override void Exec()
        {
        }

        void FormatActiveDocument(ProjectItem pi)
        {
            pi.DTE.ExecuteCommand("Edit.FormatDocument", "");
        }
    }
}
