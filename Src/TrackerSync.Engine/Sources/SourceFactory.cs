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
    /// <summary>
    /// Tracker source factory class.
    /// </summary>
    /// <remarks>
    /// Implements FACTORY design pattern to create new tracker source objects based on few 
    /// global settings as well as SourceSettings instance describing the configuration for the
    /// source to be created.
    /// 
    /// Current implementation of this class breaks OCP principle by maintaining a hardcoded
    /// list of different tracker source types with intent of keeping the desing simple since
    /// at the moment very few source types are supported.  In the future, we would probably 
    /// changes this to use reflection or some other dynamic registration mechanism.
    /// </remarks>
    public class SourceFactory
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Creates a new instances of the SourceFactory
        /// </summary>
        public SourceFactory()
        {
            LogLevel = SyncLogLevel.None;
            NoUpdates = false;
        }

        /// <summary>
        /// Gets/sets the logging level to applied to the source
        /// </summary>
        public SyncLogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets/sets a flag which indicates whether the updates should be sent back to the
        /// source, or if the source should act as if it's read-only.
        /// </summary>
        public bool NoUpdates { get; set; }

        /// <summary>
        /// Creates a new instance of the source tracker.
        /// </summary>
        /// <param name="settings">Source settings object describing the source configuration</param>
        /// <returns>Newly created source instance</returns>
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

        #endregion

        #region ----------------------- Private Members -----------------------

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

        #endregion
    }
}
