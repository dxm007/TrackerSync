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
    /// <summary>
    /// Identifies logging level
    /// </summary>
    public enum SyncLogLevel
    {
        /// <summary>
        /// No logging
        /// </summary>
        None,

        /// <summary>
        /// Report tracker source update actions
        /// </summary>
        PrintActions,

        /// <summary>
        /// Detailed logging
        /// </summary>
        Verbose
    }


    /// <summary>
    /// Settings class for a synchronizer engine
    /// </summary>
    /// <remarks>
    /// Contains settings defining full behavior of a SyncEngine object. As part of those settings
    /// are two instances of SourceSettings classes which define the two tracker sources to be
    /// synchronized with each other.
    /// </remarks>
    public class SyncSettings
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Creates a new instance of SyncSettings
        /// </summary>
        public SyncSettings()
        {
            this.LogLevel = SyncLogLevel.None;
            this.NoUpdates = false;
        }

        /// <summary>
        /// Creates a new instance of SyncSettings using the XML file as the source.
        /// </summary>
        /// <param name="filePath">Path to the XML file to be deserialized</param>
        public SyncSettings( string filePath ) : this()
        {
            Load( filePath );
        }

        /// <summary>
        /// Gets/sets logging level to be used by the SyncEngine
        /// </summary>
        public SyncLogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets/sets a flag which indicates whether or not update requests should be sent back
        /// to the tracker sources.
        /// </summary>
        public bool NoUpdates { get; set; }

        /// <summary>
        /// Gets an array of SourceSettings objects describing configuration of each tracker
        /// source.  This array will always have exactly 2 elements which represent the trackers
        /// to be synchronized with each other
        /// </summary>
        internal Sources.SourceSettings[] SourceSettings { get; private set; }

        /// <summary>
        /// Initializes the SyncSettings object by deserializing the XML document 
        /// </summary>
        /// <param name="filePath">Path to the XML document to be deserialized</param>
        public void Load( string filePath )
        {
            Load( XElement.Load( filePath ) );
        }

        /// <summary>
        /// Initializes the SyncSettings object by deserializing the XML document 
        /// </summary>
        /// <param name="rootElement">XML document to be deserialized</param>
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

        #endregion

        #region ----------------------- Private Members -----------------------

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

        #endregion
    }
}
