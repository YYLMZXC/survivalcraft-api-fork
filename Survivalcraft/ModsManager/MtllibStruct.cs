﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace Game
{
    public class MtllibStruct
    {
        public Dictionary<string, string> TexturePaths = new Dictionary<string, string>();
        public static MtllibStruct Load(Stream stream)
        {
            MtllibStruct mtllibStruct = new MtllibStruct();
            using (stream)
            {
                StreamReader streamReader = new StreamReader(stream);
                string Tkey = null;
                string CurrentTkey = null;
                while (streamReader.EndOfStream == false)
                {

                    string line = streamReader.ReadLine();
                    string[] spl = line.Split(new char[] { (char)0x09, (char)0x20 }, System.StringSplitOptions.None);
                    switch (spl[0])
                    {
                        case "newmtl":
                            {
                                Tkey = spl[1];
                                break;
                            }

                        case "map_Kd":
                            {
                                if (string.IsNullOrEmpty(Tkey)) throw new Exception("请先newmtl");
                                mtllibStruct.TexturePaths.Add(Tkey, spl[1]);
                                break;
                            }
                    }
                }
            }
            return mtllibStruct;
        }
    }
}
