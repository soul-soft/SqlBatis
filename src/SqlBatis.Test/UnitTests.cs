using MySql.Data.MySqlClient;
using NUnit.Framework;
using SqlBatis.Attributes;
using Dapper;
using SqlBatis.Expressions;
using SqlBatis.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SqlBatis.Test
{
    public class UnitTests
    {

        [Test]
        public void TestInsertAsync()
        {
            var builder = new DbContextBuilder
            {
                Connection = new MySqlConnection("server=rm-bp16hgp1ext33r96b2o.mysql.rds.aliyuncs.com;user id=mammothcode;password=Jiuxian20180920;database=mammothcode_xiaoyema;"),
            };
            GlobalSettings.XmlCommandsProvider.Load(@"D:\SqlBatis\src\SqlBatis.Test\Student.xml");
            try
            {
                using (var db = new DbContext(builder))
                {
                    var ff = db.Execute("insert into advert_banners(banner_img,banner_sort,banner_group) values(@img,@sort,@group)", new { img=(string)null,sort=20, group=1 });
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

    }
    public class Student
    {
        public int Id { get; set; }
        [Column("is_del")]
        public byte[] IsDel { get; set; }
    }
    public class QueryProductGoodsListModel
    {
        /// <summary>
        /// 品牌id
        /// </summary>
        public int? BrandId { get; set; }
        /// <summary>
        /// 供应商id
        /// </summary>
        public int? SupplierId { get; set; }
        /// <summary>
        /// 商品分类：1级
        /// </summary>
        public int[] CategoryId1 { get; set; }
        /// <summary>
        /// 商品分类：2级
        /// </summary>
        public int[] CategoryId2 { get; set; }
        /// <summary>
        /// 供应商地区id第一级
        /// </summary>
        public int[] SupplierRegionId1 { get; set; }
        /// <summary>
        /// 供应商地区id第二级
        /// </summary>
        public int[] SupplierRegionId2 { get; set; }
        /// <summary>
        /// 供应商类目
        /// </summary>
        public int? SupplierCategoryId { get; set; }
        /// <summary>
        /// 是否特价促销
        /// </summary>
        public bool? IsPromoteSales { get; set; }
        /// <summary>
        /// 是否新品上架
        /// </summary>
        public bool? IsNewArrivals { get; set; }
        /// <summary>
        /// 用户经度
        /// </summary>
        public double? LocationLng { get; set; }
        /// <summary>
        /// 用户维度
        /// </summary>
        public double? LocationLat { get; set; }
        /// <summary>
        /// 0降序，1升序
        /// </summary>
        public int? SortType { get; set; }
        /// <summary>
        /// 0浏览量，1价格
        /// </summary>
        public int? SortName { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 页长
        /// </summary>
        public int PageSize { get; set; }
    }
}