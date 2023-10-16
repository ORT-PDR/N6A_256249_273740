using System;
using System.Net.Sockets;

namespace Communication.FileHandlers
{
	public class FileCommsHandler
	{
        private readonly ConversionHandler _conversionHandler;
        private readonly FileHandler _fileHandler;
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly NetworkDataHelper _networkDataHelper;

        public FileCommsHandler(NetworkDataHelper networkDataHelper)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _networkDataHelper = networkDataHelper;
        }

        public void SendFile(string path)
        {
            if (_fileHandler.FileExists(path))
            {
                var fileName = _fileHandler.GetFileName(path);
                _networkDataHelper.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
                _networkDataHelper.Send(_conversionHandler.ConvertStringToBytes(fileName));

                long fileSize = _fileHandler.GetFileSize(path);
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                _networkDataHelper.Send(convertedFileSize);
                SendFileWithStream(fileSize, path);
            }
            else
            {
                byte[] responseBytes = _conversionHandler.ConvertStringToBytes("empty");
                int responseLength = responseBytes.Length;
                byte[] lengthBytes = _conversionHandler.ConvertIntToBytes(responseLength);
                _networkDataHelper.Send(lengthBytes);
                _networkDataHelper.Send(responseBytes);
            }
        }

        public string ReceiveFile()
        {
            string fullSavePath = "";

            int fileNameSize = _conversionHandler.ConvertBytesToInt(
                _networkDataHelper.Receive(Protocol.FixedDataSize));
            string fileName = _conversionHandler.ConvertBytesToString(_networkDataHelper.Receive(fileNameSize));

            if (fileName != "empty")
            {
                long fileSize = _conversionHandler.ConvertBytesToLong(_networkDataHelper.Receive(Protocol.FixedFileSize));

                string saveFolderPath = "../../Server/productImages";

                fullSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolderPath, fileName);
                ReceiveFileWithStreams(fileSize, fullSavePath);
            }
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

                _networkDataHelper.Send(data);
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
                    data = _networkDataHelper.Receive(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _networkDataHelper.Receive(Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
    }
}

