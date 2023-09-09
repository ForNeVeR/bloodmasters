/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters;

public class ArchiveManager
{
    #region ================== Variables

    // All archives
    private static Dictionary<string, Archive> archives = new();

    // Temporary path
    private static string tempPath = Paths.TempDir;

    #endregion

    #region ================== Methods

    // This returns the temp path for a specific archive
    public static string GetArchiveTempPath(Archive archive)
    {
        return Path.Combine(tempPath, archive.Title);
    }

    // This finds all files of a specific type in all archives
    public static List<string> FindAllFiles(string filetype)
    {
        List<string> result = new List<string>();

        // Go for all archives
        foreach((string archivename, Archive a) in archives)
        {
            // Go for all files in archive
            foreach(string f in a.FileNames)
            {
                // Filename matches?
                if(f.EndsWith(filetype, StringComparison.InvariantCultureIgnoreCase))
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
        foreach((string archivename, Archive a) in archives)
        {
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
        if(archives.TryGetValue(archivename, out Archive archive))
        {
            // Return archive
            return archive;
        }
        else
        {
            // No such archive
            throw(new Exception("No archive '" + archivename + "' loaded."));
        }
    }

    // This returns the archive for a specific file
    // The filepathname must be like this:  textures.zip/grass.bmp
    public static Archive GetFileArchive(string filepathname)
    {
        // Split the filepathname
        string[] files = filepathname.Split('/');

        // Make the archive name
        string archivename = files[0].ToLower();

        // Check if archive exists
        if(archives.TryGetValue(archivename, out Archive archive))
        {
            // Return archive
            return archive;
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
        if(archives.TryGetValue(archivename, out Archive a))
        {
            // Check if archive has the specified file
            return a.FileExists(files[1]);
        }
        else
        {
            // Archive not found
            return false;
        }
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
        string tempdir = Path.Combine(tempPath, files[0].ToLower());
        return a.ExtractFile(files[1], tempdir, overwrite);
    }

    // Will open all archives in the given directory and manages the files
    public static void Initialize(string archivespath)
    {
        // Find all .zip files and directories
        string[] archfiles = Directory.GetFiles(archivespath, "*.zip");
        string[] archdirs = Directory.GetDirectories(archivespath, "*.zip");

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
        string tempdir = Path.Combine(ArchiveManager.tempPath, lf);
        if(Directory.Exists(tempdir) == false) Directory.CreateDirectory(tempdir);

        // Add to collection
        archives.Add(lf, a);
    }

    // Dispose
    public static void Dispose()
    {
        // Close all archives
        foreach((string filename, Archive a) in archives)
        {
            // Remove temporary directory
            string tempdir = Path.Combine(tempPath, filename);
            try { Directory.Delete(tempdir, true); } catch(Exception) { }
        }
    }

    #endregion
}
