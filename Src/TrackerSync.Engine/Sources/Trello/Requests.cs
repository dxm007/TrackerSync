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
using System.Net;
using System.Text;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TrackerSync.Data;


namespace TrackerSync.Sources.Trello
{
    class ResolveBoardIdRequest : HttpRequest
    {
        public ResolveBoardIdRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute()
        {
            SendRequest( "/members/{0}/boards?filter=open&fields=name", SourceSettings.UserName );
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JArray jsonBoards = JArray.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            JObject board = jsonBoards.Cast< JObject >().FirstOrDefault( 
                                        x => string.Compare( this.SourceSettings.BoardName,
                                                             (string)x[ "name" ], true      ) == 0 );

            if( board == null )
            {
                throw new ApplicationException( string.Format( 
                                        "Board '{0}' for user '{1}' was not found",
                                        SourceSettings.BoardName, SourceSettings.UserName ) );
            }

            SourceSettings.BoardId = (string)board[ "id" ];
        }
    }



    class ResolveListIdsRequest : HttpRequest
    {
        public ResolveListIdsRequest( Sources.SourceSettings settings ) : base( settings )
        {
            _nameIdMap = new Dictionary<string,string>();
        }

        public void Execute()
        {
            SendRequest( "/boards/{0}/lists?cards=none&filter=open&fields=name", SourceSettings.BoardId );
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JArray jsonLists = JArray.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            var result = from list in jsonLists
                         select new { Id = (string)list[ "id" ],
                                      Name = (string)list[ "name" ] };

            foreach( var r in result )
            {
                _nameIdMap[ r.Name ] = r.Id;
            }

            SourceSettings.OpenCardListIds = TranslateNamesToIds( SourceSettings.OpenCardLists, true );
            SourceSettings.ClosedCardListIds = TranslateNamesToIds( SourceSettings.ClosedCardLists, false );

            if( string.IsNullOrEmpty( SourceSettings.NewCardListId ) )
            {
                SourceSettings.NewCardListId = TranslateNameToId( SourceSettings.NewCardList );

                string[] openCardsListIds = SourceSettings.OpenCardListIds;

                Array.Resize( ref openCardsListIds, openCardsListIds.Length + 1 );
                openCardsListIds[ openCardsListIds.Length - 1 ] = SourceSettings.NewCardListId;
            }
        }

        private string TranslateNameToId( string name )
        {
            string  id;

            if( !_nameIdMap.TryGetValue( name, out id ) )
            {
                throw new ApplicationException( string.Format(
                    "Unable to find list '{0}' for board '{1}/{2}'",
                    name, SourceSettings.UserName, SourceSettings.BoardName ) );
            }

            return id;
        }

        private string[] TranslateNamesToIds( string[] names, bool allowNewCardList )
        {
            return names.Select( x =>
            {
                string id = TranslateNameToId( x );

                if( string.Compare( SourceSettings.NewCardList, x, true ) == 0 )
                {
                    if( !allowNewCardList )
                    {
                        throw new ApplicationException( string.Format(
                            "List '{0}' assigned to new items appears in closed issues list", x ) );
                    }

                    SourceSettings.NewCardListId = id;
                }

                return id;

            } ).ToArray();
        }

        private Dictionary< string, string >    _nameIdMap;
    }



