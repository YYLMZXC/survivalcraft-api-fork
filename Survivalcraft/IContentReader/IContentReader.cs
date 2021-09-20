using System;
using System.IO;

namespace Game.IContentReader
{
    public abstract class IContentReader
    {
        public abstract string Type { get;}
        public abstract string[] DefaultSuffix { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FullPath">文件路径，不带后缀</param>
        /// <returns></returns>
        public abstract object Get(ContentInfo[] contents);
    }
}
