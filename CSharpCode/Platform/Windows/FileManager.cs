﻿using System;
using System.Collections.Generic;

namespace EngineNS.IO
{
    partial class FileManager
    {
        partial void InitDirectory()
        {
            var mBin = System.IO.Directory.GetCurrentDirectory();//AppDomain.CurrentDomain.BaseDirectory;
            var root = GetBaseDirectory(mBin, 1);

            if (FileExists(root + "Engine.cfg"))
            {
                var xml = new System.Xml.XmlDocument();
                xml.Load(root + "Engine.cfg");
            }
            else
            {

            }

            SetRoot(ERootDir.Root, root);
            SetRoot(ERootDir.Current, mBin);
            SetRoot(ERootDir.Engine, root + "enginecontent");
            SetRoot(ERootDir.Game, root + "content");
            SetRoot(ERootDir.Editor, root + "editorcontent");
            SetRoot(ERootDir.Cache, root + "cache");
        }
    }
    public partial class UOpenFileDialog
    {
        partial void ShowDialogImpl(ref int result)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = this.Title;
            fileDialog.Filter = this.Filter;
            fileDialog.InitialDirectory = this.InitialDirectory;
            fileDialog.Multiselect = this.Multiselect;
            fileDialog.ShowReadOnly = this.ShowReadOnly;
            result = (int)fileDialog.ShowDialog();
            mFileName = fileDialog.FileName;
            mFileNames = fileDialog.FileNames;
        }
    }
}