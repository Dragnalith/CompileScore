using Microsoft.VisualStudio.Shell;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace CompileScore
{
    /// <summary>
    /// Interaction logic for FoldersWindowControl.
    /// </summary>
    public partial class FoldersWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FoldersWindowControl"/> class.
        /// </summary>
        public FoldersWindowControl()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.InitializeComponent();

            timeline.SetMode(Timeline.Timeline.Mode.Includers);
        }

        public void SetFolders(CompileFolder folder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            timeline.SetFolder(folder);
        }
    }
}