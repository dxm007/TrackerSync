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
using System.Text.RegularExpressions;

using TrackerSync.Data;


namespace TrackerSync.Sources.Trello
{
    public class SourceNormalizer : SourceDecoratorBase
    {
        #region ----------------------- Public Members ------------------------

        public SourceNormalizer( ISource contained ) : base( contained )
        {
            _NormalToContainedIdMap = new Dictionary<string,string>();
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        public override IEnumerable< Issue > GetIssues()
        {
            return from x in Contained.GetIssues()
                   select Normalize( x );
        }

        public override Issue GetIssue( string id )
        {
            return Normalize( base.GetIssue( id ) );
        }

        public override void AddIssue( Issue issue )
        {
            base.AddIssue( Denormalize( issue ) );
        }

        public override void UpdateIssue( Issue         issue,
                                          IssueFieldId  fieldsToUpdate )
        {
            base.UpdateIssue( Denormalize( issue ),
                              Denormalize( fieldsToUpdate ) );
        }

        public override void CloseIssue( Issue issue )
        {
            base.CloseIssue( Denormalize( issue ) );
        }

        #endregion

        #endregion

        #region ----------------------- Protected Members ---------------------

        #endregion

        #region ----------------------- Private Members -----------------------

        private Issue Normalize( Issue denormalizedIssue )
        {
            Issue       issue;
            Match       match = _descSplitRegExp.Match( denormalizedIssue.Description );

            issue = denormalizedIssue.Clone();
            issue.ID = ( match.Groups.Count > 1 ? match.Groups[ 1 ].Value : "" );
            issue.Description = denormalizedIssue.Description.Substring( match.Length );
            issue.OriginalIssue = denormalizedIssue;

            //_NormalToContainedIdMap[ issue.ID ] = denormalizedIssue.ID;

            return issue;
        }

        private Issue Denormalize( Issue normalizedIssue )
        {
            Issue   issue = normalizedIssue.Clone();

            issue.OriginalIssue = normalizedIssue;
            issue.Description = string.Format( "S{0}: {1}",
                                    normalizedIssue.ID, normalizedIssue.Description );

            if( normalizedIssue.OriginalIssue != null )
            {
                issue.ID = normalizedIssue.OriginalIssue.ID;
            }

            return issue;
        }

        private IssueFieldId Denormalize( IssueFieldId fields )
        {
            IssueFieldId    fieldsOut = fields;

            if( fields.HasFlag( IssueFieldId.ID ) )
            {
                fieldsOut &= ~IssueFieldId.ID;
                fieldsOut |= IssueFieldId.Desc;
            }
            
            return fieldsOut;
        }


        private Dictionary< string, string >    _NormalToContainedIdMap;

        private static readonly Regex _descSplitRegExp = new Regex( @"\AS(\d+):\s*" );

        #endregion
    }
}
