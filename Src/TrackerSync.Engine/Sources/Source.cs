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
using System.Reflection;
using System.Text;

using TrackerSync.Data;


namespace TrackerSync.Sources
{
    /// <summary>
    /// Represents a single tracker (issue, bug,  defect...) database. Referred to as
    /// "tracker source" or more simply "source"
    /// </summary>
    public interface ISource
    {
        /// <summary>
        ///  Gets a name of the source
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets settings of the source object
        /// </summary>
        SourceSettings Settings { get; }

        /// <summary>
        /// Must be called after the tracker source object is created and before it is
        /// used to communicate with the tracker database.
        /// </summary>
        void Connect();

        /// <summary>
        /// Must be called when tracker source object is no longer needed to give the source
        /// a chance to cleanup any resources
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets a list of issues from the tracker source. This list includes all open issues
        /// and depending on source settings, may also include closed issues
        /// </summary>
        /// <returns>List of issues from the source</returns>
        IEnumerable< Issue > GetIssues();

        /// <summary>
        /// Retrieves a single issue from the tracker source
        /// </summary>
        /// <param name="id">Unique identifier of the issue to return</param>
        /// <returns>Issue object retrieved from the source</returns>
        Issue GetIssue( string id );

        /// <summary>
        /// Adds a new issue to the tracker source
        /// </summary>
        /// <param name="issue">Issue to be added</param>
        void AddIssue( Issue issue );

        /// <summary>
        /// Updates existing issue in the tracker source
        /// </summary>
        /// <param name="issue">Issue to be updated</param>
        /// <param name="fieldsToUpdate">Identifies the fields which are to be updated</param>
        void UpdateIssue( Issue         issue,
                          IssueFieldId  fieldsToUpdate );

        /// <summary>
        /// Closes an issue
        /// </summary>
        /// <param name="issue">Issue to be closed</param>
        void CloseIssue( Issue issue );
    }


    /// <summary>
    /// Base for a tracker source class
    /// </summary>
    /// <remarks>
    /// It is intended that deriving concrete Source class is placed into a child namespace
    /// dedicated to that specific source type.  For example, a tracker source that communicates with
    /// Trello should be in "TrackerSync.Sources.Trello" namespace.
    /// 
    /// Although each concrete source class is free to implement ISource interface any way it sees fit, 
    /// in general it makes sense to define a separate class to handle each type request that will query
    /// or update remote tracker database. For those sources that follow this model, this class provides
    /// base implementation of ISource interface methods which use reflection to automatically find
    /// request classes in deriving concrete class namespace (i.e. "TrackerSync.Sources.Trello") which
    /// implement request interfaces defined in Requests.cs
    /// </remarks>
    public abstract class Source : ISource
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="sourceSettings">Settings object for the source</param>
        public Source( SourceSettings sourceSettings )
        {
            this.Settings = sourceSettings;
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        /// <inheritdoc/>
        /// <remarks>
        /// Base implementation of this method uses reflection to return the name of the last
        /// namespace to which deriving tracker source object belongs. For example, if a deriving
        /// class is in "TrackerSync.Sources.Trello" namespace, this property will return
        /// "Trello".
        /// </remarks>
        public virtual string Name
        {
            get
            {
                string ns = this.GetType().Namespace;

                return ns.Substring( ns.LastIndexOf( '.' ) + 1 );                        
            }
        }

        /// <inheritdoc/>
        public SourceSettings Settings { get; private set; }

        /// <inheritdoc/>
        public virtual void Connect()
        {
        }

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Issue> GetIssues()
        {
            return CreateRequest< IGetIssuesListRequest >().Execute();
        }

        /// <inheritdoc/>
        public virtual Issue GetIssue( string id )
        {
            return CreateRequest< IGetIssueRequest >().Execute( id );
        }

        /// <inheritdoc/>
        public virtual void AddIssue( Issue issue )
        {
            CreateRequest< IAddIssueRequest >().Execute( issue );
        }

        /// <inheritdoc/>
        public virtual void UpdateIssue( Issue         issue,
                                         IssueFieldId  fieldsToUpdate )
        {
            CreateRequest< IUpdateIssueRequest >().Execute( issue, fieldsToUpdate );
        }

        /// <inheritdoc/>
        public virtual void CloseIssue( Issue issue )
        {
            CreateRequest< ICloseIssueRequest >().Execute( issue );
        }

        #endregion

        #endregion

        #region ----------------------- Private Members -----------------------

        private TItf CreateRequest< TItf >()
        {
            return ( TItf )RequestHandlerFactory.GetInstance( this.GetType() ).
                                CreateRequest( typeof( TItf ), this.Settings );
        }

        #endregion
    }


    /// <summary>
    /// Internal request handler factory used by the base Source class. This factory
    /// builds a two-tier dictionary. First tier maps concrete source types and second
    /// tier maps request interface types (see request interfaces in Requests.cs) to
    /// actual classes that implement those interfaces.
    /// </summary>
    class RequestHandlerFactory
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Instantiates a new instance of a request that implements a specific request interface
        /// </summary>
        /// <param name="interfaceType">Interface which must be implemented by the newly instantiated
        /// request object</param>
        /// <param name="settings">Settings object of the source associated with the request</param>
        /// <returns>Newly created request instantce</returns>
        public object CreateRequest( Type            interfaceType,
                                     SourceSettings  settings       )
        {
            return _creatorMap[ interfaceType ].Invoke( new object[] { settings } );
        }

        /// <summary>
        /// Returns an instance of a request handler factory for a given source type
        /// </summary>
        /// <param name="sourceType">Type of source class</param>
        /// <returns>Request handler factory that instantiates requests for source
        /// of type 'sourceType'</returns>
        public static RequestHandlerFactory GetInstance( Type sourceType )
        {
            RequestHandlerFactory   factory;

            if( !_requestHandlers.TryGetValue( sourceType, out factory ) )
            {
                factory = new RequestHandlerFactory( sourceType );

                _requestHandlers.Add( sourceType, factory );
            }

            return factory;
        }

        #endregion

        #region ----------------------- Private Members -----------------------

        static RequestHandlerFactory()
        {
            _requestHandlers = new Dictionary< Type, RequestHandlerFactory >();
        }

        private RequestHandlerFactory( Type sourceType )
        {
            var types = from t2 in
                            from t in sourceType.Assembly.GetTypes()
                            where t.Namespace == sourceType.Namespace
                            select new { InterfaceType = t.GetInterfaces().FirstOrDefault( x => 
                                                Attribute.IsDefined( x, typeof( RequestInterfaceAttribute ) ) ),
                                         Constructor = t.GetConstructor( new Type[] { typeof( SourceSettings ) } ) }
                        where t2.InterfaceType != null
                        select t2;

            _creatorMap = new Dictionary<Type,ConstructorInfo>();

            foreach( var type in types )
            {
                _creatorMap.Add( type.InterfaceType, type.Constructor );
            }
        }

        private Dictionary< Type, ConstructorInfo >                 _creatorMap;

        private static Dictionary< Type, RequestHandlerFactory >    _requestHandlers;

        #endregion
    }
}
