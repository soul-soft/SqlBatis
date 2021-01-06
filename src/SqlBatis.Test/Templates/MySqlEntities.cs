using System;
using SqlBatis.Attributes;
using SqlBatis.Test;

namespace SqlBatis.Test
{
	
	/// <summary>
    /// 
    /// </summary>
	[Table("message")]
	public partial class MessageDto
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
		[Column("remark")]
        public string Remark { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("create_time")]
        public DateTime? CreateTime { get; set; }
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
		[Column("stu_name")]
        public StuName StuName { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("balance")]
        public int? Balance { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("version")]
        public string Version { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("sid")]
        public int? Sid { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("create_time")]
        public DateTime? CreateTime { get; set; }
	}
	
	/// <summary>
    /// 
    /// </summary>
	[Table("student_bill")]
	public partial class StudentBillDto
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
		[Column("student_id")]
        public int? StudentId { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("remark")]
        public string Remark { get; set; }
	}
	
	/// <summary>
    /// 
    /// </summary>
	[Table("student_school")]
	public partial class StudentSchoolDto
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
	[Table("template")]
	public partial class TemplateDto
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
		[Column("projectId")]
        public string Projectid { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("merchantId")]
        public string Merchantid { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("terminalType")]
        public string Terminaltype { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("pageTemplate")]
        public string Pagetemplate { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailClientTemplate")]
        public string Emailclienttemplate { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailClientTemplateContent")]
        public string Emailclienttemplatecontent { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailClientSubject")]
        public string Emailclientsubject { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailMerchantTemplate")]
        public string Emailmerchanttemplate { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailMerchantTemplateContent")]
        public string Emailmerchanttemplatecontent { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("emailMerchantSubject")]
        public string Emailmerchantsubject { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("logoName")]
        public string Logoname { get; set; }
			
		/// <summary>
        /// 
		/// </summary>
		[Column("metadata")]
        public object Metadata { get; set; }
	}
}



