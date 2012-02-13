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
using System.Xml.Linq;


namespace TrackerSync
{
    public enum SyncLogLevel
    {
        None,
        PrintActions,
        Verbose
    }



    public class SyncSettings
    {
        public SyncSettings()
        {
            this.LogLevel = SyncLogLevel.None;
            this.NoUpdates = false;
        }

        public SyncSettings( string filePath ) : this()
        {
            Load( filePath );
        }

        public SyncLogLevel LogLevel { get; set; }

        public bool NoUpdates { get; set; }

        internal Sources.SourceSettings[] SourceSettings { get; private set; }

        public void Load( string filePath )
        {
            Load( XElement.Load( filePath ) );
        }

        public void Load( XElement rootElement )
        {
            this.SourceSettings = rootElement.Elements( "TrackerConfig" ).Select( ( src, idx ) =>
                {
                    if( idx >= 2 )
                    {
                        throw new ApplicationException( "Too many sources in configuration file" );
                    }

                    return CreateSourceConfig( src );

                } ).ToArray();
        }

        private TrackerSync.Sources.SourceSettings CreateSourceConfig( XElement sourceElement )
        {
            TrackerSync.Sources.SourceSettings  sourceSettings;
            
            string sourceType = (string)sourceElement.Attribute( "type" );

            switch( sourceType.ToLower() )
            {
            case "trello":
                sourceSettings = new TrackerSync.Sources.Trello.SourceSettings();
                break;
            case "github":
                sourceSettings = new TrackerSync.Sources.GitHub.SourceSettings();
                break;
            default:
                throw new ApplicationException( string.Format( "Unknown source type '{0}'", sourceType ) );
            }

            sourceSettings.Load( sourceElement );

            return sourceSettings;
        }
    }
}
