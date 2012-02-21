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
    /// <summary>
    /// GitHub connection verification request which is issued when the source is first connected
    /// </summary>
    class VerifyInfoRequest : HttpRequest
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public VerifyInfoRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <summary>
        /// Invoked to execute the verification request.
        /// </summary>
        /// <remarks>
        /// This method has no input or output parameters. If anything goes wrong during verification
        /// process, an exception will be thrown.
        /// </remarks>
        public void Execute()
        {
            SendRequest( BaseHttpReqUrlType.None, "/user" );
        }

        /// <inheritdoc/>
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


    /// <summary>
    /// GitHub API "GetIssuesList" HTTP REST request
    /// </summary>
    class GetListOfIssuesRequest : HttpRequest,
                                   IGetIssuesListRequest
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public GetListOfIssuesRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public IEnumerable<Issue> Execute()
        {
            SendRequest( GetUrlSuffix( SourceSettings ) );

            return _issues;
        }

        /// <inheritdoc/>
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


    /// <summary>
    /// GitHub API "GetIssue" HTTP REST request
    /// </summary>
    class GetIssueRequest : HttpRequest,
                            IGetIssueRequest
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public GetIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public Issue Execute( string issueId )
        {
            SendRequest( "/issues/{0}", issueId );

            return _issue;
        }

        /// <inheritdoc/>
        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonIssue = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issue = new Issue() { ID =          ( (int)jsonIssue[ "number" ] ).ToString(),
                                   Description = (string)jsonIssue[ "title" ],
                                   Details =     (string)jsonIssue[ "body" ],
                                   State =       (string)jsonIssue[ "state" ] == "open" ?
                                                            IssueState.Open : IssueState.Closed };
        }

        /// <inheritdoc/>
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


    /// <summary>
    /// GitHub API "CloseIssue" HTTP REST request
    /// </summary>
    class CloseIssueRequest : HttpRequest,
                              ICloseIssueRequest
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public CloseIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public void Execute( Issue issue )
        {
            SendRequest( "/issues/{0}", issue.ID );
        }

        /// <inheritdoc/>
        protected override string GetHttpMethod()
        {
            return "PATCH";
        }

        /// <inheritdoc/>
        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            base.FillInHttpRequest( request );

            var writer = new StreamWriter( request.GetRequestStream() );

            writer.Write( "{\"state\":\"closed\"}" );

            writer.Close();
        }
    }


    /// <summary>
    /// GitHub API "AddIssue" HTTP REST request
    /// </summary>
    class AddIssueRequest : HttpRequest,
                            IAddIssueRequest
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public AddIssueRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public void Execute( Issue issue )
        {
            _issue = issue;

            SendRequest( "/issues" );
        }

        /// <inheritdoc/>
        protected override string GetHttpMethod()
        {
            return "POST";
        }

        /// <inheritdoc/>
        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            base.FillInHttpRequest( request );

            request.ContentType = "application/vnd.github.v3+json";
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void HandleResponse( HttpWebResponse httpResponse, Stream responseStream )
        {
            JObject jsonIssue = JObject.Load( new JsonTextReader( new StreamReader( responseStream ) ) );

            _issue.ID = ( (int)jsonIssue[ "number" ] ).ToString();
        }


        private Issue   _issue;
    }
}
