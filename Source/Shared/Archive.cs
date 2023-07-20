/********************************************************************\
*                                                                   *
*  Archive class by Pascal vd Heiden, www.codeimp.com               *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This class provides functionality to read files from an
// archive (Directory or RAR file).

using System.Collections;
using System.Runtime.InteropServices;

namespace CodeImp
{
	public sealed class Archive
	{
		#region ================== Constants

		private const int ERAR_END_ARCHIVE = 10;
		private const int ERAR_NO_MEMORY = 11;
		private const int ERAR_BAD_DATA = 12;
		private const int ERAR_BAD_ARCHIVE = 13;
		private const int ERAR_UNKNOWN_FORMAT = 14;
		private const int ERAR_EOPEN = 15;
		private const int ERAR_ECREATE = 16;
		private const int ERAR_ECLOSE = 17;
		private const int ERAR_EREAD = 18;
		private const int ERAR_EWRITE = 19;
		private const int ERAR_SMALL_BUF = 20;
		private const int ERAR_UNKNOWN = 21;

		private const int RAR_OM_LIST = 0;
		private const int RAR_OM_EXTRACT = 1;

		private const int RAR_SKIP = 0;
		private const int RAR_TEST = 1;
		private const int RAR_EXTRACT = 2;

		#endregion

		#region ================== Structures

		// RAR Archive Open information
		private struct RarArchiveOpen
		{
			[MarshalAs(UnmanagedType.LPStr)] public string filename;
			[MarshalAs(UnmanagedType.LPWStr)] public string filenamew;
			public uint openmode;
			public uint openresult;
			public IntPtr commentbuffer;
			public uint commentbuffersize;
			public uint commentsize;
			public uint commentstate;
			public uint flags;
			public uint[] reserved;
		}

		// RAR file header information
		private struct RarFileHeader
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)] public string archivename;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)] public string filename;
			public uint flags;
			public uint packedsize;
			public uint unpackedsize;
			public uint hostos;
			public uint filecrc;
			public uint filetime;
			public uint unpackedversion;
			public uint method;
			public uint fileattr;
			public IntPtr commentbuffer;
			public uint commentbuffersize;
			public uint commentsize;
			public uint commentstated;
		}

		#endregion

		#region ================== Library Links

		// #if LINUX // TODO: Just support unrar on Linux
  //
		// 	private static IntPtr RAROpenArchiveEx(ref RarArchiveOpen archiveinfo)
		// 	{
		// 		return (IntPtr)0;
		// 	}
  //
		// 	private static int RARCloseArchive(IntPtr archivehandle)
		// 	{
		// 		return 0;
		// 	}
  //
		// 	private static int RARReadHeader(IntPtr archivehandle, ref RarFileHeader headerinfo)
		// 	{
		// 		return 0;
		// 	}
  //
		// 	private static int RARProcessFileW(IntPtr archivehandle, int operation,
  //                                            string destpath,
  //                                            string destname)
		// 	{
		// 		return 0;
		// 	}
  //
		// 	private static void RARSetPassword(IntPtr archivehandle, string password)
		// 	{
		// 	}
		// #else

			[DllImport("libunrar.dll")]
			static extern IntPtr RAROpenArchiveEx(ref RarArchiveOpen archiveinfo);

			[DllImport("libunrar.dll")]
			static extern int RARCloseArchive(IntPtr archivehandle);

			[DllImport("libunrar.dll")]
			static extern int RARReadHeader(IntPtr archivehandle,
                                            ref RarFileHeader headerinfo);

			[DllImport("libunrar.dll")]
			static extern int RARProcessFileW(IntPtr archivehandle, int operation,
                                             [MarshalAs(UnmanagedType.LPWStr)] string destpath,
                                             [MarshalAs(UnmanagedType.LPWStr)] string destname);

			[DllImport("libunrar.dll")]
			static extern void RARSetPassword(IntPtr archivehandle, string password);

		#endregion

		#region ================== Variables

		// This indicates if the source are the contents of a directory
		private bool isdirectory = false;

		// This is the full path and name of the directory or rar file
		private string archivename;

		// This is the title of the open archive
		private string archivetitle;

		// The list of file names (no paths) and crcs
		private string[] archivefiles;
		private uint[] archivefilecrcs;

		// This indicates if the archive was disposed
		private bool isdisposed = false;

		#endregion

		#region ================== Properties

		// Title of this archive
		public string Title { get { return archivetitle; } }

		// File or directory of this archive
		public string ArchiveFile { get { return archivename; } }

		// This indicates if the object was disposed
		public bool Disposed { get { return isdisposed; } }

		// This returns the list of files
		public string[] FileNames { get { return archivefiles; } }

		// This returns the number of files
		public int FileCount { get { return archivefiles.Length; } }

		#endregion

		#region ================== Constructor / Destructor

		// This opens an archive (directory or rar)
		// using the default unzipdirectory
		public Archive(string pathname)
		{
			// Open archive
			this.Open(pathname);
		}

		// Destructor
		~Archive()
		{
			// Dispose if not already disposed
			if(!isdisposed) this.Dispose();
		}

		#endregion

		#region ================== Methods

		// This opens an archive (directory or rar)
		private void Open(string pathname)
		{
			int result;
			ArrayList files = new ArrayList();
			ArrayList crcs = new ArrayList();

			// Set the archive pathname and title
			archivename = pathname;
			archivetitle = Path.GetFileName(pathname);

			// Check if the archive exists
			if(File.Exists(pathname))
			{
				// Source is an archive
				isdirectory = false;

				// Create archive open structure
				RarArchiveOpen openinfo = new RarArchiveOpen();
				openinfo.filename = pathname;
				openinfo.filenamew = pathname;
				openinfo.commentbuffer = Marshal.AllocHGlobal(2);
				openinfo.commentbuffersize = 2;
				openinfo.openmode = RAR_OM_LIST;
				openinfo.reserved = new uint[32];

				// Open the archive file
				IntPtr archive = RAROpenArchiveEx(ref openinfo);
				RARSetPassword(archive, "");

				// Check for errors
				if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to initialize data structures."));
				if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("Archive header broken."));
				if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("File is not valid RAR archive."));
				if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("File open error."));

				// Create header structure
				RarFileHeader header = new RarFileHeader();
				header.commentbuffer = Marshal.AllocHGlobal(2);
				header.commentbuffersize = 2;

				// Get the first header
				result = RARReadHeader(archive, ref header);

				// Continue until end of archive
				while(result != ERAR_END_ARCHIVE)
				{
					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File header in archive is broken."));

					// Keep the filename
					files.Add(header.filename);
					crcs.Add(header.filecrc);

					// Move on to the next file
					result = RARProcessFileW(archive, RAR_SKIP, "", "");

					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File data in archive is broken."));
					if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("Volume is not a valid archive."));
					if(openinfo.openresult == ERAR_UNKNOWN_FORMAT) throw(new Exception("Unknown archive format."));
					if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("Error opening the volume."));
					if(openinfo.openresult == ERAR_ECREATE) throw(new Exception("Unable to write the file."));
					if(openinfo.openresult == ERAR_ECLOSE) throw(new Exception("Error closing the file."));
					if(openinfo.openresult == ERAR_EREAD) throw(new Exception("Error while reading."));
					if(openinfo.openresult == ERAR_EWRITE) throw(new Exception("Error while writing."));

					// Get the next header
					result = RARReadHeader(archive, ref header);
				}

				// Make standard array from files
				archivefiles = (string[])files.ToArray(typeof(string));
				archivefilecrcs = (uint[])crcs.ToArray(typeof(uint));

				// Close archive
				RARCloseArchive(archive);

				// Clean up allocated memory
				Marshal.FreeHGlobal(openinfo.commentbuffer);
				Marshal.FreeHGlobal(header.commentbuffer);
			}
			// Check if a directory exists
			else if(Directory.Exists(pathname))
			{
				// Source is a directory
				isdirectory = true;

				// List all directory contents
				archivefiles = Directory.GetFiles(archivename);
				archivefilecrcs = new uint[archivefiles.Length];

				// Go for all files
				for(int i = 0; i < archivefiles.Length; i++)
				{
					// Keep filename only
					archivefiles[i] = Path.GetFileName(archivefiles[i]);

					// No CRC
					archivefilecrcs[i] = 0;
				}
			}
			// Nothing found at pathname
			else
			{
				// Throw error: File not found
				throw new FileNotFoundException("Cannot find the specified archive or directory '" + pathname + "'.", pathname);
			}
		}

		// This closes an open archive
		public void Dispose()
		{
			// No more files
			archivefiles = null;

			// Object is now disposed
			isdisposed = true;
		}

		// This returns the CRC for a file in the archive
		public uint GetFileCRC(string filename)
		{
			// Find the file
			for(int i = 0; i < archivefiles.Length; i++)
			{
				// Is this the file we're looking for?
				if(string.Compare(archivefiles[i], filename, true) == 0)
				{
					// Return the CRC
					return archivefilecrcs[i];
				}
			}

			// No such file
			return 0;
		}

		// This tests if a file title exists
		public bool FileExists(string filename)
		{
			// Go for all files and return true if this file exists
			foreach(string f in archivefiles) if(string.Compare(f, filename, true) == 0) return true;

			// Nothing with this name found, return false
			return false;
		}

		// This gets the full path and filename for a file by title
		public string FilePathName(string filename)
		{
			// Go for all files to find the specified file
			foreach(string f in archivefiles)
			{
				// If this the specified file?
				if(f == filename)
				{
					// Check if this archive is a directory
					if(isdirectory)
					{
						// Get the full filepathname
						return Path.Combine(archivename, f);
					}
					else
					{
						// Combine the archive filename and file
						return Path.GetFullPath(archivename) + "/" + f;
					}
				}
			}

			// Nothing found
			return "?";
		}

		// This extracts a single file and returns the full filepathname
		public string ExtractFile(string filename, string targetpath) { return this.ExtractFile(filename, targetpath, true); }
		public string ExtractFile(string filename, string targetpath, bool overwrite)
		{
			int result;

			// Determine target filepathname
			string targetfile = Path.Combine(targetpath, filename);

			// Does the target already exist?
			if(File.Exists(targetfile))
			{
				// Should we overwrite the file?
				if(overwrite)
				{
					// Remove the old file
					File.Delete(targetfile);
				}
				else
				{
					// Return the existing filepathname
					return targetfile;
				}
			}

			// Check if archive is a directory
			if(isdirectory)
			{
				// Determine source filepathname
				string sourcefile = Path.Combine(archivename, filename);

				// Check if the source file exists
				if(File.Exists(sourcefile))
				{
					// Copy the file to the target location
					//File.Copy(sourcefile, targetfile, true);

					// Return the original file
					return sourcefile;
				}
				else
				{
					// File not found
					throw(new FileNotFoundException("Cannot find the file '" + filename + "' in archive '" + archivetitle + "'."));
				}
			}
			else
			{
				// Create archive open structure
				RarArchiveOpen openinfo = new RarArchiveOpen();
				openinfo.filename = archivename;
				openinfo.filenamew = archivename;
				openinfo.commentbuffer = Marshal.AllocHGlobal(2);
				openinfo.commentbuffersize = 2;
				openinfo.openmode = RAR_OM_EXTRACT;
				openinfo.reserved = new uint[32];

				// Open the archive file
				IntPtr archive = RAROpenArchiveEx(ref openinfo);
				RARSetPassword(archive, "");

				// Check for errors
				if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to initialize data structures."));
				if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("Archive header broken."));
				if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("File is not valid RAR archive."));
				if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("File open error."));

				// Create header structure
				RarFileHeader header = new RarFileHeader();
				header.commentbuffer = Marshal.AllocHGlobal(2);
				header.commentbuffersize = 2;

				// Get the first header
				result = RARReadHeader(archive, ref header);

				// Continue until end of archive
				while(result != ERAR_END_ARCHIVE)
				{
					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File header in archive is broken."));

					// Check if this is the file to be extracted
					if(filename == header.filename)
					{
						// Extract the file
						result = RARProcessFileW(archive, RAR_EXTRACT, "", targetfile);
					}
					else
					{
						// Move on to the next file
						result = RARProcessFileW(archive, RAR_SKIP, "", "");
					}

					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File data in archive is broken."));
					if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("Volume is not a valid archive."));
					if(openinfo.openresult == ERAR_UNKNOWN_FORMAT) throw(new Exception("Unknown archive format."));
					if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("Error opening the volume."));
					if(openinfo.openresult == ERAR_ECREATE) throw(new Exception("Unable to write the file."));
					if(openinfo.openresult == ERAR_ECLOSE) throw(new Exception("Error closing the file."));
					if(openinfo.openresult == ERAR_EREAD) throw(new Exception("Error while reading."));
					if(openinfo.openresult == ERAR_EWRITE) throw(new Exception("Error while writing."));

					// Was this file extracted? then leave the search loop
					if(filename == header.filename) break;

					// Get the next header
					result = RARReadHeader(archive, ref header);
				}

				// Close archive
				RARCloseArchive(archive);

				// Clean up allocated memory
				Marshal.FreeHGlobal(openinfo.commentbuffer);
				Marshal.FreeHGlobal(header.commentbuffer);

				// Was the file not extracted?
				if(filename != header.filename)
				{
					// File not found
					throw(new FileNotFoundException("Cannot find the file '" + filename + "' in archive '" + archivetitle + "'."));
				}
			}

			// Return the target filepathname
			return targetfile;
		}

		// This extracts all files
		public void ExtractAllFiles(string targetpath)
		{
			int result;

			// Check if archive is a directory
			if(isdirectory)
			{
				// Go for all filenames
				foreach(string f in archivefiles)
				{
					// Determine source and target filepathname
					string sourcefile = Path.Combine(archivename, f);
					string targetfile = Path.Combine(targetpath, f);

					// Check if the source file exists
					if(File.Exists(sourcefile))
					{
						// Copy the file to the target location
						File.Copy(sourcefile, targetfile, true);
					}
					else
					{
						// File not found
						throw(new FileNotFoundException("Cannot find the file '" + f + "' in archive '" + archivetitle + "'."));
					}
				}
			}
			else
			{
				// Create archive open structure
				RarArchiveOpen openinfo = new RarArchiveOpen();
				openinfo.filename = archivename;
				openinfo.filenamew = archivename;
				openinfo.commentbuffer = Marshal.AllocHGlobal(2);
				openinfo.commentbuffersize = 2;
				openinfo.openmode = RAR_OM_EXTRACT;
				openinfo.reserved = new uint[32];

				// Open the archive file
				IntPtr archive = RAROpenArchiveEx(ref openinfo);
				RARSetPassword(archive, "");

				// Check for errors
				if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to initialize data structures."));
				if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("Archive header broken."));
				if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("File is not valid RAR archive."));
				if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("File open error."));

				// Create header structure
				RarFileHeader header = new RarFileHeader();
				header.commentbuffer = Marshal.AllocHGlobal(2);
				header.commentbuffersize = 2;

				// Get the first header
				result = RARReadHeader(archive, ref header);

				// Continue until end of archive
				while(result != ERAR_END_ARCHIVE)
				{
					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File header in archive is broken."));

					// Determine target filepathname
					string targetfile = Path.Combine(targetpath, header.filename);

					// Extract the file
					result = RARProcessFileW(archive, RAR_EXTRACT, "", targetfile);

					// Check for errors
					if(openinfo.openresult == ERAR_NO_MEMORY) throw(new OutOfMemoryException("Not enough memory to extract the data."));
					if(openinfo.openresult == ERAR_BAD_DATA) throw(new Exception("File data in archive is broken."));
					if(openinfo.openresult == ERAR_BAD_ARCHIVE) throw(new Exception("Volume is not a valid archive."));
					if(openinfo.openresult == ERAR_UNKNOWN_FORMAT) throw(new Exception("Unknown archive format."));
					if(openinfo.openresult == ERAR_EOPEN) throw(new Exception("Error opening the volume."));
					if(openinfo.openresult == ERAR_ECREATE) throw(new Exception("Unable to write the file."));
					if(openinfo.openresult == ERAR_ECLOSE) throw(new Exception("Error closing the file."));
					if(openinfo.openresult == ERAR_EREAD) throw(new Exception("Error while reading."));
					if(openinfo.openresult == ERAR_EWRITE) throw(new Exception("Error while writing."));

					// Get the next header
					result = RARReadHeader(archive, ref header);
				}

				// Close archive
				RARCloseArchive(archive);

				// Clean up allocated memory
				Marshal.FreeHGlobal(openinfo.commentbuffer);
				Marshal.FreeHGlobal(header.commentbuffer);
			}
		}

		#endregion
	}
}
