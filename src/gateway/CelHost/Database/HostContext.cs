using CelHost.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CelHost.Data
{
    public class HostContext : DbContext
    {
        public HostContext(DbContextOptions options) : base(options)
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
        public DbSet<BlocklistRecord> blocklistRecords { get; set; }
        public DbSet<RateLimitPolicy> RateLimitPolicies { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cluster>().HasMany(p => p.Nodes).WithOne(p => p.Cluster).HasForeignKey(p => p.ClusterId);
            modelBuilder.Entity<Cluster>().HasOne<RateLimitPolicy>().WithMany(p => p.Clusters).HasForeignKey(p => p.RateLimitPolicyId);
        }
    }
}
