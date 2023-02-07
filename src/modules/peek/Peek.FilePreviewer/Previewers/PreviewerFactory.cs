﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Peek.Common.Models;

namespace Peek.FilePreviewer.Previewers
{
    public class PreviewerFactory
    {
        public IPreviewer Create(IFileSystemItem file)
        {
            if (PngPreviewer.IsFileTypeSupported(file.Extension))
            {
                return new PngPreviewer(file);
            }
            else if (ImagePreviewer.IsFileTypeSupported(file.Extension))
            {
                return new ImagePreviewer(file);
            }
            else if (WebBrowserPreviewer.IsFileTypeSupported(file.Extension))
            {
                return new WebBrowserPreviewer(file);
            }

            // Other previewer types check their supported file types here
            return CreateDefaultPreviewer(file);
        }

        public IPreviewer CreateDefaultPreviewer(IFileSystemItem file)
        {
            return new UnsupportedFilePreviewer(file);
        }
    }
}