    class GetListOfIssuesRequest : HttpRequest,
                                   IGetIssuesListRequest
    {
        public GetListOfIssuesRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public IEnumerable< Issue > Execute()
        {
            SendRequest( "/boards/{0}/lists?card_fields=name,desc", SourceSettings.BoardId );

            return _issues;
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JArray jsonLists = JArray.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            var query = SourceSettings.GetAllIncludesClosedIssues ?
                ( from list in jsonLists
                  select new { Cards = list[ "cards" ],
                               IsOpen = SourceSettings.OpenCardListIds.Contains( (string)list[ "id" ] )   ? (bool?)true  :
                                        SourceSettings.ClosedCardListIds.Contains( (string)list[ "id" ] ) ? (bool?)false : null } ) :
                ( from list in jsonLists
                  where SourceSettings.OpenCardListIds.Contains( (string)list[ "id" ] )
                  select new { Cards = list[ "cards" ], IsOpen=(bool?)true }            );

            _issues = query.Where( x => x.IsOpen != null )
                           .SelectMany( x => x.Cards, ( x, card ) => 
                                    new Issue() { ID = (string)card[ "id" ],
                                                  Description = (string)card[ "name" ],
                                                  Details = (string)card[ "desc" ],
                                                  State = x.IsOpen.Value ? IssueState.Open : IssueState.Closed } );
        }

        private IEnumerable< Issue >     _issues;
    }



    class GetIssueRequest : HttpRequest,
                            IGetIssueRequest
    {
        public GetIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public Issue Execute( string issueId )
        {
            SendRequest( "/cards/{0}?fields=name,desc,idList", issueId );

            return _issue;
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonCard = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issue = new Issue() { ID = (string)jsonCard[ "id" ],
                                   Description = (string)jsonCard[ "name" ],
                                   Details = (string)jsonCard[ "desc" ]      };

            string listId = (string)jsonCard[ "idList" ];

            if( SourceSettings.OpenCardListIds.Contains( listId ) )
            {
                _issue.State = IssueState.Open;
            }
            else if( SourceSettings.ClosedCardListIds.Contains( listId ) )
            {
                _issue.State = IssueState.Closed;
            }
            else
            {
                throw new ApplicationException( string.Format(
                                "Unable to determine state for issue '{0}'", listId ) );
            }
        }


        private Issue _issue;
    }



    class CloseIssueRequest : HttpRequest,
                              ICloseIssueRequest
    {
        public CloseIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute( Issue issue )
        {
            SendRequest( "/cards/{0}/idList?value={1}",
                         issue.ID, SourceSettings.ClosedCardListIds[0] );
        }

        protected override string GetHttpMethod()
        {
            return "PUT";
        }
    }



    class AddIssueRequest : HttpRequest,
                            IAddIssueRequest
    {
        public AddIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute( Issue issue )
        {
            _issue = issue;

            SendRequest( "/cards" );
        }

        protected override string GetHttpMethod()
        {
            return "POST";
        }

        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            request.Headers.Add( "name", _issue.Description );
            request.Headers.Add( "idList", SourceSettings.NewCardListId );

            if( !string.IsNullOrEmpty( _issue.Details ) )
            {
                request.Headers.Add( "desc", _issue.Details );
            }
        }

        
        private Issue       _issue;
    }



    class UpdateIssueRequest : HttpRequest,
                               IUpdateIssueRequest
    {
        public UpdateIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute( Issue issue, IssueFieldId fieldsToUpdate)
        {
            int     fieldCount = 0;
            var     sb = new StringBuilder();

            sb.AppendFormat( "/cards/{0}", issue.ID );

            foreach( var x in fieldsToUpdate.Flags().Cast<IssueFieldId>() )
            {
                sb.Append( fieldCount == 0 ? '?' : '&' );

                switch( x )
                {
                case IssueFieldId.Desc:
                    sb.AppendFormat( "name={0}", HttpUtility.UrlEncode( issue.Description ) );
                    break;

                case IssueFieldId.Details:
                    sb.AppendFormat( "desc={1}", HttpUtility.UrlEncode( issue.Details ) );
                    break;

                default:
                    throw new ApplicationException( string.Format(
                                    "Update of {0} field is not supported",
                                    Enum.GetName( typeof( IssueFieldId ), x ) ) );
                }

                fieldCount++;
            }

            if( fieldCount > 0 )
            {
                SendRequest( sb.ToString() );
            }
        }

        protected override string GetHttpMethod()
        {
            return "PUT";
        }
    }
}
