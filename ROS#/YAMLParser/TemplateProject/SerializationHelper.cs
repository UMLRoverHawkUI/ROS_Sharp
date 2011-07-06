﻿#region USINGZ

using System;
using System.Runtime.InteropServices;

#endregion

namespace Messages
{
    public static class SerializationHelper
    {
        public static T Deserialize<T>(byte[] bytes)
        {
            T thestructure = default(T);
            IntPtr pIP = Marshal.AllocHGlobal(Marshal.SizeOf(thestructure));
            Marshal.Copy(bytes, 0, pIP, Marshal.SizeOf(thestructure));
            thestructure = (T) Marshal.PtrToStructure(pIP, typeof (T));
            Marshal.FreeHGlobal(pIP);
            /*StructTranslator thisone = new StructTranslator();
            T thestructure = default(T);
            if (thisone.Read<T>(bytes, 0, ref thestructure))
                Console.WriteLine("YAY!");*/
            return thestructure;
        }

        public static byte[] Serialize<T>(T outgoing) where T : IRosMessage
        {
            if (outgoing.Serialized != null)
                return outgoing.Serialized;
            outgoing.Serialized = new byte[Marshal.SizeOf(outgoing)];
            GCHandle h = GCHandle.Alloc(outgoing.Serialized, GCHandleType.Pinned);

            // copy the struct into int byte[] mem alloc 
            Marshal.StructureToPtr(outgoing, h.AddrOfPinnedObject(), false);

            h.Free(); //Allow GC to do its job 

            return outgoing.Serialized;
        }
    }

    public class IRosMessage
    {
        public bool HasHeader;
        public bool KnownSize = true;

        public byte[] Serialized;
        public string connection_header;
        public TypeEnum type = TypeEnum.Unknown;

        public IRosMessage()
        {
        }

        public IRosMessage(byte[] SERIALIZEDSTUFF)
        {
            Deserialize(SERIALIZEDSTUFF);
        }

        public virtual void Deserialize(byte[] SERIALIZEDSTUFF)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            return null;
        }
    }
}