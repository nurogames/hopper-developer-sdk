using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using VRWeb.User;
using Debug = UnityEngine.Debug;

namespace VRWeb.Managers
{
	public class RunExecutableManager : HopperManagerMonoBehaviour< RunExecutableManager >
    {
        public enum WindowMode : int
        {
            SW_HIDE = 0,       // Hides the window and activates another window.
            SW_SHOWNORMAL = 1, // Activates and displays a window.If the window is

            // minimized, maximized, or arranged, the system restores
            // it to its original size and position.An application should specify
            // this flag when displaying the window for the first
            SW_SHOWMINIMIZED = 2,  // Activates the window and displays it as a minimized window.
            SW_SHOWMAXIMIZED = 3,  // Activates the window and displays it as a maximized window.
            SW_SHOWNOACTIVATE = 4, // Displays a window in its most recent size and position.

            // This value is similar to SW_SHOWNORMAL, except that the window is
            // not activated.
            SW_SHOW = 5,     // Activates the window and displays it in its current size and position.
            SW_MINIMIZE = 6, // Minimizes the specified window and activates the next top - level

            // window in the Z order.
            SW_SHOWMINNOACTIVE = 7, // Displays the window as a minimized window.This value

            // is similar to SW_SHOWMINIMIZED, except the window is not activated.
            SW_SHOWNA = 8, // Displays the window in its current size and position.This value is

            // similar to SW_SHOW, except that the window is not activated.
            SW_RESTORE = 9, // Activates and displays the window.If the window is minimized, maximized,

            // or arranged, the system restores it to its original size and position.
            // An application should specify this flag when restoring a minimized window.
            SW_SHOWDEFAULT = 10, // Sets the show state based on the SW_ value specified in the STARTUPINFO

            // structure passed to the CreateProcess function by the program that
            // started the application.
            SW_FORCEMINIMIZE = 11
        }

        public bool AllowSteamApps = true;

#if PLATFORM_STANDALONE
        //private void Start()
        //{
        //    Run( "steam://rungameid/451520", true );
        //}

        private IEnumerator KillSteamVR()
        {
            Process[] currentProcesses = Process.GetProcesses();

            Process foundProcess = null;
            foreach ( Process currentProcess in currentProcesses )
            {
                if ( currentProcess.ProcessName.ToLower() == "vrmonitor" )
                {
                    foundProcess = currentProcess;
                    break;
                }
            }

            if (foundProcess == null)
                yield break;

            foundProcess.Kill();

            yield return foundProcess;
        }

        private void Awake()
        {
            RegisterManager();
        }

        public void Run( string executablePath, bool restartAfterClose )
        {
            StartCoroutine( RunCoroutine( executablePath, restartAfterClose ) );
        }

        public IEnumerator RunCoroutine( string executablePath, bool restartAfterClose )
        {
            if ( Application.platform != RuntimePlatform.WindowsPlayer &&
                 Application.platform != RuntimePlatform.WindowsEditor )
            {
                yield break;
            }

            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = Uri.UnescapeDataString( executablePath );
            startInfo.Arguments = "";
            startInfo.WorkingDirectory = Path.GetDirectoryName( startInfo.FileName );

            bool isSteam = IsSteamApp( executablePath );

            if ( isSteam && !AllowSteamApps )
            {
                UnityEngine.Debug.LogError( $"fatal exception: trying to call {executablePath}, but Steam apps are not allowed" );
                yield break;
            }

            if ( isSteam )
            {
                yield return KillSteamVR();
                startInfo.UseShellExecute = true;
            }
            else if ( !executablePath.EndsWith( "exe" ) )
                startInfo.UseShellExecute = true;

            List < string > initialProcesses = null;

            if (isSteam)
            {
                while ( initialProcesses == null )
                {
                    try
                    {
                        initialProcesses = Process.GetProcesses().Select( ( o ) => o.MainModule.FileName ).ToList();
                    }
                    catch
                    {
                        initialProcesses = null;
                    }
                }
            }

            IntPtr activeWindow = GetActiveWindow();
            Process exeProcess = null;
            try
            {
                SetWindowMode( activeWindow, WindowMode.SW_HIDE );
                exeProcess = Process.Start( startInfo );
            }
            catch ( Exception e )
            {
                Debug.LogError( $"fatal exception while trying to execute \"{executablePath}\": {e.Message}" );
                SetWindowMode( activeWindow, WindowMode.SW_SHOW );

                yield break;
            }

            if ( restartAfterClose )
            {
                if ( isSteam )
                {
                    yield return new WaitForSeconds( 5 );
                    Process p = FindSteamAppProcess( initialProcesses );

                    if ( p != null )
                        exeProcess = p;
                }

                try
                {
                    executablePath = exeProcess.MainModule.FileName;
                    Debug.Log( $"waiting for {executablePath} to exit..." );
                }
                catch
                {
                }

                yield return new WaitUntil( () => exeProcess.HasExited );

                SetWindowMode( activeWindow, WindowMode.SW_SHOW );

                Debug.Log( $"{executablePath} has exited" );
            }
        }

        private Process FindSteamAppProcess(List <string> ignoreList )
        {
            Process[] currentProcesses = Process.GetProcesses();

            foreach ( Process currentProcess in currentProcesses )
            {
                try
                {
                    string processPath = currentProcess.MainModule.FileName.ToLower();

                    if ( processPath.Contains( "steamapps" ) &&
                         !processPath.Contains( "\\steamvr\\" ) &&
                         !ignoreList.Contains( currentProcess.MainModule.FileName ) )
                    {
                        return currentProcess;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

#endif // PLATFORM_STANDALONE

        public static bool IsSteamApp( string executablePath )
        {
            return executablePath.ToLower().StartsWith( @"steam://rungameid" );
        }

        [DllImport( "user32.dll" )]
        private static extern bool ShowWindow( IntPtr hwnd, int nCmdShow );

        [DllImport( "user32.dll" )]
        private static extern IntPtr GetActiveWindow();

        private void SetWindowMode( IntPtr activeWindow, WindowMode windowMode )
        {
            ShowWindow( activeWindow, (int)windowMode );
        }
    }
}
