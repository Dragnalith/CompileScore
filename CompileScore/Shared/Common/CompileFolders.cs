using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CompileScore
{
    public class CompileFolder
    {
        public bool Visited { set; get; } = false;
        public string Name { set; get; }
        public string Path { set; get; }
        public int Index { set; get; }
        public List<int> Children { set; get; }
        public List<UnitValue> Units { set; get; }
        public List<CompileValue> Includes { set; get; }
    }

    public class CompileFolders
    {
        private List<CompileFolder> Folders { set; get; } = new List<CompileFolder>();

        private Timeline.TimelineNode _root = null;

        private CompileScorePackage Package { get; set; }

        public void Initialize(CompileScorePackage package)
        {
            Package = package;
        }

        private string GetUnitPathRecursive(UnitValue unit, CompileFolder node, string fullpath)
        {
            foreach (UnitValue value in node.Units)
            {
                if (value == unit)
                {
                    return fullpath + value.Name;
                }
            }

            foreach (int childrenIndex in node.Children)
            {
                if (childrenIndex < Folders.Count)
                {
                    CompileFolder folder = Folders[childrenIndex];
                    string result = GetUnitPathRecursive(unit, folder, fullpath + folder.Name + '/');
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
        private string GetIncludePathRecursive(CompileValue value, CompileFolder node, string fullpath)
        {
            foreach (CompileValue thisValue in node.Includes)
            {
                if (thisValue == value)
                {
                    return fullpath + thisValue.Name;
                }
            }

            foreach (int childrenIndex in node.Children)
            {
                if (childrenIndex < Folders.Count)
                {
                    CompileFolder folder = Folders[childrenIndex];
                    string result = GetIncludePathRecursive(value, folder, fullpath + folder.Name + '/');
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public string GetUnitPath(UnitValue unit)
        {
            if (unit != null && Folders != null && Folders.Count > 0)
            {
                return GetUnitPathRecursive(unit, Folders[0], "");
            }
            return null;
        }

        public string GetValuePath(CompilerData.CompileCategory category, CompileValue value)
        {
            if (value != null && Folders != null && Folders.Count > 0 && category == CompilerData.CompileCategory.Include)
            {
                return GetIncludePathRecursive(value, Folders[0], "");
            }
            return null;
        }

        public string GetUnitPathSafe(UnitValue unit)
        {
            string fullPath = GetUnitPath(unit);
            return fullPath == null ? "" : fullPath;
        }

        public string GetValuePathSafe(CompilerData.CompileCategory category, CompileValue value)
        {
            string fullPath = GetValuePath(category, value);
            return fullPath == null ? "" : fullPath;
        }

        private CompileFolder GetFolderFromPathRecursive(CompileFolder node, string[] directories, int index)
        {
            if (directories.Length == (index + 1))
            {
                //found the folder
                return node;
            }

            string thisName = directories[index];
            foreach (int childrenIndex in node.Children)
            {
                if (childrenIndex < Folders.Count && Folders[childrenIndex].Name == thisName)
                {
                    return GetFolderFromPathRecursive(Folders[childrenIndex], directories, index + 1);
                }
            }

            return null;
        }

        private CompileFolder GetFolderFromPath(string[] directories)
        {
            if (Folders != null && Folders.Count > 0 && directories.Length > 1)
            {
                return GetFolderFromPathRecursive(Folders[0], directories, 0);
            }
            return null;
        }

        public UnitValue GetUnitByPath(string path)
        {
            if (path != null)
            {
                string[] directories = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                CompileFolder folder = GetFolderFromPath(directories);
                if (folder != null)
                {
                    string filename = directories[directories.Length - 1];

                    foreach (UnitValue unit in folder.Units)
                    {
                        if (unit.Name == filename)
                        {
                            return unit;
                        }
                    }
                }
            }
            return null;
        }

        public CompileValue GetValueByPath(CompilerData.CompileCategory category, string path)
        {
            if (path != null && category == CompilerData.CompileCategory.Include)
            {
                string[] directories = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                CompileFolder folder = GetFolderFromPath(directories);
                if (folder != null)
                {
                    string filename = directories[directories.Length - 1];

                    foreach (CompileValue value in folder.Includes)
                    {
                        if (value.Name == filename)
                        {
                            return value;
                        }
                    }
                }
            }
            return null;
        }

        public void RecomputeCacheData()
        {
            RecomputePath(Folders[0], "");
        }

        public void RecomputePath(CompileFolder folder, string parentPath = "")
        {
            if (folder.Name == "")
            {
                folder.Path = parentPath;

            } 
            else if (parentPath == "")
            {
                folder.Path = folder.Name;
            }
            else
            {
                folder.Path = parentPath + "/" + folder.Name;
            }

            foreach (int childrenIndex in folder.Children)
            {
                if (childrenIndex < Folders.Count)
                {
                    RecomputePath(Folders[childrenIndex], folder.Path);
                }
            }

            foreach (var value in folder.Units)
            {
                value.Path = folder.Path + "/" + value.Name;
            }


            foreach (var value in folder.Includes)
            {
                value.Path = folder.Path + "/" + value.Name;
            }
        }

        private void ReadFolder(BinaryReader reader, List<CompileFolder> list)
        {
            var folder = new CompileFolder();

            folder.Name = reader.ReadString();

            CompilerData compilerData = CompilerData.Instance;

            uint countChildren = reader.ReadUInt32();
            if (countChildren >= 0)
            {
                folder.Children = new List<int>();
                for (uint i = 0; i < countChildren; ++i)
                {
                    folder.Children.Add((int)reader.ReadUInt32());
                }
            }

            uint countUnits = reader.ReadUInt32();
            if (countUnits >= 0)
            {
                folder.Units = new List<UnitValue>();
                for (uint i = 0; i < countUnits; ++i)
                {
                    folder.Units.Add(compilerData.GetUnitByIndex(reader.ReadUInt32()));
                }
            }

            uint countIncludes = reader.ReadUInt32();
            if (countIncludes >= 0)
            {
                folder.Includes = new List<CompileValue>();
                for (uint i = 0; i < countIncludes; ++i)
                {
                    folder.Includes.Add(compilerData.GetValue(CompilerData.CompileCategory.Include, (int)reader.ReadUInt32()));
                }
            }

            list.Add(folder);
            folder.Index = list.Count - 1;
        }

        public void ReadFolders(BinaryReader reader)
        {
            //Read Folders
            uint foldersLength = reader.ReadUInt32();
            var folderList = new List<CompileFolder>((int)foldersLength);
            for (uint i = 0; i < foldersLength; ++i)
            {
                ReadFolder(reader, folderList);
            }
            Folders = new List<CompileFolder>(folderList);
        }

        public Timeline.TimelineNode LoadTimeline(CompileFolder unit)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_root == null)
            {
                _root = BuildGraphRecursive(Folders[0]);
                InitializeTree(_root);
            }
            return _root;
        }

        public class FolderTimelineNode : Timeline.TimelineNode
        {
            public FolderTimelineNode(string label, uint start, uint duration, CompilerData.CompileCategory category, object compileValue = null)
                : base(label, start, duration, category, compileValue)
            { }
            public uint UnitCount { get; set; } = 0;
        }

        public void DisplayFolders()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            FoldersWindow window = FocusFoldersWindow();
            window.SetFolders(Folders[0]);
        }

        public FoldersWindow FocusFoldersWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            FoldersWindow window = Package.FindToolWindow(typeof(FoldersWindow), 0, true) as FoldersWindow;
            if ((null == window) || (null == window.GetFrame()))
            {
                throw new NotSupportedException("Cannot create folders window");
            }

            window.ProxyShow();

            return window;
        }

        private FolderTimelineNode BuildGraphRecursive(CompileFolder folder)
        {
            //Only add each element once to avoid cycles
            Debug.Assert(!folder.Visited);
            folder.Visited = true;

            FolderTimelineNode node = new FolderTimelineNode(null, 0, 0, CompilerData.CompileCategory.Folder, folder);

            for (int i = 0; i < folder.Children.Count; ++i)
            {
                //Build all children 
                FolderTimelineNode child = BuildGraphRecursive(Folders[folder.Children[i]]);
                if (child != null)
                {
                    node.Duration += child.Duration;
                    node.UnitCount += child.UnitCount;
                    node.AddChild(child);
                }
            }

            for (int i = 0; i < folder.Units.Count; ++i)
            {
                var unit = folder.Units[i];
                var duration = unit.ValuesList[(int)CompilerData.CompileCategory.ExecuteCompiler];
                string label = unit.Name;
                var child = new FolderTimelineNode(label, 0, duration, CompilerData.CompileCategory.ExecuteCompiler, unit);
                node.Duration += duration;
                node.UnitCount += 1;
                node.AddChild(child);
            }

            ulong avg = node.Duration;
            if (node.UnitCount > 0)
            {
                avg /= node.UnitCount;
            }
            //fix up node
            node.Label = folder.Name
                + " (dur: " + Common.UIConverters.GetTimeStr(node.Duration) 
                + ", count: " + node.UnitCount 
                + ", avg: " + Common.UIConverters.GetTimeStr(avg)
                + " )";

            return node;
        }

        private void InitializeTree(Timeline.TimelineNode node)
        {
            node.Start = 0;
            node.DepthLevel = 0;
            InitializeNodeRecursive(node);
        }

        private void InitializeNodeRecursive(Timeline.TimelineNode node)
        {
            node.DepthLevel = node.Parent == null ? 0 : node.Parent.DepthLevel + 1;
            node.MaxDepthLevel = node.DepthLevel;
            node.UIColor = Common.Colors.GetCategoryBackground(node.Category);

            //reorder children by duration
            ulong offset = node.Start;
            node.Children.Sort((a, b) => a.Duration == b.Duration ? 0 : (a.Duration > b.Duration ? -1 : 1));

            foreach (Timeline.TimelineNode child in node.Children)
            {
                child.Start = offset;
                offset += child.Duration;
                InitializeNodeRecursive(child);
                node.MaxDepthLevel = Math.Max(node.MaxDepthLevel, child.MaxDepthLevel);
            }
        }
    }
}
