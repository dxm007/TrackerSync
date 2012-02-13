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
    public class ReadOnlySourceDecorator : SourceDecoratorBase
    {
        public ReadOnlySourceDecorator( ISource contained ) : base( contained )
        {
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        public override void AddIssue( Issue issue )
        {
        }

        public override void UpdateIssue( Issue          issue,
                                         IssueFieldId   fieldsToUpdate )
        {
        }

        public override void CloseIssue( Issue issue )
        {
        }

        #endregion

    }
}
