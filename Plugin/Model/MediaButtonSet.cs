using static StreamDeck.OscBridge.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StreamDeck.OscBridge.Model
{
    internal class MediaButtonSet
    {
        private readonly FileSystemWatcher _watcher;
        public MediaButtonSet()
        {
            _watcher = new FileSystemWatcher()
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _watcher.Changed += (s, e) => RefreshItems(e.FullPath);
            _watcher.Deleted += (s, e) => RefreshItems(e.FullPath);
            _watcher.Created += (s, e) => RefreshItems(e.FullPath);
            _watcher.Renamed += (s, e) => RefreshItems(e.FullPath);
            Items = Enumerable.Range(0, MaxMediaItemsPerSet).Select(i => new MediaItem()).ToList().AsReadOnly();
            RefreshItems();
        }

        internal string DirectoryPath
        {
            get
            {
                return _watcher.Path;
            }
            set
            {
                string expanded = Environment.ExpandEnvironmentVariables(value);
                if (_watcher.Path != expanded)
                {
                    _watcher.EnableRaisingEvents = false;
                    if (Directory.Exists(expanded))
                    {
                        _watcher.Path = expanded;
                        RefreshItems();
                    }
                }
            }
        }

        private bool PathIsInFilter(string path)
        {
            return _filters == null || _filters.Contains(Path.GetExtension(path));
        }

        private HashSet<string> _filters;

        private string _filter;
        internal string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    if (string.IsNullOrWhiteSpace(_filter) || _filter == "*.*")
                    {
                        _filters = null;
                    }
                    else
                    {
                        try
                        {
                            _filters = new HashSet<string>(_filter?.Split('|').Select(f => "." + f.Split(".").Last()));
                        }
                        catch
                        {
                            _filters = null;
                        }
                    }
                    RefreshItems();
                }
            }
        }

        private bool _forceLabel;
        internal bool ForceLabel
        {
            get
            {
                return _forceLabel;
            }
            set
            {
                if (ForceLabel != value)
                {
                    _forceLabel = value;
                    RefreshItems();
                }
            }
        }

        internal IReadOnlyList<MediaItem> Items { get; }

        private void RefreshItems(string changedPath)
        {
            if (PathIsInFilter(changedPath))
            {
                RefreshItems();
            }
        }

        private void RefreshItems()
        {
            int index = 0;
            if (!string.IsNullOrWhiteSpace(_watcher.Path) && Directory.Exists(_watcher.Path))
            {
                IEnumerable<string> paths = Directory.GetFiles(_watcher.Path).Where(p => PathIsInFilter(p)).Take(MaxMediaItemsPerSet);
                
                foreach (string path in paths)
                {
                    Items[index].Update(path, _forceLabel);
                    index++;
                }
                _watcher.EnableRaisingEvents = true;
            }
            else
            {
                _watcher.EnableRaisingEvents = false;
            }
            for (int i = index; i < MaxMediaItemsPerSet; i++)
            {
                Items[i].Update(null, _forceLabel);
            }
        }
    }
}
