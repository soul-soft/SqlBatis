using SqlBatis.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Test.Extensions
{
    public class StudentDto
    {
        public int? Id { get; set; }
        [Column("stu_name")]
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool? IsDelete { get; set; }
    }
}
