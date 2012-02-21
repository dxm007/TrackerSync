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
    /// <summary>
    /// Represents an HTTP REST request. This class is intended to serve as the base for specific source 
    /// tracker request classes.
    /// 
    /// This class implements TEMPLATE CLASS design pattern by providing SendRequest method which should be
    /// called by deriving classes when a request is being executed. A deriving class can customize the
    /// request behavior by providing one or more overrides of the virtual methods of HttpRequest 
    /// </summary>
    abstract class HttpRequest
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializes a new instance of the HttpRequest
        /// </summary>
        /// <param name="settings">Settings of the source to which the request is to be sent</param>
        public HttpRequest( SourceSettings settings )
        {
            this.SourceSettings = settings;
        }

        #endregion

        #region ----------------------- Protected Members ---------------------

        /// <summary>
        /// Gets the tracker source settings
        /// </summary>
        protected SourceSettings SourceSettings { get; private set; }

        /// <summary>
        /// To be called by a deriving class when a request is being executed. This method defines the
        /// basic workflow of an HTTP REST, which can be customized by a deriving class via virtual
        /// method overrides
        /// </summary>
        /// <param name="url">HTTP URL of the request</param>
        /// <param name="credentials">Credentials to use for the HTTP request. This parameter is optional
        /// and null can be passed in if there's no credentials</param>
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

        /// <summary>
        /// Returns HTTP method for the request. Default implementation returns "GET". Override this method
        /// if a different HTTP method is needed
        /// </summary>
        /// <returns>HTTP method to use for the request</returns>
        protected virtual string GetHttpMethod()
        {
            return "GET";
        }

        /// <summary>
        /// This method can be overridden to allow deriving class a chance to set any property/header value
        /// on the outgoing HTTP request before it is sent out. Deriving class should not use this call to 
        /// pass in message body; that is done through a separate mechanism.
        /// </summary>
        /// <param name="request">Unsent HTTP request object</param>
        protected virtual void FillInHttpRequest( HttpWebRequest request )
        {
        }

        /// <summary>
        /// This method can be overridden to allow deriving class a chance to provide a body for the request
        /// </summary>
        /// <returns>Text to be sent as the body of the request</returns>
        protected virtual string GetRequestBody() { return null; }

        /// <summary>
        /// Invoked after HTTP request is sent and response is received. A deriving class can override this
        /// method to implement custom handling of the response data that is returned from the tracker source
        /// </summary>
        /// <param name="httpResponse">HTTP response object</param>
        /// <param name="responseStream">Stream containing the body of the response</param>
        protected virtual void HandleResponse( HttpWebResponse httpResponse,
                                               Stream          responseStream )
        {
        }

        /// <summary>
        /// Invoked if a failure condition is encountered while sending/receiving the HTTP request. The default
        /// implementation throws an ApplicationException indicating the failure. Deriving classes can override this
        /// method if any other behavior is desired instead.
        /// </summary>
        /// <param name="exception">Exception object thrown while sending/receiving HTTP request</param>
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

        /// <summary>
        /// This method is used only for debugging to dump HTTP response to a file. Note, when this method is called
        /// it advances the position of the response stream.
        /// </summary>
        /// <param name="responseStream">Stream containing HTTP response body</param>
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

        #endregion

        #region ----------------------- Private Members -----------------------

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

        #endregion
    }
}
