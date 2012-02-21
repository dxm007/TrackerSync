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
    /// <summary>
    /// Type of action that needs to be taken in order to resolve a difference
    /// </summary>
    public enum ResolverActionType
    {
        /// <summary>
        /// Issue needs to be added
        /// </summary>
        Add,

        /// <summary>
        /// Issue needs to be closed
        /// </summary>
        Close
    }


    /// <summary>
    /// Identifies the source that needs to be resolved
    /// </summary>
    public enum ResolverSourceType
    {
        /// <summary>
        /// Identifies a source whose issues list is designated as 'primary'
        /// </summary>
        Primary,

        /// <summary>
        /// Identifies a source whose issues list is designated as 'secondary'
        /// </summary>
        Secondary
    }


    /// <summary>
    /// Defines extension methods on ResolveerSourceType enum type
    /// </summary>
    static class ResolverSourceTypeExtension
    {
        /// <summary>
        /// Helper method for returning the opposite type from the one that is specified.
        /// </summary>
        /// <param name="value">source type identifier</param>
        /// <returns>Source type opposite of the one that was passed in</returns>
        public static ResolverSourceType Other( this ResolverSourceType value )
        {
            return value == ResolverSourceType.Primary ? ResolverSourceType.Secondary :
                                                         ResolverSourceType.Primary;
        }
    }


    /// <summary>
    /// Event data class for the ItemResolver class's NeedItemLookup event
    /// </summary>
    public class ResolverNeedItemLookupEventArgs : EventArgs
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="id">Identifier of the issue that needs to be retrieved</param>
        /// <param name="source">Identifies the source fro which the issue is to be retrieved</param>
        public ResolverNeedItemLookupEventArgs( string              id,
                                                ResolverSourceType  source )
        {
            this.ID = id;
            this.Source = source;
        }

        /// <summary>
        /// Gets the unique identifier of the issue which needs to be retrieved
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Gets the identifier of the source from which the issue is to be retrieved
        /// </summary>
        public ResolverSourceType Source { get; private set; }

        /// <summary>
        /// Gets/sets the issue that was retrieved. The event handler should set this property
        /// after the issue identified by 'ID' is retrieved from source identified by 'Source'
        /// </summary>
        public Issue Issue { get; set; }
    }


    /// <summary>
    /// Event data for IssueResolver class's Action event
    /// </summary>
    public class ResolverActionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="issue">Issue associated with the action</param>
        /// <param name="source">Identifies the source on which the action to take place</param>
        /// <param name="action">Identifies the action to perform</param>
        public ResolverActionEventArgs( Issue               issue,
                                        ResolverSourceType  source,
                                        ResolverActionType  action  )
        {
            this.Issue = issue;
            this.Source = source;
            this.Action = action;
        }

        /// <summary>
        /// Gets the issue which is associated with the action
        /// </summary>
        public Issue Issue { get; private set; }

        /// <summary>
        /// Gets the identifier of the source on which the action is to take place
        /// </summary>
        public ResolverSourceType Source { get; private set; }

        /// <summary>
        /// Gets the identifier of the action which is to be performed
        /// </summary>
        public ResolverActionType Action { get; private set; }
    }

    
    /// <summary>
    /// Issue resolver class which accepts two lists of issues, one from each source,
    /// and determines which actions should take place to consolidate the differences
    /// between the two lists.
    /// </summary>
    public class IssueResolver
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor. This constructor accepts two lists of issues. Each
        /// one is designated as either primary or secondary, but that distinction is made
        /// only so that each list can be uniquely identified.
        /// </summary>
        /// <param name="primaryList">List of issues from the primary source</param>
        /// <param name="secondaryList">List of issues from the secondary source</param>
        public IssueResolver( IEnumerable<Issue>    primaryList,
                              IEnumerable<Issue>    secondaryList )
        {
            _primaryList = primaryList;
            _secondaryList = secondaryList;
        }

        #region - - - - - - - Events  - - - - - - - - - - - - - - - -

        /// <summary>
        /// Gets fired if the resolver needs an item to be retrieved.
        /// </summary>
        /// <remarks>
        /// This event is used when resolving lists where only open items are retrieved from
        /// the source. In that case, an extra retrieval call for a specific issue needs to be
        /// issued in order to determine if an issue is new or if it's an existing one that 
        /// was closed
        /// </remarks>
        public event EventHandler< ResolverNeedItemLookupEventArgs >    NeedItemLookup;

        /// <summary>
        /// Gets fired when IssueResolver identifies an action that needs to take place in order
        /// to consolidate the differences between the two lists.
        /// </summary>
        public event EventHandler< ResolverActionEventArgs >            Action;

        #endregion

        /// <summary>
        /// Should be invoked in order to consolidate the two lists. This method will traverse
        /// the lists passed in through the constructor. All feedback from this class is generated
        /// through the public events of the IssueResolver
        /// </summary>
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

        #endregion

        #region ----------------------- Private Members -----------------------

        private void VerifyEventSubscriptions()
        {
            if( this.NeedItemLookup == null || this.Action == null )
            {
                throw new ApplicationException( 
                    "IssueResolver MUST have both events subscribed for in order to function" );
            }
        }

        private void HandleOneSidedIssue( Issue primaryIssue, Issue secondaryIssue )
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

        private void HandleIssueToClose( Issue primaryIssue, Issue secondaryIssue )
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


        private IEnumerable<Issue>      _primaryList;
        private IEnumerable<Issue>      _secondaryList;

        #endregion
    }
}
