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

namespace TrackerSync.Sources
{
    public class SourceSettings
    {
        public SourceSettings()
        {
            GetAllIncludesClosedIssues = true;
        }

        public SourceSettings( SourceSettings other )
        {
            this.GetAllIncludesClosedIssues = other.GetAllIncludesClosedIssues;
        }

        public bool GetAllIncludesClosedIssues { get; protected set; }

        public bool IsPrimary { get; protected set; }

        public virtual void Load( XElement elem )
        {
            foreach( var prop in elem.Elements() )
            {
                switch( prop.Name.ToString().ToLower() )
                {
                case "getallincludesclosedissues":
                    this.GetAllIncludesClosedIssues = (bool)prop;
                    break;
                default:
                    break;
                }
            }
        }
    }
}
