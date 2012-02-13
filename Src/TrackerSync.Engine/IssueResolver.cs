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

using TrackerSync.Data;

namespace TrackerSync.Engine
{
    public enum ResolverSourceType
    {
        Primary,
        Secondary
    }

    static class ResolverSourceTypeExtension
    {
        public static ResolverSourceType Other( this ResolverSourceType value )
        {
            return value == ResolverSourceType.Primary ? ResolverSourceType.Secondary :
                                                         ResolverSourceType.Primary;
        }
    }


    public enum ResolverActionType
    {
        Add,
        Close
    }

    public class ResolverNeedItemLookupEventArgs : EventArgs
    {
        public ResolverNeedItemLookupEventArgs( string              id,
                                                ResolverSourceType  source )
        {
            this.ID = id;
            this.Source = source;
        }

        public string ID { get; private set; }
        public ResolverSourceType Source { get; private set; }
        public Issue Issue { get; set; }
    }

    public class ResolverActionEventArgs : EventArgs
    {
        public ResolverActionEventArgs( Issue               issue,
                                        ResolverSourceType  source,
                                        ResolverActionType  action  )
        {
            this.Issue = issue;
            this.Source = source;
            this.Action = action;
        }

        public Issue Issue { get; private set; }
        public ResolverSourceType Source { get; private set; }
        public ResolverActionType Action { get; private set; }
    }

    

    public class IssueResolver
    {
        #region ----------------------- Public Members ------------------------

        public IssueResolver( IEnumerable<Issue>    primaryList,
                              IEnumerable<Issue>    secondaryList )
        {
            _primaryList = primaryList;
            _secondaryList = secondaryList;
        }

        #region - - - - - - - Events  - - - - - - - - - - - - - - - -

        public event EventHandler< ResolverNeedItemLookupEventArgs >    NeedItemLookup;

        public event EventHandler< ResolverActionEventArgs >            Action;

        #endregion

        #endregion

        public void Resolve()
        {
            Dictionary< string, Issue >     primaryIssues = _primaryList.ToDictionary( x => x.Description );
            Dictionary< string, Issue >     secondaryIssues = _secondaryList.ToDictionary( x => x.Description );

            VerifyEventSubscriptions();

            foreach( var primary in primaryIssues.Values )
            {
                Issue   secondary;

                if( !secondaryIssues.TryGetValue( primary.Description, out secondary ) )
                {
                    HandleOneSidedIssue( primary, null );
                }
                else
                {
                    if( primary.State != secondary.State )
                    {
                        HandleIssueToClose( primary, secondary );
                    }

                    secondaryIssues.Remove( secondary.Description );
                }
            }

            primaryIssues.Clear();

            foreach( var secondary in secondaryIssues.Values )
            {
                HandleOneSidedIssue( null, secondary );
            }
        }

        #region ----------------------- Private Members -----------------------

        void VerifyEventSubscriptions()
        {
            if( this.NeedItemLookup == null || this.Action == null )
            {
                throw new ApplicationException( 
                    "IssueResolver MUST have both events subscribed for in order to function" );
            }
        }

        void HandleOneSidedIssue( Issue primaryIssue, Issue secondaryIssue )
        {
            Issue                   existingIssue;
            ResolverSourceType      existingSource;
            ResolverSourceType      missingSource;

            System.Diagnostics.Debug.Assert( 
                        ( primaryIssue == null ) != ( secondaryIssue == null ) );

            if( primaryIssue == null )
            {
                existingIssue = secondaryIssue;
                existingSource = ResolverSourceType.Secondary;
                missingSource = ResolverSourceType.Primary;
            }
            else
            {
                existingIssue = primaryIssue;
                existingSource = ResolverSourceType.Primary;
                missingSource = ResolverSourceType.Secondary;
            }

            if( existingIssue.State == IssueState.Closed ) return;

            var eventData = new ResolverNeedItemLookupEventArgs( 
                                        existingIssue.ID, missingSource );

            if( !string.IsNullOrEmpty( existingIssue.ID ) )
            {
                this.NeedItemLookup( this, eventData );
            }

            if( eventData.Issue == null )
            {
                this.Action( this, new ResolverActionEventArgs( 
                                existingIssue, missingSource, ResolverActionType.Add ) );
            }
            else if( eventData.Issue.State == IssueState.Closed )
            {
                this.Action( this, new ResolverActionEventArgs( 
                                existingIssue, existingSource, ResolverActionType.Close ) );
            }
        }

        void HandleIssueToClose( Issue primaryIssue, Issue secondaryIssue )
        {
            Issue               issueToClose;
            ResolverSourceType  sourceToClose;

            if( primaryIssue.State == IssueState.Closed )
            {
                issueToClose = secondaryIssue;
                sourceToClose = ResolverSourceType.Secondary;
            }
            else
            {
                issueToClose = primaryIssue;
                sourceToClose = ResolverSourceType.Primary;
            }

            this.Action( this, new ResolverActionEventArgs( 
                            issueToClose, sourceToClose, ResolverActionType.Close ) );
        }


        IEnumerable<Issue>      _primaryList;
        IEnumerable<Issue>      _secondaryList;

        #endregion
    }
}
