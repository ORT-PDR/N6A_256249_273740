using System;
using System.Net.Sockets;

namespace Communication.FileHandlers
{
	public class FileCommsHandler
	{
        private readonly ConversionHandler _conversionHandler;
        private readonly FileHandler _fileHandler;
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly SocketHelper _socketHelper;

        public FileCommsHandler(SocketHelper socketHelper)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _socketHelper = socketHelper;
        }

        public void SendFile(string path)
        {
            if (_fileHandler.FileExists(path))
            {
                var fileName = _fileHandler.GetFileName(path);
                _socketHelper.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
                _socketHelper.Send(_conversionHandler.ConvertStringToBytes(fileName));

                long fileSize = _fileHandler.GetFileSize(path);
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                _socketHelper.Send(convertedFileSize);
                SendFileWithStream(fileSize, path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }

        public string ReceiveFile()
        {
            int fileNameSize = _conversionHandler.ConvertBytesToInt(
                _socketHelper.Receive(Protocol.FixedDataSize));
            string fileName = _conversionHandler.ConvertBytesToString(_socketHelper.Receive(fileNameSize));
            long fileSize = _conversionHandler.ConvertBytesToLong(_socketHelper.Receive(Protocol.FixedFileSize));

            string saveFolderPath = "../../Server/productImages";

            string fullSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolderPath, fileName);
            ReceiveFileWithStreams(fileSize, fullSavePath);
            return fullSavePath;
        }

        private void SendFileWithStream(long fileSize, string path)
        {
            long fileParts = Protocol.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize); 
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }

                _socketHelper.Send(data);
                currentPart++;
            }
        }

        private void ReceiveFileWithStreams(long fileSize, string fileName)
        {
            long fileParts = Protocol.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _socketHelper.Receive(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _socketHelper.Receive(Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
    }
}

