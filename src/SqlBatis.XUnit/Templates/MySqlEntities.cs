using System;
using SqlBatis.Attributes;

namespace SqlBatis.XUnit
{
	
	/// <summary>
    /// 
    /// </summary>
	[Table("address")]
	public partial class AddressDto
	{
				
		/// <summary>
        /// 
		/// </summary>
		[Column("id")]
        [PrimaryKey]  
        public int? Id { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("addr_name")]
        public string AddrName { get; set; }
	}
	
	/// <summary>
    /// 
    /// </summary>
	[Table("school")]
	public partial class SchoolDto
	{
				
		/// <summary>
        /// 
		/// </summary>
		[Column("id")]
        [PrimaryKey]  
        [Identity]
        public int? Id { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("sch_name")]
        public string SchName { get; set; }
	}
	
	/// <summary>
    /// 
    /// </summary>
	[Table("student")]
	public partial class StudentDto
	{
				
		/// <summary>
        /// 
		/// </summary>
		[Column("id")]
        [PrimaryKey]  
        [Identity]
        public int? Id { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("sch_id")]
        public int? SchId { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("addr_id")]
        public int? AddrId { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("stu_name")]
        public string StuName { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("stu_gender")]
        public bool? StuGender { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("score")]
        public double? Score { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("version")]
        [ConcurrencyCheck]
        public string Version { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("create_time")]
        [Default]
        public DateTime? CreateTime { get; set; }
	}
}



