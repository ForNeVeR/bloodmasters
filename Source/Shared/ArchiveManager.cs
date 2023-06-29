/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;

namespace CodeImp.Bloodmasters
{
	public class ArchiveManager
	{
		#region ================== Variables

		// All archives
		private static Hashtable archives = new Hashtable();

		// Temporary path
		private static string temppath;

		#endregion

		#region ================== Methods

		// This returns the temp path for a specific archive
		public static string GetArchiveTempPath(Archive archive)
		{
			return Path.Combine(temppath, archive.Title);
		}

		// This finds all files of a specific type in all archives
		public static ArrayList FindAllFiles(string filetype)
		{
			ArrayList result = new ArrayList();

			// Go for all archives
			foreach(DictionaryEntry de in archives)
			{
				// Get filename and archive
				string archivename = (string)de.Key;
				Archive a = (Archive)de.Value;

				// Go for all files in archive
				foreach(string f in a.FileNames)
				{
					// Filename matches?
					if(f.ToLower().EndsWith(filetype.ToLower()))
					{
						// Add to result list
						result.Add(archivename + "/" + f);
					}
				}
			}

			// Return result
			return result;
		}

		// This finds an archive that has the specified file
		public static string FindFileArchive(string filename)
		{
			// Go for all archives
			foreach(DictionaryEntry de in archives)
			{
				// Get filename and archive
				string archivename = (string)de.Key;
				Archive a = (Archive)de.Value;

				// File in this archive?
				if(a.FileExists(filename))
				{
					// Return the name of this archive
					return archivename;
				}
			}

			// No archive found that has this file
			return "";
		}

		// This returns an archive by filename
		public static Archive GetArchive(string archivename)
		{
			// Check if archive exists
			if(archives.Contains(archivename))
			{
				// Return archive
				return (Archive)archives[archivename];
			}
			else
			{
				// No such archive
				throw(new Exception("No archive '" + archivename + "' loaded."));
			}
		}

		// This returns the archive for a specific file
		// The filepathname must be like this:  textures.rar/grass.bmp
		public static Archive GetFileArchive(string filepathname)
		{
			// Split the filepathname
			string[] files = filepathname.Split('/');

			// Make the archive name
			string archivename = files[0].ToLower();

			// Check if archive exists
			if(archives.Contains(archivename))
			{
				// Return archive
				return (Archive)archives[archivename];
			}
			else
			{
				// No such archive
				throw(new Exception("No archive '" + archivename + "' loaded while looking for '" + filepathname + "'."));
			}
		}

		// This tests if a file can be found
		public static bool FilePathNameExists(string filepathname)
		{
			// Split the filepathname
			string[] files = filepathname.Split('/');

			// Make the archive name
			string archivename = files[0].ToLower();

			// Check if archive exists
			if(archives.Contains(archivename))
			{
				// Check if archive has the specified file
				Archive a = (Archive)archives[archivename];
				return a.FileExists(files[1]);
			}
			else
			{
				// Archive not found
				return false;
			}
		}

		// This returns the CRC for a given file
		public static uint GetFileCRC(string filepathname)
		{
			// Get the archive
			Archive a = GetFileArchive(filepathname);

			// Split the filepathname
			string[] files = filepathname.Split('/');

			// Return the file CRC
			return a.GetFileCRC(files[1]);
		}

		// This extracts the file from its archive and returns
		// the full path and filename to the temporary file
		public static string ExtractFile(string filepathname) { return ExtractFile(filepathname, false); }
		public static string ExtractFile(string filepathname, bool overwrite)
		{
			// Get the archive
			Archive a = GetFileArchive(filepathname);

			// Split the filepathname
			string[] files = filepathname.Split('/');

			// Extract file
			string tempdir = Path.Combine(temppath, files[0].ToLower());
			return a.ExtractFile(files[1], tempdir, overwrite);
		}

		// Will open all archives in the given directory and manages the files
		public static void Initialize(string archivespath, string temppath)
		{
			// Keep temp path
			ArchiveManager.temppath = temppath;

			// Find all .rar files and directories
			string[] archfiles = Directory.GetFiles(archivespath, "*.rar");
			string[] archdirs = Directory.GetDirectories(archivespath, "*.rar");

			// Merge the lists
			string[] archfilesdirs = new string[archfiles.Length + archdirs.Length];
			archfiles.CopyTo(archfilesdirs, 0);
			archdirs.CopyTo(archfilesdirs, archfiles.Length);

			// Open all archives
			foreach(string f in archfilesdirs)
			{
				// Open archive
				try { OpenArchive(f); }
				catch(Exception) { }
			}
		}

		// This adds an archive manually
		public static void OpenArchive(string filepathname)
		{
			// Open the archive
			string lf = Path.GetFileName(filepathname).ToLower();
			Archive a = new Archive(filepathname);

			// Make temporary directory
			string tempdir = Path.Combine(ArchiveManager.temppath, lf);
			if(Directory.Exists(tempdir) == false) Directory.CreateDirectory(tempdir);

			// Add to collection
			archives.Add(lf, a);
		}

		// Dispose
		public static void Dispose()
		{
			// Close all archives
			foreach(DictionaryEntry de in archives)
			{
				// Get filename and archive
				string filename = (string)de.Key;
				Archive a = (Archive)de.Value;

				// Close archive
				a.Dispose();

				// Remove temporary directory
				string tempdir = Path.Combine(temppath, filename);
				try { Directory.Delete(tempdir, true); } catch(Exception) { }
			}
		}

		#endregion
	}
}
