using CelHost.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CelHost.Database
{
    public class HostContext : DbContext
    {
        public HostContext(DbContextOptions<HostContext> options) : base(options)
        {

        }
        /// <summary>
        /// 管理端页面
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// 集群
        /// </summary>
        public DbSet<Cluster> Clusters { get; set; }
        /// <summary>
        /// 节点
        /// </summary>
        public DbSet<ClusterNode> ClusterNodes { get; set; }
        /// <summary>
        /// 级联信息
        /// </summary>
        public DbSet<Cascade> Cascades { get; set; }
        /// <summary>
        /// ip黑名单
        /// </summary>
        public DbSet<BlocklistRecord> blocklistRecords { get; set; }
        /// <summary>
        /// 限流策略
        /// </summary>
        public DbSet<RateLimitPolicy> RateLimitPolicies { get; set; }
        /// <summary>
        /// 健康检查策略
        /// </summary>
        public DbSet<HealthCheckOption> HealthCheckOptions { get; set; }
        /// <summary>
        /// 系统字典
        /// </summary>
        public DbSet<SystemDict> SystemDict { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cluster>().HasMany(p => p.Nodes).WithOne(p => p.Cluster).HasForeignKey(p => p.ClusterId);
            modelBuilder.Entity<Cluster>().HasOne<HealthCheckOption>().WithMany(p => p.Clusters).HasForeignKey(p => p.HealthCheckId);
            modelBuilder.Entity<BlocklistRecord>().HasIndex(p => p.BlockIp).IsUnique().HasFilter(null).HasDatabaseName("Host_Block_Ip");
            modelBuilder.Entity<SystemDict>(entity =>
            {
                entity.HasIndex(p => new { p.typeCode, p.itemCode }).IsUnique();
            });

        }
    }
}
