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
    public class SourceDecoratorBase : ISource
    {
        public SourceDecoratorBase( ISource contained )
        {
            _contained = contained;
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        public virtual string Name
        {
            get { return _contained.Name; }
        }

        public SourceSettings Settings
        {
            get { return _contained.Settings; }
        }

        public virtual void Connect()
        {
            _contained.Connect();
        }

        public virtual void Disconnect()
        {
            _contained.Disconnect();
        }

        public virtual IEnumerable< Issue > GetIssues()
        {
            return _contained.GetIssues();
        }

        public virtual Issue GetIssue( string id )
        {
            return _contained.GetIssue( id );
        }

        public virtual void AddIssue( Issue issue )
        {
            _contained.AddIssue( issue );
        }

        public virtual void UpdateIssue( Issue          issue,
                                         IssueFieldId   fieldsToUpdate )
        {
            _contained.UpdateIssue( issue, fieldsToUpdate );
        }

        public virtual void CloseIssue( Issue issue )
        {
            _contained.CloseIssue( issue );
        }

        #endregion


        protected ISource Contained
        {
            get { return _contained; }
        }


        private ISource     _contained;
    }
}
