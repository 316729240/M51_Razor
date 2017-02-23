﻿using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MWMS.DAL.Datatype.FieldType
{
    public class Picture:File
    {
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string MinPath { get; set; }
    }
    public class Pictures : Picture
    {
        List<Picture> _list = new List<Picture>();
        /// <summary>
        /// 将字符串转换为Files字段数据类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Pictures Parse(string data)
        {
            Pictures files = new Pictures();
            try
            {
                List<Picture> list = data.ParseJson<List<Picture>>();
                foreach (Picture file in list)
                {
                    if (file.isDel == 0)
                    {
                        files.Add(file);
                    }
                    else
                    {
                        #region 删除无效文件 
                        try
                        {
                            string path = HttpContext.Current.Server.MapPath("~" + file.path);
                            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                            string minpath = HttpContext.Current.Server.MapPath("~" + file.MinPath);
                            if (System.IO.File.Exists(minpath)) System.IO.File.Delete(minpath);
                        }
                        catch
                        {

                        }
                        #endregion
                    }
                }
            }
            catch
            {
            }
            if (files.Count > 0) {
                #region 设置默认值
                files.title = files[0].title;
                files.path = files[0].path;
                files.title = files[0].title;
                files.isDel = files[0].isDel;
                files.MinPath = files[0].MinPath;
                #endregion
            }
            return files;
        }
        public Picture this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
        public void Add(Picture file)
        {
            _list.Add(file);
        }
        public override string ToString()
        {
            return path;
        }
        public int Count {get{return _list.Count;} }
        public string ToJson()
        {
            return _list.ToJson();
        }
    }
}
