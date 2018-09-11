using LuaInterface;
using System.Collections.Generic;

public class MessageSerializer {

    private static List<byte> allMsgBuffers = new List<byte>();
    private static byte[] lengthBuffer = new byte[4];

    public static string createWritePath(string path)
    {
        if (path == null) return null;
        string[] names = path.Split('/');
        path = ResourceManager.inst.WriteablePath + path + "/";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        if (names.Length > 1) path = ResourceManager.inst.WriteablePath + names[0] + "/";
        return path;
    }

    public static void deleteLocalMsgFile(string dirPath,string histroyname)
    {
        if (System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.Delete(dirPath,true);
        }
        System.IO.Directory.CreateDirectory(dirPath + histroyname);
    }

    public static bool moveFile(string fileSour,string fileDest) {
        if (!System.IO.File.Exists(fileDest))
        {
            if (System.IO.File.Exists(fileSour))
            {
                System.IO.File.Move(fileSour, fileDest);
                return true;
            }
        }
        else {
            if (System.IO.File.Exists(fileSour))
            {
                System.IO.File.Delete(fileSour);
            }
        }
        return false;
    }

    public static LuaByteBuffer unserialMessageInfo(string path,int offset)
    {
        if (System.IO.File.Exists(path))
        {
            using (System.IO.FileStream fs = System.IO.File.OpenRead(path))
            {
                if (fs.Length <= offset) return null;
                if (offset > 0) fs.Seek(offset, System.IO.SeekOrigin.Begin);
                byte[] bytes = new byte[4];
                fs.Read(bytes, 0, 4);
                int length = readInt32Byte(bytes);
                bytes = new byte[length];
                fs.Read(bytes, 0, length);
                fs.Flush();
                return new LuaByteBuffer(bytes);
            }
        }
        return null;
    }

    public static LuaByteBuffer[] unserialAllMessageInfo(string path)
    {
        if (System.IO.File.Exists(path))
        {
            List<LuaByteBuffer> msgs = new List<LuaByteBuffer>();
            byte[] msgBytes = null;
            byte[] allbytes = System.IO.File.ReadAllBytes(path);
            int bufferLen = 0;
            for (int index = 0, length = allbytes.Length; index < length;)
            {
                bufferLen = readSingleInt32Byte(allbytes[index], allbytes[index + 1], allbytes[index + 2], allbytes[index + 3]);
                msgBytes = new byte[bufferLen];
                index += 4;
                System.Buffer.BlockCopy(allbytes, index, msgBytes, 0, bufferLen);
                index += bufferLen;
                msgs.Add(new LuaByteBuffer(msgBytes));
            }
            return msgs.ToArray();
            /*using (System.IO.FileStream fs = System.IO.File.OpenRead(path))
            {
                List<LuaByteBuffer> msgs = new List<LuaByteBuffer>();
                long fileLength = fs.Length;
                if (fileLength > 0)
                {
                    byte[] msgBytes = null;
                    / *int readCount = 0;
                    do{
                        readCount = fs.Read(bytes, 0, 4);
                        if (readCount <= 0) break;
                        int length = readInt32Byte(bytes);
                        msgBytes = new byte[length];
                        readCount = fs.Read(msgBytes, 0, length);
                        fs.Flush();
                        msgs.Add(new LuaByteBuffer(msgBytes));
                    } while (readCount > 0);* /

                    long pos = 0;
                    do
                    {
                        fs.Read(lengthBuffer, 0, 4);
                        pos += 4;
                        int length = readInt32Byte(lengthBuffer);
                        msgBytes = new byte[length];
                        fs.Read(msgBytes, 0, length);
                        pos += length;
                        fs.Flush();
                        msgs.Add(new LuaByteBuffer(msgBytes));
                    } while (pos < fileLength);
                    return msgs.ToArray();
                }
            }*/
        }
        return null;
    }

    public static int serialMessageInfo(string path, byte[] content)
    {

        using (System.IO.FileStream fs = System.IO.File.OpenWrite(path))
        {
            int length = content.Length;
            byte[] buffer = new byte[length + 4];
            writeInt32Byte(buffer,length,0);
            System.Buffer.BlockCopy(content, 0, buffer, 4, length);
            //System.Array.Copy(content, 0, buffer, 4, length);
            fs.Seek(0,System.IO.SeekOrigin.End);
            fs.Write(buffer, 0, length + 4);
            fs.Flush();
            return length;
        }
    }

    //public static void serialMultiMessageInfo(string path, LuaTable table, int size)
    //{
    //    using (System.IO.FileStream fs = System.IO.File.OpenWrite(path))
    //    {
    //        int length = table.Length;
    //        size = size + 4 * length;
    //        byte[] totalbuffer = new byte[size];
    //        byte[] current = null;
    //        int offset = 0;
    //        int msgLength = 0;
    //        for (int index = 1; index <= length; index++)
    //        {
    //            current = table[index] as byte[];
    //            msgLength = current.Length;
    //            writeInt32Byte(totalbuffer, msgLength, offset);
    //            System.Buffer.BlockCopy(current, 0, totalbuffer, offset + 4, msgLength);
    //            //System.Array.Copy(content, 0, totalbuffer,  offset + 4, msgLength);  
    //            offset = offset + 4 + msgLength;
    //        }
    //        fs.Seek(0, System.IO.SeekOrigin.End);
    //        fs.Write(totalbuffer, 0, size);
    //        fs.Flush();
    //    }
    //}

    public static void addBufferAndSerialMsgInfo(string path, byte[] buffer,bool lastWrite)
    {
        if (buffer == null || buffer.Length == 0) return;
        writeInt32Byte(lengthBuffer, buffer.Length, 0);
        allMsgBuffers.AddRange(lengthBuffer);
        allMsgBuffers.AddRange(buffer);
        if (lastWrite)
        {
            using (System.IO.FileStream fs = System.IO.File.OpenWrite(path))
            {
                fs.Seek(0, System.IO.SeekOrigin.End);
                fs.Write(allMsgBuffers.ToArray(), 0, allMsgBuffers.Count);
                fs.Flush();
            }
            allMsgBuffers.Clear();
        }
    }

    public static int[] getMsgSessionIDs(string path)
    {
        string[] files = System.IO.Directory.GetFiles(path,"*.*",System.IO.SearchOption.TopDirectoryOnly);
        int length = files.Length;
        if (length > 0)
        {
            //int[] fileIds = new int[length];
            //for (int index = 0; index < length; index++)
            //{
            //    //Debug.Log(files[index]);
            //    fileIds[index] = int.Parse(System.IO.Path.GetFileNameWithoutExtension(files[index]));
            //}
            //return fileIds;

            List<int> fileIds = new List<int>();
            int curId = 0;
            for (int index = 0; index < length; index++)
            {
                if (int.TryParse(System.IO.Path.GetFileNameWithoutExtension(files[index]), out curId))
                {
                    fileIds.Add(curId);
                }
            }
            return fileIds.ToArray();
        }
        return null;
    }

    protected static void writeInt32Byte(byte[] buffer,int value,int offset) {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    protected static int readInt32Byte(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 4)
        {
            return 0;
        }
        return (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | (buffer[0]);
    }

    protected static int readSingleInt32Byte(byte one,byte two,byte three,byte four)
    {
        return (four << 24) | (three << 16) | (two << 8) | one;
    }

}
