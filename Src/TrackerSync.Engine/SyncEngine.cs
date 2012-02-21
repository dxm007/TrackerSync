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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TrackerSync.Data;

namespace TrackerSync.Engine
{
    /// <summary>
    /// Main synchronization engine class
    /// </summary>
    public class SyncEngine
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Creates a new instance of SyncEngine and initializes it using the specified settings
        /// </summary>
        /// <param name="settings">Synchronization engine settings to use</param>
        public SyncEngine( SyncSettings settings ) : this()
        {
            var sourceFactory = new Sources.SourceFactory();

            sourceFactory.LogLevel = settings.LogLevel;
            sourceFactory.NoUpdates = settings.NoUpdates;

            _primarySource = sourceFactory.Create( settings.SourceSettings[0] );
            _secondarySource = sourceFactory.Create( settings.SourceSettings[1] );
        }

        /// <summary>
        /// Runs the synchronization between the two tracker sources.
        /// </summary>
        /// <remarks>
        /// This method performs the following:
        ///     - Establishes connections with the two synchronization sources
        ///     - Retrieves lists of existing issues from each source
        ///     - Compares the two lists to find differences.
        ///         - for each difference found it creates one or more synchonization actions
        ///     - Sequentially executes each synchronization action that was created in the
        ///       previous step
        ///     - Disconnects from the synchronization sources.
        /// </remarks>
        public void Execute()
        {
            ValidateSources();

            _secondarySource.Connect();
            _primarySource.Connect();

            IssueResolver resolver = new IssueResolver( _primarySource.GetIssues(),
                                                        _secondarySource.GetIssues() );

            resolver.NeedItemLookup += OnResolverNeedItemLookup;
            resolver.Action += _actionListBuilder.OnResolverActionEvent;

            resolver.Resolve();

            foreach( var a in _actionListBuilder.Actions )
            {
                a.Run();
            }

            _primarySource.Disconnect();
            _secondarySource.Disconnect();
        }

        #endregion

        #region ----------------------- Private Members -----------------------

        private SyncEngine()
        {
            _actionListBuilder = new SyncActionListBuilder( GetSourceByType );
        }

        private void ValidateSources()
        {
            // One of the sources must be marked primary, but not both. Primary is the one that generates
            // issue IDs.  Secondary must always synchronize with IDs of the primary.
            if( !( _primarySource.Settings.IsPrimary ^ _secondarySource.Settings.IsPrimary ) )
            {
                throw new ApplicationException( "One and only one source must be the primary one" );
            }
        }

        private void OnResolverNeedItemLookup( object sender, ResolverNeedItemLookupEventArgs e )
        {
            e.Issue = GetSourceByType( e.Source ).GetIssue( e.ID );
        }

        private Sources.ISource GetSourceByType( ResolverSourceType sourceType )
        {
            return sourceType == ResolverSourceType.Primary ? _primarySource : _secondarySource;
        }


        private Sources.ISource         _primarySource;
        private Sources.ISource         _secondarySource;
        private SyncActionListBuilder   _actionListBuilder;

        #endregion
    }



    delegate Sources.ISource SourceFromTypeDelegate( ResolverSourceType sourceType );

    /// <summary>
    /// Helper class for building a synchronizer action list
    /// </summary>
    class SyncActionListBuilder
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Constructs a new instance of SyncActionListBuilder
        /// </summary>
        /// <param name="sourceFromType">Delegate which is to be invoked to translate ResolverSourceType enum 
        /// values used by IssueResolver into actual source object references</param>
        public SyncActionListBuilder( SourceFromTypeDelegate sourceFromType )
        {
            _sourceFromType = sourceFromType;
            _actions = new List<SyncAction>();
        }

        /// <summary>
        /// Gets a list of synchronizer actions
        /// </summary>
        public IEnumerable< SyncAction > Actions
        {
            get { return _actions; }
        }

        /// <summary>
        /// To be hooked up to IssueResolver class's Action event. This method processes the event and creates
        /// one or more synchronizer actions which are then added to the internally maintained action list
        /// </summary>
        /// <param name="sender">Sender object of the event</param>
        /// <param name="e">Data associated with 'Action' event</param>
        public void OnResolverActionEvent( object sender, ResolverActionEventArgs e )
        {
            switch( e.Action )
            {
            case ResolverActionType.Add:
                OnAddIssue( e.Source, e.Issue );
                break;
            case ResolverActionType.Close:
                OnCloseIssue( e.Source, e.Issue );
                break;
            default:
                throw new ApplicationException( string.Format( 
                    "Unknown action type({0})",
                    Enum.GetName( typeof( ResolverActionType ), e.Action ) ) );
            }
        }

        #endregion

        #region ----------------------- Private Members -----------------------

        private void OnAddIssue( ResolverSourceType sourceType, Issue issue )
        {
            Sources.ISource     destination = _sourceFromType( sourceType );
            Sources.ISource     source = _sourceFromType( sourceType.Other() );

            _actions.Add( new AddIssueSyncAction( destination, issue ) );

            if( destination.Settings.IsPrimary )
            {
                _actions.Add( new UpdateIssueSyncAction( source, issue, IssueFieldId.ID ) );
            }
        }

        private void OnCloseIssue( ResolverSourceType sourceType, Issue issue )
        {
            _actions.Add( new CloseIssueSyncAction( _sourceFromType( sourceType ), issue ) );
        }

        SourceFromTypeDelegate          _sourceFromType;
        private List< SyncAction >      _actions;

        #endregion
    }
}
