﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Peek.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Peek.Common.Models;
    using Peek.UI.Helpers;

    public partial class FolderItemsQuery : ObservableObject
    {
        private const int UninitializedItemIndex = -1;

        public void Clear()
        {
            CurrentFile = null;

            if (InitializeFilesTask != null && InitializeFilesTask.Status == TaskStatus.Running)
            {
                Debug.WriteLine("Detected existing initializeFilesTask running. Cancelling it..");
                CancellationTokenSource.Cancel();
            }

            InitializeFilesTask = null;

            lock (_mutateQueryDataLock)
            {
                Files = new List<File>();
                _currentItemIndex = UninitializedItemIndex;
            }
        }

        public void UpdateCurrentItemIndex(int desiredIndex)
        {
            if (Files.Count <= 1 || _currentItemIndex == UninitializedItemIndex ||
                (InitializeFilesTask != null && InitializeFilesTask.Status == TaskStatus.Running))
            {
                return;
            }

            // Current index wraps around when reaching min/max folder item indices
            desiredIndex %= Files.Count;
            _currentItemIndex = desiredIndex < 0 ? Files.Count + desiredIndex : desiredIndex;

            if (_currentItemIndex < 0 || _currentItemIndex >= Files.Count)
            {
                Debug.Assert(false, "Out of bounds folder item index detected.");
                _currentItemIndex = 0;
            }

            CurrentFile = Files[_currentItemIndex];
        }

        public void Start()
        {
            var folderView = FileExplorerHelper.GetCurrentFolderView();
            if (folderView == null)
            {
                return;
            }

            Shell32.FolderItems selectedItems = folderView.SelectedItems();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            // Prioritize setting CurrentFile, which notifies UI
            var firstSelectedItem = selectedItems.Item(0);
            CurrentFile = new File(firstSelectedItem.Path);

            var items = selectedItems.Count > 1 ? selectedItems : folderView.Folder?.Items();
            if (items == null)
            {
                return;
            }

            try
            {
                if (InitializeFilesTask != null && InitializeFilesTask.Status == TaskStatus.Running)
                {
                    Debug.WriteLine("Detected unexpected existing initializeFilesTask running. Cancelling it..");
                    CancellationTokenSource.Cancel();
                }

                CancellationTokenSource = new CancellationTokenSource();
                InitializeFilesTask = new Task(() => InitializeFiles(items, firstSelectedItem, CancellationTokenSource.Token));

                // Execute file initialization/querying on background thread
                InitializeFilesTask.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception trying to run initializeFilesTask:\n" + e.ToString());
            }
        }

        // Finds index of firstSelectedItem either amongst folder items, initializing our internal File list
        //  since storing Shell32.FolderItems as a field isn't reliable.
        // Can take a few seconds for folders with 1000s of items; ensure it runs on a background thread.
        //
        // TODO optimization:
        //  Handle case where selected items count > 1 separately. Although it'll still be slow for 1000s of items selected,
        //  we can leverage faster APIs like Windows.Storage when 1 item is selected, and navigation is scoped to
        //  the entire folder. We can then avoid iterating through all items here, and maintain a dynamic window of
        //  loaded items around the current item index.
        private void InitializeFiles(
            Shell32.FolderItems items,
            Shell32.FolderItem firstSelectedItem,
            CancellationToken cancellationToken)
        {
            var tempFiles = new List<File>(items.Count);
            var tempCurIndex = UninitializedItemIndex;

            for (int i = 0; i < items.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items.Item(i);
                if (item == null)
                {
                    continue;
                }

                if (item.Name == firstSelectedItem.Name)
                {
                    tempCurIndex = i;
                }

                tempFiles.Add(new File(item.Path));
            }

            if (tempCurIndex == UninitializedItemIndex)
            {
                Debug.WriteLine("File query initialization: selectedItem index not found. Navigation remains disabled.");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            lock (_mutateQueryDataLock)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Files = tempFiles;
                _currentItemIndex = tempCurIndex;
            }
        }

        private readonly object _mutateQueryDataLock = new ();

        [ObservableProperty]
        private File? _currentFile;

        private List<File> Files { get; set; } = new ();

        private int _currentItemIndex = UninitializedItemIndex;

        public int CurrentItemIndex => _currentItemIndex;

        private CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        private Task? InitializeFilesTask { get; set; } = null;
    }
}
