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

        public async Task SendFile(string path)
        {
            if (await _fileHandler.FileExists(path))
            {
                var fileName = _fileHandler.GetFileName(path);
                await _networkDataHelper.SendAsync(_conversionHandler.ConvertIntToBytes((await fileName).Length));
                await _networkDataHelper.SendAsync(_conversionHandler.ConvertStringToBytes(await fileName));

                long fileSize = await _fileHandler.GetFileSize(path);
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                await _networkDataHelper.SendAsync(convertedFileSize);
                await SendFileWithStream(fileSize, path);
            }
            else
            {
                byte[] responseBytes = _conversionHandler.ConvertStringToBytes("empty");
                int responseLength = responseBytes.Length;
                byte[] lengthBytes = _conversionHandler.ConvertIntToBytes(responseLength);
                await _networkDataHelper.SendAsync(lengthBytes);
                await _networkDataHelper.SendAsync(responseBytes);
            }
        }

        public async Task<string> ReceiveFile()
        {
            string fullSavePath = "";

            int fileNameSize = _conversionHandler.ConvertBytesToInt(await _networkDataHelper.ReceiveAsync(Protocol.FixedDataSize));
            string fileName = _conversionHandler.ConvertBytesToString(await _networkDataHelper.ReceiveAsync(fileNameSize));

            if (fileName != "empty")
            {
                long fileSize = _conversionHandler.ConvertBytesToLong(await _networkDataHelper.ReceiveAsync(Protocol.FixedFileSize));

                string saveFolderPath = "../../../productImages";

                fullSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolderPath, fileName);
                await ReceiveFileWithStreams(fileSize, fullSavePath);
            }
            return fullSavePath;
        }

        private async Task SendFileWithStream(long fileSize, string path)
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
                    data = await _fileStreamHandler.Read(path, offset, lastPartSize); 
                    offset += lastPartSize;
                }
                else
                {
                    data = await _fileStreamHandler.Read(path, offset, Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }

                await _networkDataHelper.SendAsync(data);
                currentPart++;
            }
        }

        private async Task ReceiveFileWithStreams(long fileSize, string fileName)
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
                    data = await _networkDataHelper.ReceiveAsync(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = await _networkDataHelper.ReceiveAsync(Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }
                await _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
    }
}

