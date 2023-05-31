using System;

namespace Model
{
    [Serializable]
    /// <summary>
    /// 操作结果封装
    /// </summary>
    public class OperateResultModel
    {
        private int _returnCode = 1; // 200:成功 默认为200
        private string _returnMsg = "成功";//默认为"成功"
        private object _returnData = new object();
        private int _returnTotalRecords = 0;
        private string _token = string.Empty;

        public OperateResultModel()
        {
            ReturnDate = DateTime.Now;
        }

        /// <summary>
        /// 接口返回响应时间
        /// </summary>
        public DateTime ReturnDate { get; set; }

        public int ReturnCode
        {
            get { return _returnCode; }
            set { _returnCode = value; }
        }
        public string ReturnMsg
        {
            get { return _returnMsg; }
            set { _returnMsg = value; }
        }

        public object ReturnData
        {
            //get { return _returnData; }
            get { return _returnData; }
            set { _returnData = value; }
        }
        public int ReturnTotalRecords
        {
            get { return _returnTotalRecords; }
            set { _returnTotalRecords = value; }
        }




        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="message">消息</param>
        /// <param name="data">操作结果</param>
        public OperateResultModel(string message, object data = null, int code = 1)
        {
            this.ReturnCode = code;
            this.ReturnMsg = message;
            this.ReturnData = data;
        }

    }
    public class OperateResultModel<T> where T : new()
    {
        private int _returnCode = 1; // 0:失败 1:成功 默认为1
        private string _returnMsg = "成功";//默认为"成功"
        private T _returnData = new T();
        private int _returnTotalRecords = 0;
        private string _token = string.Empty;


        public int ReturnCode
        {
            get { return _returnCode; }
            set { _returnCode = value; }
        }
        public string ReturnMsg
        {
            get { return _returnMsg; }
            set { _returnMsg = value; }
        }

        public T ReturnData
        {
            get { return _returnData; }
            set { _returnData = value; }
        }
        public int ReturnTotalRecords
        {
            get { return _returnTotalRecords; }
            set { _returnTotalRecords = value; }
        }
    }
}
