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
    /// Base class for source wrappers that implement DECORATOR design pattern
    /// </summary>
    /// <remarks>
    /// This class implements "identity" decorator behavior (i.e. a decorator which does not alter
    /// any behavior). It allows deriving classes to override only those methods/behaviors that it
    /// wishes to customize/alter while leaving it to the base class to take care of all other ones.
    /// </remarks>
    public class SourceDecorator : ISource
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="contained">Source object whose behavior is to be altered</param>
        public SourceDecorator( ISource contained )
        {
            _contained = contained;
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -
        
        /// <inheritdoc/>
        public virtual string Name
        {
            get { return _contained.Name; }
        }

        /// <inheritdoc/>
        public SourceSettings Settings
        {
            get { return _contained.Settings; }
        }

        /// <inheritdoc/>
        public virtual void Connect()
        {
            _contained.Connect();
        }

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
            _contained.Disconnect();
        }

        /// <inheritdoc/>
        public virtual IEnumerable< Issue > GetIssues()
        {
            return _contained.GetIssues();
        }

        /// <inheritdoc/>
        public virtual Issue GetIssue( string id )
        {
            return _contained.GetIssue( id );
        }

        /// <inheritdoc/>
        public virtual void AddIssue( Issue issue )
        {
            _contained.AddIssue( issue );
        }

        /// <inheritdoc/>
        public virtual void UpdateIssue( Issue          issue,
                                         IssueFieldId   fieldsToUpdate )
        {
            _contained.UpdateIssue( issue, fieldsToUpdate );
        }

        /// <inheritdoc/>
        public virtual void CloseIssue( Issue issue )
        {
            _contained.CloseIssue( issue );
        }

        #endregion

        /// <summary>
        /// Gets an inner instance of the source being decorated
        /// </summary>
        protected ISource Contained
        {
            get { return _contained; }
        }

        private ISource     _contained;
    }
}
