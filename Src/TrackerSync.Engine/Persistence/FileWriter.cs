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
using System.IO;
using System.Linq;
using System.Text;

using TrackerSync.Data;


namespace TrackerSync.Persistence
{
    /// <summary>
    /// This class is used mainly for debugging to dump certain TrackerSync information to a file
    /// </summary>
    class FileWriter : IDisposable
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializes a new instance of the FileWriter to write to the specific file
        /// </summary>
        /// <param name="filePath">Path of a file to write to. If a file already exists, it will be overwritten</param>
        public FileWriter( string filePath )
        {
            FileStream stream = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.Read );

            _writer = new StreamWriter( stream );
        }

        /// <summary>
        /// Writes out a list of issues
        /// </summary>
        /// <param name="issues">List of issues to write out</param>
        public void Write( IEnumerable< Issue > issues )
        {
            foreach( var x in issues )
            {
                _writer.WriteLine( "--------------------------------------------------------------------" );

                Write( x );

                _writer.WriteLine();
            }
        }

        /// <summary>
        /// Writes out a single issue
        /// </summary>
        /// <param name="issue">Issue to be written out</param>
        public void Write( Issue issue )
        {
            _writer.WriteLine( "{0}[{2}]: {1}", issue.ID, issue.Description,
                               issue.State == IssueState.Open ? "O" : "C" );
            _writer.WriteLine( "{0}", issue.Details );
        }

        #region - - - - - - - IDisposable Interface - - - - - - - - -

        /// <inheritdoc/>
        public void Dispose()
        {
            if( _writer != null )
            {
 	            _writer.Dispose();
                _writer = null;
            }
        }

        #endregion

        #endregion

        #region ----------------------- Private Members -----------------------

        private StreamWriter    _writer;

        #endregion
}
}
