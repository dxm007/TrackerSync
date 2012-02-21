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

namespace TrackerSync.Sources
{
    /// <summary>
    /// Attribute which is assigned to source tracker request interfaces. Using this attribute
    /// makes it much easier to use reflection to enumerate request classes for a specific
    /// tracker source
    /// </summary>
    public class RequestInterfaceAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Represents "GetIssuesList" request
    /// </summary>
    [RequestInterface]
    interface IGetIssuesListRequest
    {
        /// <summary>
        /// Executes the tracker source request
        /// </summary>
        /// <returns>A list of issues. This includes all open issues. It may also include all
        /// closed issues if tracker source configuration indicates that closed issues should
        /// be returned by this request</returns>
        IEnumerable< Data.Issue > Execute();
    }

    /// <summary>
    /// Represents "GetIssue" request
    /// </summary>
    [RequestInterface]
    interface IGetIssueRequest
    {
        /// <summary>
        /// Executes the tracker source request
        /// </summary>
        /// <param name="issueId">Unique issue identifier</param>
        /// <returns>Specific issue from the tracker source</returns>
        Data.Issue Execute( string issueId );
    }

    /// <summary>
    /// Represents "AddIssue" request
    /// </summary>
    [RequestInterface]
    interface IAddIssueRequest
    {
        /// <summary>
        /// Executes the tracker source request
        /// </summary>
        /// <param name="issue">Issue to be added</param>
        void Execute( Data.Issue issue );
    }

    /// <summary>
    /// Represents "CloseIssue" request
    /// </summary>
    [RequestInterface]
    interface ICloseIssueRequest
    {
        /// <summary>
        /// Executes the tracker source request
        /// </summary>
        /// <param name="issue">Issue to be closed</param>
        void Execute( Data.Issue issue );
    }

    /// <summary>
    /// Represents "UpdateIssue" request
    /// </summary>
    [RequestInterface]
    interface IUpdateIssueRequest
    {
        /// <summary>
        /// Executes the tracker source request
        /// </summary>
        /// <param name="issue">Issue to be updated</param>
        /// <param name="fieldsToUpdate">Flags enumeration that indicates which fields are to
        /// be updated</param>
        void Execute( Data.Issue            issue,
                      Data.IssueFieldId     fieldsToUpdate );
    }
}
