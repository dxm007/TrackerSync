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

using TrackerSync.Data;


namespace TrackerSync.Sources
{
    /// <summary>
    /// Tracker source decorator which can be used to make a tracker source object read-only by
    /// overriding method calls that would write data out to simply return.  This decorator is
    /// used when synchonization engine is run in "/status" mode to show the user what will be
    /// updated without performing the actual updates.
    /// </summary>
    public class ReadOnlySourceDecorator : SourceDecorator
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="contained">Tracker source object which is to be read-only</param>
        public ReadOnlySourceDecorator( ISource contained ) : base( contained )
        {
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        /// <inheritdoc/>
        public override void AddIssue( Issue issue )
        {
        }

        /// <inheritdoc/>
        public override void UpdateIssue( Issue          issue,
                                         IssueFieldId   fieldsToUpdate )
        {
        }

        /// <inheritdoc/>
        public override void CloseIssue( Issue issue )
        {
        }

        #endregion

    }
}
