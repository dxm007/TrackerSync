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
    public class RequestInterfaceAttribute : System.Attribute
    {
    }


    [RequestInterface]
    interface IGetIssuesListRequest
    {
        IEnumerable< Data.Issue > Execute();
    }

    [RequestInterface]
    interface IGetIssueRequest
    {
        Data.Issue Execute( string issueId );
    }

    [RequestInterface]
    interface IAddIssueRequest
    {
        void Execute( Data.Issue issue );
    }

    [RequestInterface]
    interface ICloseIssueRequest
    {
        void Execute( Data.Issue issue );
    }

    [RequestInterface]
    interface IUpdateIssueRequest
    {
        void Execute( Data.Issue            issue,
                      Data.IssueFieldId     fieldsToUpdate );
    }
}
