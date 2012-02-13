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
    public class LoggingSourceConfig
    {
        public LoggingSourceConfig()
        {
        }

        public LoggingSourceConfig( LoggingSourceConfig other )
        {
            this.IsInputLogged = other.IsInputLogged;
            this.IsOutputLogged = other.IsOutputLogged;
        }

        public bool IsInputLogged { get; set; }

        public bool IsOutputLogged { get; set; }
    }


    public class LoggingSourceDecorator : SourceDecoratorBase
    {
        #region ----------------------- Public Members ------------------------

        public LoggingSourceDecorator( ISource              contained,
                                       TextWriter           logWriter,
                                       LoggingSourceConfig  config     ) : base( contained )
        {
            _logWriter = logWriter;
            _config = new LoggingSourceConfig( config );
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

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

        public override void AddIssue( Issue issue )
        {
            if( _config.IsOutputLogged )
            {
                LogIssue( "ADD", issue );
            }

            Contained.AddIssue( issue );
        }

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
