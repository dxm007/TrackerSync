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

namespace TrackerSync.Engine
{
    public abstract class SyncAction
    {
        public abstract void Run();

        protected SyncAction( Sources.ISource    source,
                              Data.Issue         issue   )
        {
            this.Source = source;
            this.Issue = issue;
        }

        protected Sources.ISource Source { get; private set; }

        protected Data.Issue Issue { get; private set; }
    }


    public class CloseIssueSyncAction : SyncAction
    {
        public CloseIssueSyncAction( Sources.ISource    source,
                                     Data.Issue         issue   ) : base( source, issue ) {}

        public override void Run()
        {   
            this.Source.CloseIssue( this.Issue );
        }
    }


    public class AddIssueAction : SyncAction
    {
        public AddIssueAction( Sources.ISource  source,
                               Data.Issue       issue   ) : base( source, issue ) {}

        public override void Run()
        {
            this.Source.AddIssue( this.Issue );
        }
    }

    public class UpdateIssueAction : SyncAction
    {
        public UpdateIssueAction( Sources.ISource       source,
                                  Data.Issue            issue,
                                  Data.IssueFieldId     fieldsToUpdate )
                : base( source, issue )
        {
            _fieldsToUpdate = fieldsToUpdate;
        }

        public override void Run()
        {
            this.Source.UpdateIssue( this.Issue, _fieldsToUpdate );
        }


        private Data.IssueFieldId   _fieldsToUpdate;
    }
}
