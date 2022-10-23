using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace CompileScore
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("e9f71445-d02d-48e9-8877-13b414da434d")]
    public class FoldersWindow : Common.WindowProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FoldersWindow"/> class.
        /// </summary>
        public FoldersWindow()
        {
            this.Caption = "Compile Score Folders";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new FoldersWindowControl();
        }

        public void SetFolders(CompileFolder folder = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            (this.Content as FoldersWindowControl).SetFolders(folder);
        }
    }
}
