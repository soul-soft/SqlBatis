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
    var list = multi.Read<Student>();
    var count = multi.ReadFirst<int>();
}
public class Student
{
    public int Id {get;set;}
    public string StuName {get; set;}
}

```

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
    var list = multi.Read<Student>();
    var count = multi.ReadFirst<int>();
}

```
## Linq查询

``` C#
var entity = context.From<Student>().Where(a=>a.id==1).Single();
var entitys = context.From<Student>().Where(a=>a.id>1).Select();
var entiityNames = context.From<Student>().Select(s=>new 
{ 
    s.Name,
    s.Age
});
//默认忽略：这会忽略id字段
var row = context.From<Student>().Insert(new {Name="zs",Age=1});
//默认忽略：这会忽略Age字段
var row = context.From<Student>().Update(new {Name="zs",Id=1});
//显示忽略：这会忽略所有为null的字段
var row = context.From<Student>().Ignore().Update(new Student{Name="zs",Id=1});

public class Student
{
   [PrimaryKey]
   [Indenity]
   public int Id {get;set;}
   public string Name {get;set;}
   public int Age {get;set;}
}
```

## 自定义类型映射


``` C#
/// <summary>
/// 提供json转换能力
/// </summary>
public class MyDDbContextBehavior : DbContextBehavior
{
    readonly static MethodInfo _methodInfo = typeof(MyDDbContextBehavior).GetMethod("StringToJson");

    static readonly JsonSerializerOptions _jsonSerializerOptions;

    static MyDDbContextBehavior()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy=JsonNamingPolicy.CamelCase
        };
        _jsonSerializerOptions.Converters.Add(new JsonDateTimeConverter("yyyy-MM-dd HH:mm:ss"));
        _jsonSerializerOptions.Converters.Add(new JsonDateTimeNullableConverter("yyyy-MM-dd HH:mm:ss"));
    }

    public static T StringToJson<T>(IDataRecord record, int i)
    {
        if (record.IsDBNull(i))
        {
            return default;
        }
        var json = record.GetString(i);
        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }

    public override KeyValuePair<string, object> CreateDbCommandParameter(string name, object value)
    {
        if (value != null && typeof(ValueObject).IsInstanceOfType(value))
        {
            var json = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            return base.CreateDbCommandParameter(name, json);
        }
        return base.CreateDbCommandParameter(name, value);
    }

    protected override MethodInfo FindConvertMethod(Type entityType, Type memberType, DataReaderField recordField)
    {
        if (typeof(ValueObject).IsAssignableFrom(memberType))
        {
            return _methodInfo.MakeGenericMethod(memberType);
        }
        return base.FindConvertMethod(entityType, memberType, recordField);
    }
}
var connectionString = @"server=127.0.0.1;user id=root;password=1024;database=sqlbatis;";
var connection = new MySqlConnection(connectionString);
//3.设置默认的数据库上下文行为
new MyDbContext(new DbContextBuilder()
{
    Connection = new MySqlConnector.MySqlConnection(configuration.GetConnectionString("Mysql")),
    DbContextType = DbContextType.Mysql,
    DbContextBehavior = new MyDDbContextBehavior()
})
```
## 定义简单的函数

```C#
[SqlBatis.Attributes.Function]
 public static class SqlFun
 {
     public static T2 IF<T1, T2>(T1 column, T2 v1, T2 v2)
     {
         return default;
     }

     public static bool ISNULL<T1>(T1 t1)
     {
         return default;
     }
 }
 
 var list = _context.From<StudentDto>()
  .Ignore(a => a.StuGender)
  .Select(s => new
  {
      FF = SqlFun.IF(SqlFun.ISNULL(s.SchId), 1, 2)
  });
```



