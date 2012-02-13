//=================================================================================================
//=================================================================================================
//
// Copyright (c) 2012 Dennis Mnuskin
//
// This file is part of TrackerSync application.
//
// This source code is distributed under the MIT license.  For full text, see
// http://www.opensource.org/licenses/mit-license.php Same text is found in LICENSE.txt file which
// is located in root directory of the project.
//
//=================================================================================================
//=================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TrackerSync
{
    class Program
    {
        static int Main( string[] args )
        {
            CommandLine     commandLine = new CommandLine();
            int             returnVal = 0;

            try
            {
                commandLine.Parse( args );

                if( commandLine.Mode == SyncAppMode.RunEngine )
                {
                    RunSyncEngine( commandLine );
                }
                else if( commandLine.Mode == SyncAppMode.CreateConfigTemplate )
                {
                    CreateConfigTemplateFile( commandLine );
                }
            }
            catch( Exception e )
            {
                Console.Error.WriteLine( "Error: {0}", e.ToString() );
                returnVal = -1;
            }

            if( commandLine.PauseOnExit )
            {
                Console.WriteLine();
                Console.WriteLine( "Press any key to exit..." );
                Console.ReadKey( true );
            }

            return returnVal;
        }

        private static void RunSyncEngine( CommandLine commandLine )
        {
            SyncSettings    settings = new SyncSettings( commandLine.ConfigFile );

            if( commandLine.NoUpdates.HasValue ) settings.NoUpdates = commandLine.NoUpdates.Value;
            if( commandLine.LogLevel.HasValue )  settings.LogLevel = commandLine.LogLevel.Value;

            var sync = new TrackerSync.Engine.SyncEngine( settings );

            sync.Execute();
        }

        private static void CreateConfigTemplateFile( CommandLine commandLine )
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            try
            {
                using( Stream resStream = assembly.GetManifestResourceStream(
                                                      assembly.GetName().Name + ".Resources.ConfigTemplate.xml" ) )
                using( Stream fileStream = File.Open( commandLine.ConfigFile, FileMode.CreateNew,
                                                      FileAccess.Write, FileShare.None            ) )
                {
                    resStream.CopyTo( fileStream );
                }
            }
            catch
            {
                DeleteIncompleteConfigFile( commandLine.ConfigFile );
                throw;
            }
        }

        private static void DeleteIncompleteConfigFile( string filePath )
        {
            try
            {
                File.Delete( filePath );
            }
            catch( Exception )
            {
            }
        }
    }
}
