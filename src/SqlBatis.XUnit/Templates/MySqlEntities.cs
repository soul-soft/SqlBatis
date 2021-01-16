using System;
using SqlBatis.Attributes;

namespace SqlBatis.XUnit
{
	
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
        public string Version { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("create_time")]
        [Default]
        public DateTime? CreateTime { get; set; }
	}
}



