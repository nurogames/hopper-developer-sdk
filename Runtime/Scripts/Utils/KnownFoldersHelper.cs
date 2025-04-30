using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VRWeb.Utils;
using UnityEngine;
using System.IO;

namespace VRWeb.Utils
{

    static public class KnownFoldersHelper
    {
        public enum KnownFolder
        {
            Documents,
            Downloads,
            Music,
            Pictures,
            SavedGames,
            Videos
        }

        private static readonly Dictionary < KnownFolder, Guid > _knownFolderGuids = new()
        {
            [KnownFolder.Documents] = new("FDD39AD0-238F-46AF-ADB4-6C85480369C7"),
            [KnownFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
            [KnownFolder.Music] = new("4BD8D571-6D19-48D3-BE97-422220080E43"),
            [KnownFolder.Pictures] = new("33E28130-4E1E-4676-835A-98395C3BC3BB"),
            [KnownFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
            [KnownFolder.Videos] = new("18989B1D-99B5-455B-841C-AB7C74E4DDFC")
        };

        [DllImport( "shell32.dll" )]
        static extern int SHGetKnownFolderPath(
            [MarshalAs( UnmanagedType.LPStruct )] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath );

        [Flags]
        public enum KnownFolderFlag : uint
        {
            None = 0x0,
            CREATE = 0x8000,
            DONT_VERFIY = 0x4000,
            DONT_UNEXPAND = 0x2000,
            NO_ALIAS = 0x1000,
            INIT = 0x800,
            DEFAULT_PATH = 0x400,
            NOT_PARENT_RELATIVE = 0x200,
            SIMPLE_IDLIST = 0x100,
            ALIAS_ONLY = 0x80000000
        }


        static KnownFolderFlag[] flags = new KnownFolderFlag[]
        {
            KnownFolderFlag.None,
            KnownFolderFlag.ALIAS_ONLY | KnownFolderFlag.DONT_VERFIY,
            KnownFolderFlag.DEFAULT_PATH | KnownFolderFlag.NOT_PARENT_RELATIVE,
        };

        public static string GetPath( KnownFolder folderType )
        {
            Guid CommonDocumentsGuid = _knownFolderGuids[folderType];
            IntPtr pPath;
            
            SHGetKnownFolderPath( 
                CommonDocumentsGuid, 
                (uint)KnownFolderFlag.DEFAULT_PATH,
                IntPtr.Zero, 
                out pPath ); // public documents

            string path = Marshal.PtrToStringUni( pPath );
            Marshal.FreeCoTaskMem( pPath );

            if ( path == null )
            {
                path = Application.temporaryCachePath;
            }

            return path;
        }
    }
}