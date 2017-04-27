using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LeanCode.Example;
using LeanCode.PushNotifications;

namespace LeanCode.Example.Migrations
{
    [DbContext(typeof(ExampleDbContext))]
    [Migration("20170427084158_AddTokens")]
    partial class AddTokens
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("LeanCode.PushNotifications.EF.PushNotificationTokenEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateCreated");

                    b.Property<int>("DeviceType");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(1024);

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("Token");

                    b.HasIndex("UserId", "DeviceType");

                    b.ToTable("Tokens");
                });
        }
    }
}
