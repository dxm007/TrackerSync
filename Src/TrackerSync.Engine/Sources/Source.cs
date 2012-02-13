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
    public interface ISource
    {
        string Name { get; }

        SourceSettings Settings { get; }

        void Connect();

        void Disconnect();

        IEnumerable< Issue > GetIssues();

        Issue GetIssue( string id );

        void AddIssue( Issue issue );

        void UpdateIssue( Issue         issue,
                          IssueFieldId  fieldsToUpdate );

        void CloseIssue( Issue issue );
    }


    public abstract class Source : ISource
    {
        public Source( SourceSettings sourceSettings )
        {
            this.Settings = sourceSettings;
        }

        public virtual string Name
        {
            get
            {
                string ns = this.GetType().Namespace;

                return ns.Substring( ns.LastIndexOf( '.' ) + 1 );                        
            }
        }

        public SourceSettings Settings { get; private set; }

        public virtual void Connect()
        {
        }

        public virtual void Disconnect()
        {
        }

        public virtual IEnumerable<Issue> GetIssues()
        {
            return CreateRequest< IGetIssuesListRequest >().Execute();
        }

        public virtual Issue GetIssue( string id )
        {
            return CreateRequest< IGetIssueRequest >().Execute( id );
        }

        public virtual void AddIssue( Issue issue )
        {
            CreateRequest< IAddIssueRequest >().Execute( issue );
        }

        public virtual void UpdateIssue( Issue         issue,
                                         IssueFieldId  fieldsToUpdate )
        {
            CreateRequest< IUpdateIssueRequest >().Execute( issue, fieldsToUpdate );
        }

        public virtual void CloseIssue( Issue issue )
        {
            CreateRequest< ICloseIssueRequest >().Execute( issue );
        }

        private TItf CreateRequest< TItf >()
        {
            return ( TItf )RequestHandlerFactory.GetInstance( this.GetType() ).
                                CreateRequest( typeof( TItf ), this.Settings );
        }
    }


    class RequestHandlerFactory
    {
        static RequestHandlerFactory()
        {
            _requestHandlers = new Dictionary< Type, RequestHandlerFactory >();
        }

        public object CreateRequest( Type            interfaceType,
                                     SourceSettings  settings       )
        {
            return _creatorMap[ interfaceType ].Invoke( new object[] { settings } );
        }

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
    }
}
