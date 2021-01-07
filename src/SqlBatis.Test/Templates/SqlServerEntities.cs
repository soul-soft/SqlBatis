
using System;
using SqlBatis.Attributes;

namespace SqlBatis.Test
{


	/// <summary>
    /// 
    /// </summary>
	[Table("student_school")]
	public class StudentSchoolDto
	{
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[id]")]
		public int? Id { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[sch_name]")]
		public object SchName { get; set; }
		
	}

	/// <summary>
    /// 
    /// </summary>
	[Table("messages")]
	public class MessagesDto
	{
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[id]")]
		[PrimaryKey]  
        [Identity]   
		public int? Id { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[remake]")]
		public string Remake { get; set; }
		
	}

	/// <summary>
    /// 
    /// </summary>
	[Table("student")]
	public class StudentDto
	{
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[age3]")]
		public byte? Age3 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[age2]")]
		public short? Age2 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[id]")]
		public int? Id { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[age1]")]
		public int? Age1 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[sid]")]
		public int? Sid { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[balance1]")]
		public float? Balance1 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[flag]")]
		public bool? Flag { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[balance2]")]
		public decimal? Balance2 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[age4]")]
		public long? Age4 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[name1]")]
		public string Name1 { get; set; }
			
		/// <summary>
		/// 
		/// </summary>
		[Column("[name2]")]
		public string Name2 { get; set; }
		
	}

}

