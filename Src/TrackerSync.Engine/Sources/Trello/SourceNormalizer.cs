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
    /// <summary>
    /// Trello source decorator responsible for normalizing issues retrieved from/sent to
    /// Trello into a standard format used by the rest of the synchronizer application. 
    /// </summary>
    /// <remarks>
    /// Primary responsibility of this class is to convert standard (ID, Description) issue
    /// properties into "Description" field which contains "[ID]: [Description]". This is 
    /// necessary because Trello uses a different ID system (GUIDs of some sort) which we
    /// do not want to work with outside of their API.
    /// </remarks>
    public class SourceNormalizer : SourceDecorator
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="contained">Trello tracker source to be normalized</param>
        public SourceNormalizer( ISource contained ) : base( contained )
        {
            _NormalToContainedIdMap = new Dictionary<string,string>();
        }

        #region - - - - - - - ISource Interface - - - - - - - - - - -

        /// <inheritdoc/>
        public override IEnumerable< Issue > GetIssues()
        {
            return from x in Contained.GetIssues()
                   select Normalize( x );
        }

        /// <inheritdoc/>
        public override Issue GetIssue( string id )
        {
            // When we ask for all issues from Trello (via "GetIssuesList" request), we get the entire
            // list, including closed issues. If we didn't get the issue before, "GetIssue" request
            // won't return anything either, so it is safe to simply return 'null'.
            return null;
        }

        /// <inheritdoc/>
        public override void AddIssue( Issue issue )
        {
            base.AddIssue( Denormalize( issue ) );
        }

        /// <inheritdoc/>
        public override void UpdateIssue( Issue         issue,
                                          IssueFieldId  fieldsToUpdate )
        {
            base.UpdateIssue( Denormalize( issue ),
                              Denormalize( fieldsToUpdate ) );
        }

        /// <inheritdoc/>
        public override void CloseIssue( Issue issue )
        {
            base.CloseIssue( Denormalize( issue ) );
        }

        #endregion

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
        private static readonly Regex           _descSplitRegExp = new Regex( @"\AS(\d+):\s*" );

        #endregion
    }
}
