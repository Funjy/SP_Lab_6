using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClientServerInterface
{
    public class FilePreparer : IDisposable
    {

// ReSharper disable InconsistentNaming
        public const string FILE_READ_COMPLETE = "Файл полностью прочитан.";
        public const string TRY_WRITE = "Попытка записи.";
        public const string TRY_READ = "Попытка чтения.";
        public const string NEED_OPEN_FILE_FIRST = "Сначала необходимо открыть файл.";
        public const string TRY_OPEN_WRITE = "Попытка открыть для записи.";
        public const string TRY_OPEN_READ = "Попытка открыть для чтения.";
        public const string OPENED_FOR_ANOTHER_OPERATION = "Файл открыт для другой операции.";
        public const string OPERATION_CANT_DO = "Невозможно выполнять запрошенную операцию.";
// ReSharper restore InconsistentNaming

        private readonly string _filePath;
        private readonly int _partsAmount;
        private int _blockSize;
        private FileStream _fs;

        public bool IsOpenedRead { get; private set; }
        public bool IsOpenedWrite { get; private set; }
        public int FileLength { get; private set; }
        public int BlocksRead { get; set; }

        public FilePreparer(string filePath, int blocksNum = 1)
        {
            _filePath = filePath;
            _partsAmount = blocksNum;
            Reset();
        }

        void Reset()
        {
            IsOpenedRead = false;
            IsOpenedWrite = false;
            FileLength = -1;
            BlocksRead = 0;
        }

        public void OpenRead()
        {
            if(IsOpenedRead)
                return;
            if (IsOpenedWrite)
                throw new InvalidOperationException(TRY_OPEN_READ + ' ' + OPENED_FOR_ANOTHER_OPERATION);
            DetermineLength();
            _fs = File.OpenRead(_filePath);
            if(!_fs.CanRead)
                throw new InvalidOperationException(TRY_OPEN_READ + ' ' + OPERATION_CANT_DO);
            IsOpenedRead = true;
        }

        public void OpenWrite()
        {
            if (IsOpenedWrite)
                return;
            if (IsOpenedRead)
                throw new InvalidOperationException(TRY_OPEN_WRITE + ' ' + OPENED_FOR_ANOTHER_OPERATION);
            DetermineLength();
            _fs = File.OpenWrite(_filePath);
            if(!_fs.CanWrite)
                throw new InvalidOperationException(TRY_OPEN_WRITE + ' ' + OPERATION_CANT_DO);
            IsOpenedWrite = true;
        }

        void DetermineLength()
        {
            //file size
            var f = new FileInfo(_filePath);
            FileLength = (int)f.Length;
            //block size
            _blockSize = FileLength/_partsAmount;
            var rem = FileLength%_partsAmount;
            if (rem > 0)
                _blockSize++;

        }

        public byte[] ReadBlock()
        {
            if(!IsOpenedRead)
                throw new InvalidOperationException(TRY_READ + ' ' + NEED_OPEN_FILE_FIRST);
            var buf = new byte[_blockSize];
            var n = _fs.Read(buf, 0, _blockSize);
            if (n == 0)
                throw new InvalidOperationException(FILE_READ_COMPLETE);
            BlocksRead++;
            return buf;
        }

        public void Write(byte[] data)
        {
            if (!IsOpenedWrite)
                throw new InvalidOperationException(TRY_WRITE + ' ' + NEED_OPEN_FILE_FIRST);
            _fs.Write(data, 0, data.Length);
        }

        public void Close()
        {
            _fs.Close();
            Reset();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
