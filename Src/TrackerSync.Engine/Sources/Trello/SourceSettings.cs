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
using System.Xml.Linq;


namespace TrackerSync.Sources.Trello
{
    /// <summary>
    /// Describes communications settings with Trello web services API
    /// </summary>
    public class SourceSettings : TrackerSync.Sources.SourceSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SourceSettings()
        {
            base.GetAllIncludesClosedIssues = true;
            base.IsPrimary = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">Existing instance from which to copy the property values</param>
        public SourceSettings( SourceSettings other ) : base( other )
        {
            this.UserName = other.UserName;
            this.DevKey = other.DevKey;
            this.MemberToken = other.MemberToken;
            this.BoardName = other.BoardName;
            this.OpenCardLists = other.OpenCardLists;
            this.ClosedCardLists = other.ClosedCardLists;
            this.NewCardList = other.NewCardList;
        }

        /// <summary>
        /// Gets the name of the user who own the issues board
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets developer key
        /// </summary>
        public string DevKey { get; private set; }

        /// <summary>
        /// Gets member token
        /// </summary>
        public string MemberToken { get; private set; }

        /// <summary>
        /// Gets the name of the board which contains the list of issues
        /// </summary>
        public string BoardName { get; private set; }

        /// <summary>
        /// Gets an array of names which identify lists with open issues
        /// </summary>
        public string[] OpenCardLists { get; private set; }

        /// <summary>
        /// Gets an array of names which identify lists with closed issues
        /// </summary>
        public string[] ClosedCardLists { get; private set; }

        /// <summary>
        /// Gets the name of the list into which newly added issues should be placed
        /// </summary>
        public string NewCardList { get; private set; }

        /// <summary>
        /// Gets the base URL string for the Trello API
        /// </summary>
        public string ApiServerUrl { get { return "https://api.trello.com"; } }

        /// <inheritdoc/>
        public override void Load( XElement elem )
        {
            base.Load( elem );

            foreach( var prop in elem.Elements() )
            {
                switch( prop.Name.ToString().ToLower() )
                {
                case "username":
                    this.UserName = (string)prop;
                    break;
                case "devkey":
                    this.DevKey = (string)prop;
                    break;
                case "membertoken":
                    this.MemberToken = (string)prop;
                    break;
                case "boardname":
                    this.BoardName = (string)prop;
                    break;
                case "opencardlists":
                    this.OpenCardLists = LoadStringArray( prop, "Item" );
                    break;
                case "closedcardlists":
                    this.ClosedCardLists = LoadStringArray( prop, "Item" );
                    break;
                case "newcardlist":
                    this.NewCardList = (string)prop;
                    break;
                default:
                    break;
                }
            }

            Validate();
        }

        private string[] LoadStringArray( XElement listElem, string childName )
        {
            return ( from x in listElem.Elements( childName ) select (string)x ).ToArray();
        }

        private void Validate()
        {
            if( string.IsNullOrEmpty( this.UserName ) )
            {
                throw new ApplicationException( "Missing user name" );
            }
            else if( string.IsNullOrEmpty( this.DevKey ) )
            {
                throw new ApplicationException( "Missing Trello dev key" );
            }
            else if( string.IsNullOrEmpty( this.MemberToken ) )
            {
                throw new ApplicationException( "Missing member token" );
            }
            else if( string.IsNullOrEmpty( this.BoardName ) )
            {
                throw new ApplicationException( "Missing board name" );
            }
            else if( this.OpenCardLists == null || this.OpenCardLists.Length == 0 )
            {
                throw new ApplicationException( "Missing a definition of open card lists" );
            }
            else if( this.ClosedCardLists == null || this.ClosedCardLists.Length == 0 )
            {
                throw new ApplicationException( "Missing a definition of closed card lists" );
            }
            else if( string.IsNullOrEmpty( this.NewCardList ) )
            {
                throw new ApplicationException( "Missing list for new cards" );
            }
        }
    }


    /// <summary>
    /// Extends SourceSettings class by defining additional connection properties which
    /// are needed at runtime
    /// </summary>
    class RuntimeSourceSettings : SourceSettings
    {
        public RuntimeSourceSettings( SourceSettings other ) : base( other )
        {
        }

        /// <summary>
        /// Gets/sets an array of IDs which identify lists that contain open issues
        /// </summary>
        public string[] OpenCardListIds { get; set; }

        /// <summary>
        /// Gets/sets an array of IDs which identfy lists that contain closed issues
        /// </summary>
        public string[] ClosedCardListIds { get; set; }

        /// <summary>
        /// Gets/sets an ID of a list into which newly created issues should be placed
        /// </summary>
        public string NewCardListId { get; set; }

        /// <summary>
        /// Gets/sets an ID of the board which contains the issues.
        /// </summary>
        public string BoardId { get; set; }
    }
}
