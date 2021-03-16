﻿using Microsoft.Win32;
using SharpShell.Attributes;
using SharpShell.SharpDropHandler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extension {

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".mp3")]
    public class Mp3CoverDroper : SharpDropHandler {

        private readonly string[] supportedImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

        protected override void DragEnter(DragEventArgs dragEventArgs) {
            var supported = DragItems.All(di => supportedImageExtensions.Contains(Path.GetExtension(di)));
            dragEventArgs.Effect = supported ? DragDropEffects.Link : DragDropEffects.None;
        }

        protected override void Drop(DragEventArgs dragEventArgs) {
            string[] args = { $"\"{SelectedItemPath}\"" };
            foreach (var imagePath in DragItems) {
                args = args.Append($"\"{imagePath}\"").ToArray();
            }

            // get implementation executable file
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AoiHosizora\Mp3CoverDroper");
            if (key == null) {
                MessageBox.Show("You have not set Mp3CoverDroper's registry config yet.", "Mp3CoverDroper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var executablePath = key.GetValue("Implementation") as string;
            executablePath = executablePath.Trim('"');
            if (string.IsNullOrWhiteSpace(executablePath) || File.Exists(executablePath)) {
                MessageBox.Show("Mp3CoverDroper's implementation executable file is not found.", "Mp3CoverDroper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // call implementation
            var process = new Process();
            var info = new ProcessStartInfo(executablePath, string.Join(", ", args));
            process.StartInfo = info;
            process.Start();
        }
    }
}