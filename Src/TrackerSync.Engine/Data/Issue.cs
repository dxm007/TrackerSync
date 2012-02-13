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
    public enum IssueState { Open, Closed };


    [Flags]
    public enum IssueFieldId
    {
        ID          = 0x0001,
        Desc        = 0x0002,
        Details     = 0x0004,
        State       = 0x0008
    }


    public class Issue : ICloneable
    {
        public string ID { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        public IssueState State { get; set; }

        public Issue OriginalIssue { get; set; }

        public Issue Clone()
        {
            return (Issue)( (ICloneable)this ).Clone();
        }

        #region - - - - - - - ICloneable Interface  - - - - - - - - -

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
