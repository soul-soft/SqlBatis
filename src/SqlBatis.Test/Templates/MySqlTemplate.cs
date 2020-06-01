using System;
using SqlBatis.Attributes;

namespace YC51.Models
{
	
	/// <summary>
    /// 
	/// 更新时间：2020-05-28 14:59:07
    /// </summary>
	[Table("student")]
	public partial class Student
	{
				
		/// <summary>
        /// 
		/// ColumnType：int, IsNull：NO, Default：NULL
		/// JsName:id
		/// </summary>
		[Column("id")]
        [PrimaryKey]  
        [Identity]
        public int? Id { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：varchar(45), IsNull：YES, Default：NULL
		/// JsName:stuName
		/// </summary>
		[Column("stu_name")]
        string StuName { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：datetime, IsNull：YES, Default：CURRENT_TIMESTAMP
		/// JsName:createTime
		/// </summary>
		[Column("create_time")]
        DateTime? CreateTime { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：bit(1), IsNull：YES, Default：NULL
		/// JsName:isDel
		/// </summary>
		[Column("is_del")]
        object IsDel { get; set; }
	}
	
	/// <summary>
    /// 
	/// 更新时间：2020-05-28 14:59:07
    /// </summary>
	[Table("student_score")]
	public partial class StudentScore
	{
				
		/// <summary>
        /// 
		/// ColumnType：int, IsNull：NO, Default：NULL
		/// JsName:id
		/// </summary>
		[Column("id")]
        [PrimaryKey]  
        [Identity]
        public int? Id { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：int, IsNull：YES, Default：NULL
		/// JsName:studentId
		/// </summary>
		[Column("student_id")]
        int? StudentId { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：double, IsNull：YES, Default：NULL
		/// JsName:score
		/// </summary>
		[Column("score")]
        double? Score { get; set; }
	}
	
	/// <summary>
    /// 
	/// 更新时间：2020-05-28 14:59:07
    /// </summary>
	[Table("user")]
	public partial class User
	{
				
		/// <summary>
        /// 
		/// ColumnType：varchar(50), IsNull：NO, Default：NULL
		/// JsName:id
		/// </summary>
		[Column("Id")]
        [PrimaryKey]  
        string Id { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：varchar(255), IsNull：YES, Default：NULL
		/// JsName:username
		/// </summary>
		[Column("Username")]
        string Username { get; set; }
			
		/// <summary>
        /// 
		/// ColumnType：varchar(255), IsNull：YES, Default：NULL
		/// JsName:password
		/// </summary>
		[Column("Password")]
        string Password { get; set; }
	}
}



