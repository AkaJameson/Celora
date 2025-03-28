﻿using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    [Table("Cluster")]
    public class Cluster
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 微服务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 微服务描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 路由地址
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
    }
}
