using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace BW.Helpers
{
    /// <summary>
    /// 寫成DataTable的擴展方法是這樣
    /// </summary>
    public static class JsonTableHelper
    {
        /// <summary> 
        /// 返回對象序列化 
        /// </summary> 
        /// <param name="obj">源對象</param> 
        /// <returns>json數據</returns> 
        public static string ToJson(object obj)
        {
            JavaScriptSerializer serialize = new JavaScriptSerializer();
            return serialize.Serialize(obj);
        }

        /// <summary> 
        /// 控制深度 
        /// </summary> 
        /// <param name="obj">源對象</param> 
        /// <param name="recursionDepth">深度</param> 
        /// <returns>json數據</returns> 
        public static string ToJson(object obj, int recursionDepth)
        {
            JavaScriptSerializer serialize = new JavaScriptSerializer();
            serialize.RecursionLimit = recursionDepth;
            return serialize.Serialize(obj);
        }

        /// <summary> 
        /// DataTable轉為json 
        /// </summary> 
        /// <param name="dt">DataTable</param> 
        /// <returns>json數據</returns> 
        public static List<object> ToJson(this DataTable dt)
        {
            List<object> dic = new List<object>();

            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (DataColumn dc in dt.Columns)
                {
                    result.Add(dc.ColumnName, dr[dc].ToString());
                }
                dic.Add(result);
            }
            return dic;
        }
    }
}