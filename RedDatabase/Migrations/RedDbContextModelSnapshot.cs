﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RedDatabase.Model;

#nullable disable

namespace RedDatabase.Migrations
{
    [DbContext(typeof(RedDbContext))]
    partial class RedDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("RedDatabase.Model.RedFile", b =>
                {
                    b.Property<ulong>("RedFileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Archive")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UsedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Uses")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("RedFileId");

                    b.ToTable("Files");
                });
#pragma warning restore 612, 618
        }
    }
}