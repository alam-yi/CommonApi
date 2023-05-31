using System;

namespace Model
{
    public class JwtModel
    {
        /// <summary>
        /// JWT返回数据模型，适用于前端判断过期时间，当后端判断做验证时候，只需要返回一个访问令牌即可 yjh
        /// </summary>
        public class ResultJwtModel
        {
            /// <summary>
            /// 访问令牌token
            /// </summary>
            public string AccessToken { get; set; }
            /// <summary>
            /// 刷新令牌token
            /// </summary>
            public string RefreshToken { get; set; }
            /// <summary>
            /// 访问令牌过期时间
            /// </summary>
            public DateTime ExpireTime { get; set; }
        }
    }
}
