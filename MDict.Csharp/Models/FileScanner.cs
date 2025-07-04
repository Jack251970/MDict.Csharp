namespace MDict.Csharp.Models;

/// <summary>
/// Example usage:
/// using (var scanner = new FileScanner("path/to/file.bin"))
/// {
///     var buffer = scanner.ReadBuffer(10, 16);
///     var numberBytes = scanner.ReadNumber(100, 4);
///     int number = BitConverter.ToInt32(numberBytes, 0);
/// }
/// </summary>
public class FileScanner : IDisposable
{
    //private string _filepath;
    //private long _offset;
    private FileStream fs;

    /// <summary>
    /// Constructor to initialize the FileScanner with a file path.
    /// </summary>
    /// <param name="filepath"></param>
    internal FileScanner(string filepath)
    {
        //_filepath = filepath;
        //_offset = 0;
        // Open file in read-only mode
        fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
    }

    /// <summary>
    /// Close the file stream if it is open.
    /// </summary>
    public void Close()
    {
        if (fs == null)
        {
            return;
        }

        fs.Close();
        fs = null!;
    }

    /// <summary>
    /// Read a buffer from a specific file offset with specified length
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public byte[] ReadBuffer(long offset, int length)
    {
        byte[] buffer = new byte[length];

        // Set the file position to the specified offset
        fs.Seek(offset, SeekOrigin.Begin);

        // Read into the buffer starting from index 0, up to the desired length
        int bytesRead = fs.Read(buffer, 0, length);

        // Return only the portion that was read
        if (bytesRead < length)
        {
            Array.Resize(ref buffer, bytesRead);
        }

        return buffer;
    }

    /// <summary>
    /// Read a binary number from a specific file offset
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public byte[] ReadNumber(long offset, int length)
    {
        byte[] buffer = new byte[length];

        // Set the file position
        fs.Seek(offset, SeekOrigin.Begin);

        // Read the number into the buffer
        fs.Read(buffer, 0, length);

        return buffer;
    }

    /// <summary>
    /// Dispose method to close the file stream when done.
    /// </summary>
    public void Dispose()
    {
        Close();
    }
}
