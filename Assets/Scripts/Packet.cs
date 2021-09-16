using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

public class Packet 
{
    public Packet(byte[] packet)
    {
        bytes = packet;
    }
    public byte[] bytes {get; set;}
    public int readPos = 0;
    
    public bool ReadBool()
    {
        if(bytes.Length < readPos) throw new Exception("Cannot read Bool: You are out of range");
        bool data = BitConverter.ToBoolean(bytes, readPos);
        readPos++;
        return data; 
    }

    public float ReadFloat()
    {
        if(bytes.Length < readPos) throw new Exception("Cannot read Float: You are out of range");
        float data = BitConverter.ToSingle(bytes, readPos);
        readPos+=4;
        return data; 
    }

}
