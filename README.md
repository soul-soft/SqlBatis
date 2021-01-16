# SqlBatis

## 免责声明

 本人对所有因使用本产品而导致的损失概不负责,使用前请先测试和阅读源代码并自觉遵守开源协议，可用于学习交流二次开发
 
## 打印日志

``` C#
//通过重写DbContext，我们可以定制化一些需求
public class LoggerDbContext : DbContext
{
    public MyDbContext(DbContextBuilder builder)
        : base(builder)
    {

    }
    //通过覆盖CreateDbCommand，来实现日志打印，DbContext中有许多方法设计成可以被覆盖的，
    protected override IDbCommand CreateDbCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
    {
        Trace.WriteLine("================Command===================");
        Trace.WriteLine(sql);
        Trace.WriteLine("===========================================");
        return base.CreateDbCommand(sql, parameter, commandTimeout, commandType);
    }
}
//创建数据上下文
var context = new LoggerDbContext(new DbContext()
{
    Connection = new MySqlConnector.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
    DbContextType = DbContextType.Mysql
});
```

## sql查询

``` C#
//返回匿名类型的集合
List<dynamic> list = context.Query("select * from student").ToList();
//映射到指定类型，忽略大小写和下划线
List<Student> list = context.Query<Student>("select id,stu_name from student").ToList();
//多结果集
using(var multi = context.QueryMultiple("select id,stu_name from student;select count(1) from student"))
{
    var list = multi.GetList<Student>();
    var count = multi.get<int>();
}
public class Student
{
    public int Id {get;set;}
    public string StuName {get; set;}
}

``

## Sqlbuilder

``` C#

var b1 = new SqlBuilder();
b1.Where("id>@Id")
  .Where("score>@Score")
  .Join("student_name as b on a.id=b.sid");
var p = new { Age = 1, Score = 20 };
//对两个模板进行构建
var tmp1 = b1.Build("select * from student as a /**join**/ /**where**/ ");
var tmp2 = b1.Build("select count(1) from student as a /**join**/ /**where**/ ");
using(var multi = context.QueryMultiple($"{tmp1.RawSql};{tmp2.RawSql}",new {Id=1,Score=5}))
{
    var list = multi.GetList<Student>();
    var count = multi.Get<int>();
}

```





