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
using System.Text;

using TrackerSync.Data;


namespace TrackerSync.Sources
{
    /// <summary>
    /// Configuration class for LoggingSourceDecorator
    /// </summary>
    public class LoggingSourceConfig
    {
        /// <summary>
        /// Initializes an instance of LoggingSourceConfig with default settings
        /// </summary>
        public LoggingSourceConfig()
        {
        }

        /// <summary>
        /// Initializes an instance of LoggingSourceConfig from another instance of
        /// the class
        /// </summary>
        /// <param name="other">Existing instance which is providing source values</param>
        public LoggingSourceConfig( LoggingSourceConfig other )
        {
            this.IsInputLogged = other.IsInputLogged;
            this.IsOutputLogged = other.IsOutputLogged;
        }

        /// <summary>
        /// Gets/sets a flag indicating if input calls (i.e. those that get data from tracker
        /// sources) should be logged
        /// </summary>
        public bool IsInputLogged { get; set; }

        /// <summary>
        /// Gets/sets a flag indicating if output calls (i.e. those that write data to
        /// tracker sources) should be logged
        /// </summary>
        public bool IsOutputLogged { get; set; }
    }


    /// <summary>
    /// Source decorator which adds logging capability to every call on the ISource interface
    /// </summary>
    public class LoggingSourceDecorator : SourceDecorator
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="contained">Tracker source object whose calls are to be logged</param>
        /// <param name="logWriter">TextWriter which will receive log output</param>
        /// <param name="config">Configuration object for the LoggingSourceDecorator</param>
        public LoggingSourceDecorator( ISource              contained,
                                       TextWriter           logWriter,
                                       LoggingSourceConfig  config     ) : base( contained )
        {
            _logWriter = logWriter;
            _config = new LoggingSourceConfig( config );
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        /// <inheritdoc/>
        public override IEnumerable<Issue> GetIssues()
        {
            if( _config.IsInputLogged )
            {
                return GetLoggedIssues( base.GetIssues() );
            }
            else
            {
                return base.GetIssues();
            }
        }

        /// <inheritdoc/>
        public override Issue GetIssue( string id )
        {
            var issue = base.GetIssue( id );

            if( _config.IsInputLogged )
            {
                LogIssue( "GET", issue, () => {
                    if( issue == null )
                    {
                        _logWriter.WriteLine( "    Issue[{0}] does not exist", id );
                    }
                } );
            }

            return issue;
        }

        /// <inheritdoc/>
        public override void AddIssue( Issue issue )
        {
            if( _config.IsOutputLogged )
            {
                LogIssue( "ADD", issue );
            }

            Contained.AddIssue( issue );
        }

        /// <inheritdoc/>
        public override void UpdateIssue( Issue          issue,
                                          IssueFieldId   fieldsToUpdate )
        {
            if( _config.IsOutputLogged )
            {
                string actionText =
                    string.Format( "UPDATE( {0} )", fieldsToUpdate.ToString( "G" ) );

                LogIssue( actionText, issue );
            }

            Contained.UpdateIssue( issue, fieldsToUpdate );
        }

        /// <inheritdoc/>
        public override void CloseIssue( Issue issue )
        {
            if( _config.IsOutputLogged )
            {
                LogIssue( "CLOSE", issue );
            }

            Contained.CloseIssue( issue );
        }

        #endregion

        #endregion

        #region ----------------------- Private Members -----------------------

        private IEnumerable< Issue > GetLoggedIssues( IEnumerable< Issue > inputList )
        {
            int     count = 0;

            foreach( var issue in inputList )
            {
                LogIssue( string.Format( "GET_LIST[{0}]", count++ ), issue );

                yield return issue;
            }
        }

        private void LogIssue( string action, Issue issue )
        {
            LogIssue( action, issue, null );
        }

        private void LogIssue( string action, Issue issue, Action additionalText )
        {
            _logWriter.WriteLine( "{0}--{1}", Name, action );

            if( issue != null )
            {
                _logWriter.WriteLine( "    {0}: {1}",
                                      issue.ID, issue.Description );
                _logWriter.WriteLine( "    {0}", issue.Details );
            }

            if( additionalText != null )
            {
                additionalText();
            }

            _logWriter.WriteLine( "--------------------------------------------------------------" );
            _logWriter.WriteLine();
        }


        private TextWriter              _logWriter;
        private LoggingSourceConfig     _config;

        #endregion
    }
}
