using System.Windows;
using System.Windows.Controls;

namespace CompileScore.Timeline
{
    /// <summary>
    /// Interaction logic for TimelineNodeTooltip.xaml
    /// </summary>
    public partial class TimelineNodeTooltip : UserControl
    {
        private TimelineNode node = null;
        public TimelineNode ReferenceNode
        { 
            set { node = value; OnNode(); }  
            get { return node; } 
        }

        public TimelineNodeTooltip()
        {
            InitializeComponent();
        }

        private void OnNode()
        {
            if (node != null)
            {
                headerText.Text = Common.UIConverters.ToSentenceCase(node.Category.ToString());
                durationText.Text = Common.UIConverters.GetTimeStr(node.Duration);

                if (node.Value is CompileValue)
                {
                    descriptionText.Visibility = Visibility.Visible;
                    detailsBorder.Visibility = Visibility.Visible;
                    detailsPanel.Visibility = Visibility.Visible;

                    CompileValue val = (node.Value as CompileValue);
                    descriptionText.Text = val.Name;
                    detailsText.Text = "Max: "+Common.UIConverters.GetTimeStr(val.Max)
                                     +" Min: "+ Common.UIConverters.GetTimeStr(val.Min)
                                     +" Avg: "+ Common.UIConverters.GetTimeStr(val.Average) 
                                     +" Count: "+ val.Count;
                }
                else if (node.Value is UnitValue)
                {
                    descriptionText.Visibility = Visibility.Visible;
                    detailsBorder.Visibility = Visibility.Collapsed;
                    detailsPanel.Visibility = Visibility.Collapsed;

                    descriptionText.Text = (node.Value as UnitValue).Name;
                }
                else if (node is CompileFolders.FolderTimelineNode)
                {
                    var folderNode = node as CompileFolders.FolderTimelineNode;
                    var value = folderNode.Value as CompileFolder;
                    ulong avg = folderNode.Duration;
                    if (folderNode.UnitCount > 0)
                    {
                        avg /= folderNode.UnitCount;
                    }
                    descriptionText.Visibility = Visibility.Visible;
                    descriptionText.Text = $"{value.Name}\n"
                        + $"{value.Path}\n"
                        + $"count: {folderNode.UnitCount}\n"
                        + $"avg: {Common.UIConverters.GetTimeStr(avg)}";
                }
                else
                {
                    descriptionText.Visibility = Visibility.Collapsed;
                    detailsBorder.Visibility = Visibility.Collapsed;
                    detailsPanel.Visibility = Visibility.Collapsed;
                }              
            }
        }
            
    }
}
