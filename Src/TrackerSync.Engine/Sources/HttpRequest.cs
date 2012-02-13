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

namespace TrackerSync.Sources
{
    abstract class HttpRequest
    {
        public HttpRequest( SourceSettings settings )
        {
            this.SourceSettings = settings;
        }

        protected SourceSettings SourceSettings { get; private set; }

        protected void SendRequest( string          url,
                                    ICredentials    credentials )
        {
            try
            {
                ServicePointManager.Expect100Continue = false;

                _httpRequest = (HttpWebRequest)WebRequest.Create( url );
                _httpRequest.Credentials = credentials;
                _httpRequest.Method = GetHttpMethod();

                FillInHttpRequest( _httpRequest );

                AddBodyToRequest( GetRequestBody() );

                var response = (HttpWebResponse)_httpRequest.GetResponse();

                HandleResponse( response, response.GetResponseStream() );

                response.Close();
            }
            catch( WebException ex )
            {
                HandleHttpFailure( ex );
            }
        }

        protected virtual string GetHttpMethod()
        {
            return "GET";
        }

        protected virtual void FillInHttpRequest( HttpWebRequest request )
        {
        }

        protected virtual string GetRequestBody() { return null; }

        protected virtual void HandleResponse( HttpWebResponse httpResponse,
                                               Stream          responseStream )
        {
        }

        protected virtual void HandleHttpFailure( WebException exception )
        {
            var response = (HttpWebResponse)exception.Response;

            if( response != null )
            {
                // This is here only for debugging. currently message body isn't actually reported up.
                var streamReader = new StreamReader( response.GetResponseStream() );
                var responseBody = streamReader.ReadToEnd();

                throw new ApplicationException( 
                        string.Format( "Failure received from {0} web service. status={1}",
                                        _httpRequest.RequestUri.Host,
                                        Enum.GetName( typeof( HttpStatusCode ), response.StatusCode ) ),
                        exception                                                                        );
            }
            else
            {
                throw new ApplicationException(
                        string.Format( "Failure received while attempting to connect to {0}. status={1}",
                                      _httpRequest.RequestUri.Host,
                                      Enum.GetName( typeof( WebExceptionStatus ), exception.Status )      ),
                        exception                                                                            );
            }
        }

        protected static void DebugDumpResponseToFile( Stream responseStream )
        {
            using( Stream fileStream = new FileStream( "c:\\temp\\http_response.txt", FileMode.Create, FileAccess.Write ) )
            {
                byte[]  buffer = new byte[256];
                int     bytesRead;

                while( ( bytesRead = responseStream.Read( buffer, 0, 256 ) ) > 0 )
                {
                    fileStream.Write( buffer, 0, bytesRead );
                }
            }
        }

        private void AddBodyToRequest( string bodyText )
        {
            if( string.IsNullOrEmpty( bodyText ) ) return;

            byte[] body = new ASCIIEncoding().GetBytes( bodyText );

            _httpRequest.ContentLength = body.Length;

            var stream = _httpRequest.GetRequestStream();

            stream.Write( body, 0, body.Length );

            stream.Close();
        }


        private HttpWebRequest      _httpRequest;
    }
}
