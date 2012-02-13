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

namespace TrackerSync.Sources
{
    public class SourceFactory
    {
        public SourceFactory()
        {
            LogLevel = SyncLogLevel.None;
            NoUpdates = false;
        }

        public SyncLogLevel LogLevel { get; set; }

        public bool NoUpdates { get; set; }

        public ISource Create( SourceSettings settings )
        {
            ISource     source = CreateSource( settings );

            if( NoUpdates )
            {
                source = new ReadOnlySourceDecorator( source );
            }

            if( LogLevel != SyncLogLevel.None )
            {
                LoggingSourceConfig     loggingConfig = new LoggingSourceConfig();

                loggingConfig.IsInputLogged = ( LogLevel >= SyncLogLevel.Verbose );
                loggingConfig.IsOutputLogged = ( LogLevel >= SyncLogLevel.PrintActions );

                source = new LoggingSourceDecorator( source,
                                                     Console.Out,
                                                     loggingConfig );
            }

            return source;
        }

        private ISource CreateSource( SourceSettings settings )
        {
            Type settingsType = settings.GetType();

            if( settingsType == typeof( GitHub.SourceSettings ) )
            {
                return new GitHub.Source( (GitHub.SourceSettings)settings );
            }
            else if( settingsType == typeof( Trello.SourceSettings ) )
            {
                return new Trello.SourceNormalizer(
                            new Trello.Source( (Trello.SourceSettings)settings ) );
            }
            else
            {
                throw new ApplicationException( string.Format(
                    "Unknown settings type: {0}", settingsType.Name ) );
            }
        }
    }
}
