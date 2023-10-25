using System;
namespace Communication.FileHandlers
{
	public class FileStreamHandler
	{
        private readonly FileHandler _fileHandler;
        public FileStreamHandler()
        {
            _fileHandler = new FileHandler();
        }
        public async Task<byte[]> Read(string path, long offset, int length)
        {
            if (await _fileHandler.FileExists(path))
            {
                var data = new byte[length];

                using var fs = new FileStream(path, FileMode.Open) { Position = offset };
                var bytesRead = 0;
                while (bytesRead < length)
                {
                    var read = fs.Read(data, bytesRead, length - bytesRead);
                    if (read == 0)
                        throw new Exception("Error reading file");
                    bytesRead += read;
                }

                return data;
            }

            throw new Exception("File does not exist");
        }

        public async Task Write(string fileName, byte[] data)
        {
            var fileMode = await _fileHandler.FileExists(fileName) ? FileMode.Append : FileMode.Create;
            using var fs = new FileStream(fileName, fileMode);
            fs.Write(data, 0, data.Length);
        }
    }
}

