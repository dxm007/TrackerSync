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
    public class SourceSettings : TrackerSync.Sources.SourceSettings
    {
        public SourceSettings()
        {
            base.GetAllIncludesClosedIssues = true;
            base.IsPrimary = false;
        }

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

        public string UserName { get; private set; }

        public string DevKey { get; private set; }

        public string MemberToken { get; private set; }

        public string BoardName { get; private set; }

        public string[] OpenCardLists { get; private set; }

        public string[] ClosedCardLists { get; private set; }

        public string NewCardList { get; private set; }

        public string ApiServerUrl { get { return "https://api.trello.com"; } }

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


    class RuntimeSourceSettings : SourceSettings
    {
        public RuntimeSourceSettings()
        {
            BoardId = "4f1bcc8e2d46bda3240698b9";
        }

        public RuntimeSourceSettings( SourceSettings other ) : base( other )
        {
            // TEMP
        }

        public string[] OpenCardListIds { get; set; }

        public string[] ClosedCardListIds { get; set; }

        public string NewCardListId { get; set; }

        public string BoardId { get; set; }
    }
}
