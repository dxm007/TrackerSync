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
    public class SyncEngine
    {
        #region ----------------------- Public Members ------------------------

        SyncEngine()
        {
            _actionListBuilder = new SyncActionListBuilder( GetSourceByType );
        }

        public SyncEngine( Sources.ISource      primarySource,
                           Sources.ISource      secondarySource ) : this()
        {
            _primarySource = primarySource;
            _secondarySource = secondarySource;
        }

        public SyncEngine( SyncSettings     settings ) : this()
        {
            var sourceFactory = new Sources.SourceFactory();

            sourceFactory.LogLevel = settings.LogLevel;
            sourceFactory.NoUpdates = settings.NoUpdates;

            _primarySource = sourceFactory.Create( settings.SourceSettings[0] );
            _secondarySource = sourceFactory.Create( settings.SourceSettings[1] );
        }

        #region - - - - - - - Properties  - - - - - - - - - - - - - -
        #endregion

        #region - - - - - - - Events  - - - - - - - - - - - - - - - -
        #endregion

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
        }

        #endregion

        #region ----------------------- Protected Members ---------------------

        #endregion

        #region ----------------------- Private Members -----------------------

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

    class SyncActionListBuilder
    {
        public SyncActionListBuilder( SourceFromTypeDelegate sourceFromType )
        {
            _sourceFromType = sourceFromType;
            _actions = new List<SyncAction>();
        }

        public IEnumerable< SyncAction > Actions
        {
            get { return _actions; }
        }

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

        private void OnAddIssue( ResolverSourceType sourceType, Issue issue )
        {
            Sources.ISource     destination = _sourceFromType( sourceType );
            Sources.ISource     source = _sourceFromType( sourceType.Other() );

            _actions.Add( new AddIssueAction( destination, issue ) );

            if( destination.Settings.IsPrimary )
            {
                _actions.Add( new UpdateIssueAction( source, issue, IssueFieldId.ID ) );
            }
        }

        private void OnCloseIssue( ResolverSourceType sourceType, Issue issue )
        {
            _actions.Add( new CloseIssueSyncAction( _sourceFromType( sourceType ), issue ) );
        }


        SourceFromTypeDelegate          _sourceFromType;
        private List< SyncAction >      _actions;
    }
}
