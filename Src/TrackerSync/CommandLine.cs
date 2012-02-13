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
using System.Linq;
using System.Text;

namespace TrackerSync
{
    enum SyncAppMode
    {
        None,
        Invalid,
        RunEngine,
        CreateConfigTemplate,
    }

    class CommandLine
    {
        public CommandLine()
        {
            this.LogLevel = SyncLogLevel.None;
        }

        public CommandLine( string[] args ) : this()
        {
            Parse( args );
        }

        public bool PauseOnExit { get; private set; }

        public bool? NoUpdates { get; private set; }

        public SyncLogLevel? LogLevel { get; private set; }

        public SyncAppMode Mode { get; private set; }

        public string ConfigFile { get; private set; }


        public void Parse( string[] args )
        {
            int     argIdx;

            this.Mode = SyncAppMode.None;

            for( argIdx = 0; argIdx < args.Length && this.Mode != SyncAppMode.Invalid; argIdx++ )
            {
                switch( args[ argIdx ] )
                {
                case "--pause-exit": 
                case "-p": 
                    this.PauseOnExit = true;
                    break;

                case "--verbose":
                case "-v":
                    this.LogLevel = SyncLogLevel.Verbose;
                    break;

                case "--status":
                case "-s":
                    this.NoUpdates = true;
                    if( !this.LogLevel.HasValue || this.LogLevel < SyncLogLevel.PrintActions )
                    {
                        this.LogLevel = SyncLogLevel.PrintActions;
                    }
                    break;

                case "--config":
                case "-c":
                case "--create-config":
                    ProcessConfigFileOption( args, ref argIdx );
                    break;

                default:
                    ReportCommandLineError( "Unknown command line option: {0}", args[ argIdx ] );
                    break;

                case "--help":
                case "-?":
                case "/?":
                    DisplayUsage();
                    this.Mode = SyncAppMode.Invalid;
                    break;
                }
            }

            if( this.Mode != SyncAppMode.Invalid )
            {
                ValidateOptions();
            }
        }

        private void ReportCommandLineError( string errorMsgFormat, params object[] args )
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine( errorMsgFormat, args );
            Console.Out.WriteLine();

            DisplayUsage();

            this.Mode = SyncAppMode.Invalid;
        }

        private void ProcessConfigFileOption( string[] args, ref int argIdx )
        {
            SyncAppMode mode = args[ argIdx ] == "--create-config" ? SyncAppMode.CreateConfigTemplate :
                                                                     SyncAppMode.RunEngine;

            if( ++argIdx == args.Length )
            {
                ReportCommandLineError( "Option {0} must be followed by a file path", args[ argIdx - 1 ] );
                return;
            }

            bool isExistingFile = System.IO.File.Exists( args[ argIdx ] );

            if( ( mode == SyncAppMode.RunEngine && !isExistingFile ) ||
                ( mode == SyncAppMode.CreateConfigTemplate && isExistingFile ) )
            {
                ReportCommandLineError( 
                    isExistingFile ? "{0} '{1}' is an existing file.\n" + 
                                        "Please, specify a name of a file that doesn't exist" :
                                     "{0} '{1}' does not appear to be a valid file",
                    args[ argIdx - 1 ], args[ argIdx ]                                          );
            }
            else if( this.Mode != SyncAppMode.None )
            {
                ReportCommandLineError( 
                    "Only one --config/-c or --create-config option must be specified, but not both" );
            }
            else
            {
                this.Mode = mode;
                this.ConfigFile = args[ argIdx ];
            }
        }

        private void DisplayUsage()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            string appName = assembly.GetName().Name;
            string offsetWhitespace = new string( ' ', appName.Length );

            Console.Out.WriteLine( "usage: {0} (-c|--config <file>)|--create-config <file> [-p|--pause]\n" +
                                   "       {1} [-s|--status] [-v|--verbose] [-?|--help]", appName, offsetWhitespace );
            Console.Out.WriteLine();
            Console.Out.WriteLine( "Command options:" );
            Console.Out.WriteLine( "   -c, --config <file>          Specifies the XML configuration file which identifies issue trackers" );
            Console.Out.WriteLine( "                                to sync as well as configuration settings applicable to those trackers" );
            Console.Out.WriteLine( "       --create-config <file>   Creates a template configuration XML file which can be filled in." );
            Console.Out.WriteLine( "   -p, --pause-exit             Pauses execution on exit until a user presses a key. This option is" );
            Console.Out.WriteLine( "                                useful when running the utility from debugger so that the window is" );
            Console.Out.WriteLine( "                                not instantly closed." );
            Console.Out.WriteLine( "   -s, --status                 Queries both issue tracking databases to determine if there are any" );
            Console.Out.WriteLine( "                                updates to be made.  Prints the list of updates to the stdout but" );
            Console.Out.WriteLine( "                                does not actually execute them." );
            Console.Out.WriteLine( "   -v, --verbose                Reports to the stdout all communications between the utility and the" );
            Console.Out.WriteLine( "                                issue tracking databases." );
            Console.Out.WriteLine( "   -?, --help                   Prints this message." );
            Console.Out.WriteLine();
        }

        private void ValidateOptions()
        {
            if( this.Mode == SyncAppMode.None )
            {
                ReportCommandLineError( "Configuration file (--config) must be specified" );
            }
        }
    }
}
