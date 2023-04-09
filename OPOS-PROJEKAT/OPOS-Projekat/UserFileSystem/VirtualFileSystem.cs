using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace UserFileSystem
{
    public class VirtualFileSystem : IDokanOperations
    {
        public class File
        {
            private byte[] _data;
            private string _name;
            private DateTime _created;

            public File(string name, DateTime created)
            {
                _data = Array.Empty<byte>();
                _name = name;
                _created = created;
            }

            public byte[] Data { get { return _data; } set { _data = value; } }

            public string Name { get { return _name; } set { _name = value; } }

            public DateTime Created { get { return _created; } set { _created = value; } }
        }

        private readonly Dictionary<string, File> inputFiles = new();
        private readonly Dictionary<string, File> outputFiles = new();

        private readonly static int CAPACITY = 512 * 1024 * 1024;
        private readonly int totalNumberOfBytes = CAPACITY;
        private int totalNumberOfFreeBytes = CAPACITY;

        public void Cleanup(string fileName, IDokanFileInfo info) { }

        public void CloseFile(string fileName, IDokanFileInfo info) { }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            //Provjeravamo vrstu zahtjeva za fajl koji se pravi i ako je zahtjev da se novi fajl napravi,FileMode.CreateNew(Indikacija da se novi fajl treba kreirati)
            //Dakle provjeravamo da li se zeli kreirati novi fajl pa onda da li vec postoji
            if (mode == FileMode.CreateNew)
            {
                //Ako ime sadrzi u sebi tj dio je putanje input i ne nalazi se u listi fajlova, dodajemo unos u Dictionary
                if (fileName.StartsWith(Path.DirectorySeparatorChar + "input" + Path.DirectorySeparatorChar) && !inputFiles.ContainsKey(fileName))
                    inputFiles.Add(fileName, new File(fileName, DateTime.Now));
                //Isto provjeramo u suprotnom ako je namijenjen za output direktorijum
                else if (fileName.StartsWith(Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar) && !outputFiles.ContainsKey(fileName))
                    outputFiles.Add(fileName, new File(fileName, DateTime.Now));
            }
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            //Ova funkcija se koristi da bi se dobila informacija o fajlovima u vfs-u
            //Ova funkcija se koristi npr da bi izlistali fajlove recimo kada otvorimo u fajl eksploreru
            //Trazimo Fajlove po datom paternu
            //lista informacija o fajlovima
            files = new List<FileInformation>();
            //Da li je korijeni direktorijum onda dodajemo informacije dva direktorijuma input i output koja tu imamo
            if (fileName == Path.DirectorySeparatorChar.ToString())
            {
                files.Add(new FileInformation()
                {
                    Attributes = FileAttributes.Directory,
                    FileName = "input"
                });
                files.Add(new FileInformation()
                {
                    Attributes = FileAttributes.Directory,
                    FileName = "output"
                });
            }
            //ako je rijec o input direktorijumu
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "input"))
            {
                foreach (var file in inputFiles.Values)
                {
                    files.Add(new FileInformation()
                    {
                        FileName = Path.GetFileName(file.Name),
                        Length = file.Data.Length,
                        Attributes = FileAttributes.Normal,
                        CreationTime = file.Created
                    });
                }
            }
            //Ako je rijec o output direktorijumu izlistavamo sve fajlove i dodajemo u nasu listu
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "output"))
            {
                foreach (var file in outputFiles.Values)
                {
                    files.Add(new FileInformation()
                    {
                        FileName = Path.GetFileName(file.Name),
                        Length = file.Data.Length,
                        Attributes = FileAttributes.Normal,
                        CreationTime = file.Created
                    });
                }
            }
            return NtStatus.Success;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = Array.Empty<FileInformation>();
            return NtStatus.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            totalNumberOfFreeBytes = this.totalNumberOfFreeBytes;
            totalNumberOfBytes = this.totalNumberOfBytes;
            freeBytesAvailable = this.totalNumberOfFreeBytes;
            return NtStatus.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            //Recimo koristicemo ovu funkciju kada trebamo da dobijemo informacije o nekom konkretnom fajlu
            //Smjestamo ga u fileinfo
            //Funkcija uzima informacije o datom fajlu
            //ako je u pitanju root directory
            if (fileName == Path.DirectorySeparatorChar.ToString())
            {
                fileInfo = new()
                {
                    FileName = fileName,
                    Attributes = FileAttributes.Directory
                };
            }
            //ako je u pitanju input direktorijum
            else if (fileName == Path.DirectorySeparatorChar + "input")
            {
                fileInfo = new()
                {
                    FileName = "input",
                    Attributes = FileAttributes.Directory
                };
            }
            //ako je u pitanju output direktorijum
            else if (fileName == Path.DirectorySeparatorChar + "output")
            {
                fileInfo = new()
                {
                    FileName = "output",
                    Attributes = FileAttributes.Directory,

                };
            }
            //ako je u pitanju fajl iz input direktorijuma
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "input") && inputFiles.ContainsKey(fileName))
            {
                fileInfo = new()
                {
                    FileName = fileName,
                    Length = inputFiles[fileName].Data.Length,
                    Attributes = FileAttributes.Normal,
                    CreationTime = inputFiles[fileName].Created
                };
            }
            //ako je u pitanju fajl iz output direktorijuma
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "output") && outputFiles.ContainsKey(fileName))
            {
                fileInfo = new()
                {
                    FileName = fileName,
                    Length = outputFiles[fileName].Data.Length,
                    Attributes = FileAttributes.Normal,
                    CreationTime = outputFiles[fileName].Created
                };
            }
            //u suprotnom fajl nije iz datog fajl sistema odnosno nije registrovan
            else
            {
                fileInfo = default;
                return NtStatus.Error;
            }
            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            security = null;
            return NtStatus.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "UserSpaceFSVolume";
            features = FileSystemFeatures.None;
            fileSystemName = "UserSpaceFS";
            maximumComponentLength = 255;
            return NtStatus.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            //funkcija koja se koristi za citanje fajla
            File? file = null;
            //ako ime putanje pocinje sa /input dakle u input fajlu je i gledamo inputFiles
            if (fileName.StartsWith(Path.DirectorySeparatorChar + "input"))
                file = inputFiles[fileName];
            //isto samo sa output
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "output"))
                file = outputFiles[fileName];
            //pozicioniramo se na offset citamo onoliko koliko je velicina bafera
            file?.Data.Skip((int)offset).Take(buffer.Length).ToArray().CopyTo(buffer, 0);
            //stavljamo koliko smo procitali bajtova
            int diff = file.Data.Length - (int)offset;
            bytesRead = buffer.Length > diff ? diff : buffer.Length;
            return NtStatus.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            File? file = null;
            //provjeravamo da li se fajl nalazi u input direktorijumu
            if (fileName.StartsWith(Path.DirectorySeparatorChar + "input"))
            {
                if (!inputFiles.ContainsKey(fileName))
                    inputFiles.Add(fileName, new File(fileName, DateTime.Now));
                file = inputFiles[fileName];
            }
            //ili u output direktorijumu
            else if (fileName.StartsWith(Path.DirectorySeparatorChar + "output"))
            {
                if (!outputFiles.ContainsKey(fileName))
                    outputFiles.Add(fileName, new File(fileName, DateTime.Now));
                file = outputFiles[fileName];
            }
            if (info.WriteToEndOfFile) // apend
            {
                file.Data = file.Data.Concat(buffer).ToArray(); // appendao sadrzaj
                bytesWritten = buffer.Length;
            }
            else // rewrite
            {
                int difference = file.Data.Length - (int)offset;
                totalNumberOfFreeBytes += difference;
                file.Data = file.Data.Take((int)offset).Concat(buffer).ToArray();
                bytesWritten = buffer.Length;
            }
            totalNumberOfFreeBytes -= bytesWritten;
            return NtStatus.Success;
        }

        public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
        {
            return DokanResult.Success;
        }
    }
}
