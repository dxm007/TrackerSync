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
    /// <summary>
    /// Settings class for a tracker source.
    /// </summary>
    /// <remarks>
    /// This class contains properties which are common to all source types. Individual source classes
    /// are free to define a class deriving from this one if they have more properties which pertain only
    /// to that source type.
    /// 
    /// Additionally, this class support deserialization from XML through the use of Load() method.
    /// </remarks>
    public class SourceSettings
    {
        /// <summary>
        /// Creates a new instance of the SourceSettings class.
        /// </summary>
        public SourceSettings()
        {
            GetAllIncludesClosedIssues = true;
        }

        /// <summary>
        /// Creates a new instance of the SourceSettings class using another instance as the source
        /// of values.
        /// </summary>
        /// <param name="other">Existing instance to be used as the source</param>
        public SourceSettings( SourceSettings other )
        {
            this.GetAllIncludesClosedIssues = other.GetAllIncludesClosedIssues;
        }

        /// <summary>
        /// Gets/sets a flag which indicates whether or not "GetIssuesList" request should return all closed
        /// issues in addition to open ones.
        /// </summary>
        public bool GetAllIncludesClosedIssues { get; protected set; }

        /// <summary>
        /// Gets/sets a flag which indicates that the source is the primary one in generating unique IDs.
        /// In current implementation of the TrackerSync, one of the sources MUST be the primary one.
        /// </summary>
        public bool IsPrimary { get; protected set; }

        /// <summary>
        /// Initializes source settings properties by deserializes an XML document
        /// </summary>
        /// <param name="elem">XML element to be deserialized</param>
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
