/********************************************************************\
*                                                                   *
*  Archive class by Pascal vd Heiden, www.codeimp.com               *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This class provides functionality to read files from an
// archive (Directory or RAR file).

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace CodeImp.Bloodmasters;

public sealed class Archive
{
    #region ================== Variables

    // This indicates if the source are the contents of a directory
    private bool isDirectory;

    // This is the full path and name of the directory or rar file
    private string archiveName;

    #endregion

    #region ================== Properties

    // Title of this archive
    public string Title { get; private set; }

    // This returns the list of files
    public string[] FileNames { get; private set; }

    #endregion

    #region ================== Constructor / Destructor

    // This opens an archive (directory or rar)
    public Archive(string pathname)
    {
        // Open archive
        this.Open(pathname);
    }

    #endregion

    #region ================== Methods

    // This opens an archive (directory or rar)
    [MemberNotNull(nameof(FileNames), nameof(Title), nameof(archiveName))]
    private void Open(string pathname)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathname);

        // Set the archive pathname and title
        archiveName = pathname;
        Title = Path.GetFileName(pathname);

        // Check if the archive exists
        if(File.Exists(pathname))
        {
            // Source is an archive
            isDirectory = false;

            switch (Path.GetExtension(pathname).ToLowerInvariant())
            {
                case ".rar":
                {
                    using var archive = RarArchive.Open(archiveName);

                    FileNames = new string[archive.Entries.Count];

                    int currentEntryIdx = 0;
                    foreach(var entry in archive.Entries)
                    {
                        FileNames[currentEntryIdx] = entry.Key;
                        currentEntryIdx++;
                    }
                    break;
                }
                case ".zip":
                {
                    using var zipArchive = new ZipArchive(File.OpenRead(archiveName));
                    FileNames = zipArchive.Entries.Select(e => e.FullName).ToArray();
                    break;
                }
                default:
                {
                    throw new Exception($"Unknown file type: \"{pathname}\".");
                }
            }
        }
        // Check if a directory exists
        else if(Directory.Exists(pathname))
        {
            // Source is a directory
            isDirectory = true;

            // List all directory contents
            FileNames = Directory.GetFiles(archiveName);

            // Go for all files
            for(int i = 0; i < FileNames.Length; i++)
            {
                // Keep filename only
                FileNames[i] = Path.GetFileName(FileNames[i]);
            }
        }
        // Nothing found at pathname
        else
        {
            // Throw error: File not found
            throw new FileNotFoundException("Cannot find the specified archive or directory '" + pathname + "'.", pathname);
        }
    }

    // This tests if a file title exists
    public bool FileExists(string filename)
    {
        // Go for all files and return true if this file exists
        foreach(string f in FileNames) if(string.Equals(f, filename, StringComparison.InvariantCultureIgnoreCase)) return true;

        // Nothing with this name found, return false
        return false;
    }

    public string ExtractFile(string filename, string targetPath, bool overwrite)
    {
        // Determine target file path
        string targetFile = Path.Combine(targetPath, filename);

        // Does the target already exist?
        if(File.Exists(targetFile))
        {
            // Should we overwrite the file?
            if(overwrite)
            {
                // Remove the old file
                File.Delete(targetFile);
            }
            else
            {
                // Return the existing file path
                return targetFile;
            }
        }

        // Check if archive is a directory
        if(isDirectory)
        {
            // Determine source file path
            string sourceFile = Path.Combine(archiveName, filename);

            // Check if the source file exists
            if(File.Exists(sourceFile))
            {
                // Copy the file to the target location
                //File.Copy(sourceFile, targetFile, true);

                // Return the original file
                return sourceFile;
            }
            else
            {
                // File not found
                throw(new FileNotFoundException("Cannot find the file '" + filename + "' in archive '" + Title + "'."));
            }
        }
        else
        {
            switch (Path.GetExtension(archiveName).ToLowerInvariant())
            {
                case ".rar":
                {
                    using var archive = RarArchive.Open(archiveName);

                    foreach(var entry in archive.Entries)
                    {
                        if (entry.Key == filename)
                        {
                            entry.WriteToDirectory(targetPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            break;
                        }
                    }
                    break;
                }
                case ".zip":
                {
                    using var zipArchive = new ZipArchive(File.OpenRead(archiveName));
                    foreach(var entry in zipArchive.Entries)
                    {
                        if (entry.FullName == filename)
                        {
                            entry.ExtractToFile(targetFile, overwrite);
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    throw new Exception($"Unknown file type: \"{archiveName}\".");
                }
            }
        }

        // Return the target file path
        return targetFile;
    }

    // This extracts all files
    public void ExtractAllFiles(string targetPath)
    {
        // Check if archive is a directory
        if(isDirectory)
        {
            // Go for all filenames
            foreach(string f in FileNames)
            {
                // Determine source and target file path
                string sourceFile = Path.Combine(archiveName, f);
                string targetFile = Path.Combine(targetPath, f);

                // Check if the source file exists
                if(File.Exists(sourceFile))
                {
                    // Copy the file to the target location
                    File.Copy(sourceFile, targetFile, true);
                }
                else
                {
                    // File not found
                    throw(new FileNotFoundException("Cannot find the file '" + f + "' in archive '" + Title + "'."));
                }
            }
        }
        else
        {
            switch (Path.GetExtension(archiveName).ToLowerInvariant())
            {
                case ".rar":
                {
                    using var archive = RarArchive.Open(archiveName);

                    foreach(var entry in archive.Entries)
                    {
                        entry.WriteToDirectory(targetPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    }
                    break;
                }
                case ".zip":
                {
                    using var zipArchive = new ZipArchive(File.OpenRead(archiveName));
                    foreach(var entry in zipArchive.Entries)
                    {
                        entry.ExtractToFile(Path.Combine(targetPath, entry.FullName), true);
                    }
                    break;
                }
                default:
                {
                    throw new Exception($"Unknown file type: \"{archiveName}\".");
                }
            }


        }
    }

    #endregion
}
