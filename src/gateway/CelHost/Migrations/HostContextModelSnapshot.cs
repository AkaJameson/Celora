﻿// <auto-generated />
using System;
using CelHost.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CelHost.Migrations
{
    [DbContext(typeof(HostContext))]
    partial class HostContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("CelHost.Data.BlocklistRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BlockCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BlockIp")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("BlockReason")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EffectiveTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpireTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsPermanent")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastViolationTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BlockIp")
                        .IsUnique()
                        .HasDatabaseName("Host_Block_Ip");

                    b.ToTable("BlocklistRecord");
                });

            modelBuilder.Entity("CelHost.Data.Cascade", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cascade");
                });

            modelBuilder.Entity("CelHost.Data.Cluster", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CheckOptionId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HealthCheck")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HealthCheckId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hosts")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Methods")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RateLimitPolicyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RouteId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CheckOptionId");

                    b.HasIndex("HealthCheckId");

                    b.ToTable("Cluster");
                });

            modelBuilder.Entity("CelHost.Data.ClusterNode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastHealthCheck")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.ToTable("ClusterNode");
                });

            modelBuilder.Entity("CelHost.Data.HealthCheckOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActiveInterval")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ActivePath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int>("ActiveTimeout")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("HealthCheckOptions");
                });

            modelBuilder.Entity("CelHost.Data.RateLimitPolicy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PermitLimit")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PolicyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("QueueLimit")
                        .HasColumnType("INTEGER");

                    b.Property<int>("QueueProcessingOrder")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Window")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("RateLimitPolicies");
                });

            modelBuilder.Entity("CelHost.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsLock")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockEnable")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LockTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CelHost.Data.Cluster", b =>
                {
                    b.HasOne("CelHost.Data.HealthCheckOption", "CheckOption")
                        .WithMany()
                        .HasForeignKey("CheckOptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CelHost.Data.HealthCheckOption", null)
                        .WithMany("Clusters")
                        .HasForeignKey("HealthCheckId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckOption");
                });

            modelBuilder.Entity("CelHost.Data.ClusterNode", b =>
                {
                    b.HasOne("CelHost.Data.Cluster", "Cluster")
                        .WithMany("Nodes")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cluster");
                });

            modelBuilder.Entity("CelHost.Data.Cluster", b =>
                {
                    b.Navigation("Nodes");
                });

            modelBuilder.Entity("CelHost.Data.HealthCheckOption", b =>
                {
                    b.Navigation("Clusters");
                });
#pragma warning restore 612, 618
        }
    }
}
