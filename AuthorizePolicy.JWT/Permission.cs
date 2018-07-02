﻿
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthorizePolicy.JWT
{
    /// <summary>
    /// 用户或角色或其他凭据实体
    /// </summary>
    public class Permission
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 用户或角色或其他凭据名称
        /// </summary>
        public virtual string Name
        { get; set; }
        /// <summary>
        /// 请求Url
        /// </summary>
        public virtual string Url
        { get; set; }
    }
}
