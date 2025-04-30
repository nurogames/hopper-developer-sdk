using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VRWeb.Managers
{

	public class CommandlineParser : HopperManagerMonoBehaviour < CommandlineParser >
    {
        [SerializeField]
        private List < string > m_AllowedParameters;

        [SerializeField]
        private string[] m_SimulatedArguments;

        public Dictionary < string, string > Parameters => GetArgs();

        private const string HOPPER_LINK = "hopper:";

        void Awake()
        {
            RegisterManager();
        }

        /// <summary>
        /// retrieve a parameter from the command line
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="paramSwitch"></param>
        /// <returns>"" if the parameter exists and has no arguments or null if the parameter is not set in the command line</returns>
        public string GetArg( Dictionary < string, string > parameters, string paramSwitch )
        {
            if ( parameters.ContainsKey( paramSwitch ) )
            {
                return parameters[paramSwitch];
            }

            return null;
        }

        /// <summary>
        /// get all command line parameters, filtered by AllowedParameters.
        /// if an arg starts with a '-' sign, it is considered a command and used as key value in the dictionary
        /// if the command is followed by a parameter which does not start with a '-', it will be used as value
        /// in the dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary < string, string > GetArgs()
        {
            Dictionary < string, string > parameters = new Dictionary < string, string >();
#if UNITY_EDITOR
            string[] args = m_SimulatedArguments;
#else
            string[] args = Environment.GetCommandLineArgs();
#endif
            //Debug.Log("ARGS LENGTH: " + args.Length);
            if (args.Length == 2)
            {
                string path = args[1].Split('?')[0];

                string fileExtension = Path.GetExtension(path).ToUpper().Remove(0, 1);
                Debug.Log("File Extension: " + fileExtension);
                if (fileExtension == "VRML")
                {
                    string vrmlArg = args[1];

                    if ( vrmlArg.ToLower().StartsWith( HOPPER_LINK ) )
                        vrmlArg = vrmlArg.Substring( HOPPER_LINK.Length );

                    Debug.Log("VRML" + vrmlArg);
                    parameters.Add( "-vrml", Uri.UnescapeDataString(vrmlArg));
                    return parameters;
                }
            }
            
            for ( int i = 0; i < args.Length; i++ )
            {
                string arg = args[i].ToLower();

                if ( !arg.StartsWith( '-' ) )
                    continue;

#if UNITY_EDITOR
                string[] parameterArray = arg.Split( ' ' );
                int len = parameterArray.Length;
                string selector = parameterArray[0];
                string argument = "";

                if ( len > 1 )
                    argument = parameterArray[1].Trim( '\"' );
#else
                string selector = arg;
                string argument = "";
                if (i+1 < args.Length && !args[i+1].StartsWith( '-' ))
                    argument = args[++i].Trim('\"');
#endif

                if ( m_AllowedParameters.Contains( selector ) )
                {
                    parameters.Add( selector, Uri.UnescapeDataString(argument) );
                }
            }

            return parameters;
        }

#if UNITY_EDITOR
        public void PurgeSimulatedArgs()
        {
            m_SimulatedArguments = Array.Empty < string >();
        }
#endif
    }

}