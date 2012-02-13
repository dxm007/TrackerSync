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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TrackerSync.Data;


namespace TrackerSync.Sources.GitHub
{
    class VerifyInfoRequest : HttpRequest
    {
        public VerifyInfoRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute()
        {
            SendRequest( BaseHttpReqUrlType.None, "/user" );
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonUser = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            string userName = (string)jsonUser[ "login" ];

            if( userName != SourceSettings.UserName )
            {
                throw new ApplicationException( string.Format( 
                    "User name retrieved from source ('{0}') != login name ('{1}')",
                    userName, SourceSettings.UserName                                ) );
            }
        }
    }



    class GetListOfIssuesRequest : HttpRequest,
                                   IGetIssuesListRequest
    {
        public GetListOfIssuesRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public IEnumerable<Issue> Execute()
        {
            SendRequest( GetUrlSuffix( SourceSettings ) );

            return _issues;
        }

        protected override void HandleResponse( HttpWebResponse httpResponse,
                                                Stream          responseStream )
        {
            JArray jsonIssueList = JArray.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issues = from o in jsonIssueList
                      select new Issue() { ID =          ( (int)o[ "number" ] ).ToString(),
                                           Description = (string)o[ "title" ],
                                           Details =     (string)o[ "body" ],
                                           State =       IssueState.Open                    };
        }

        private static string GetUrlSuffix( SourceSettings settings )
        {
            if( settings.GetAllIncludesClosedIssues )
            {
                throw new ApplicationException( 
                    "GetAll w/ closed issues is unsupported for GitHub because that list has potential to grow indefinitely" );
            }

            return "/issues?state=open";
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
            SendRequest( "/issues/{0}", issueId );

            return _issue;
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonIssue = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issue = new Issue() { ID =          ( (int)jsonIssue[ "number" ] ).ToString(),
                                   Description = (string)jsonIssue[ "title" ],
                                   Details =     (string)jsonIssue[ "body" ],
                                   State =       (string)jsonIssue[ "state" ] == "open" ?
                                                            IssueState.Open : IssueState.Closed };
        }

        protected override void HandleHttpFailure( WebException exception )
        {
            if( exception.Status == WebExceptionStatus.ProtocolError &&
                ( (HttpWebResponse)exception.Response ).StatusCode == HttpStatusCode.NotFound )
            {
                _issue = null;
            }
            else
            {
                base.HandleHttpFailure( exception );
            }
        }


        private Issue   _issue;
    }



    class CloseIssueRequest : HttpRequest,
                              ICloseIssueRequest
    {
        public CloseIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        public void Execute( Issue issue )
        {
            SendRequest( "/issues/{0}", issue.ID );
        }

        protected override string GetHttpMethod()
        {
            return "PATCH";
        }

        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            var writer = new StreamWriter( request.GetRequestStream() );

            writer.Write( "{\"state\":\"closed\"}" );

            writer.Close();
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

            SendRequest( "/issues" );
        }

        protected override string GetHttpMethod()
        {
            return "POST";
        }

        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            base.FillInHttpRequest( request );

            request.ContentType = "application/vnd.github.v3+json";
        }

        protected override string GetRequestBody()
        {
            StringBuilder   sb = new StringBuilder();

            sb.Append( "{" );
            sb.AppendFormat( "\"title\":\"{0}\"", _issue.Description );
            
            if( !string.IsNullOrEmpty( _issue.Details ) )
            {
                sb.AppendFormat( ",\"body\": \"{0}\"", _issue.Details );
            }

            sb.Append( "}" );
            
            return sb.ToString();
        }

        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonIssue = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issue.ID = ( (int)jsonIssue[ "number" ] ).ToString();
        }


        private Issue   _issue;
    }
}
