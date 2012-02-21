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


namespace TrackerSync.Data
{
    /// <summary>
    /// Indicates the state of an issue
    /// </summary>
    public enum IssueState
    {
        /// <summary>
        /// Issue is open
        /// </summary>
        Open,

        /// <summary>
        /// Issue has been closed (i.e. resolved)
        /// </summary>
        Closed
    };


    /// <summary>
    /// Flags enumeration which identifies individual fields which make up a single Issue object.
    /// </summary>
    [Flags]
    public enum IssueFieldId
    {
        /// <summary>
        /// Identifies Issue.ID field
        /// </summary>
        ID          = 0x0001,

        /// <summary>
        /// Identifies Issue.Description field
        /// </summary>
        Desc        = 0x0002,

        /// <summary>
        /// Identifies Issue.Details field
        /// </summary>
        Details     = 0x0004,

        /// <summary>
        /// Identifies Issue.State field
        /// </summary>
        State       = 0x0008
    }


    /// <summary>
    /// This is a data object which represents a single issue (bug, feature request, defect...)
    /// </summary>
    public class Issue : ICloneable
    {
        /// <summary>
        /// Unique identifier of the issue
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Short description of the issue. Often this is the title or main text which is displayed
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Detail text which more fully describes the issue and provides other information
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Current state of the issue
        /// </summary>
        public IssueState State { get; set; }

        /// <summary>
        /// If a transformation has taken place (i.e. issue was converted from source-specific to a normalized
        /// format or vice-a-versa), this property will point back at the original issue as it was before the
        /// conversion
        /// </summary>
        public Issue OriginalIssue { get; set; }

        /// <summary>
        /// Creates a copy of the current issue
        /// </summary>
        /// <returns>Newly cloned issue object</returns>
        public Issue Clone()
        {
            return (Issue)( (ICloneable)this ).Clone();
        }

        #region - - - - - - - ICloneable Interface  - - - - - - - - -

        /// <inheritdoc/>
        object ICloneable.Clone()
        {
            return new Issue() { ID = this.ID,
                                 Description = this.Description,
                                 Details = this.Details,
                                 State = this.State              };
        }

        #endregion
    }
}
