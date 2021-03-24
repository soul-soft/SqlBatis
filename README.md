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
/// 1.定义转换器
/// </summary>
public static class MyConvertMethod
{
    public static MethodInfo CharArrayConvertStringMethod = typeof(MyConvertMethod).GetMethod(nameof(CharArrayConvertString));
    public static MethodInfo StringConvertJsonMethod = typeof(MyConvertMethod).GetMethod(nameof(StringConvertJson));
    /// <summary>
    /// 用于移除sqlserver中nchar类型末尾的空格
    /// 参数必须得有(IDataRecord record, int i)
    /// </summary>
    /// <param name="record">必须的</param>
    /// <param name="i">必须的</param>
    /// <returns></returns>
    public static string CharArrayConvertString(IDataRecord record, int i)
    {
        if (record.IsDBNull(i))
        {
            return default;
        }
        return record.GetString(i).Trim();
    }
    /// <summary>
    /// 用于将数据库中的json类型映射成实体类
    /// 泛型方法
    /// 参数必须得有(IDataRecord record, int i)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="record"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    public static T StringConvertJson<T>(IDataRecord record, int i)
    {
        if (record.IsDBNull(i))
        {
            return default;
        }
        var json = record.GetString(i);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }
}

/// <summary>
/// 2.重写转换器匹配函数
/// </summary>
public class MyDbEntityMapperProvider : DbEntityMapperProvider
{
    protected override MethodInfo MatchDataRecordConvertMethod(Type returnType, Type entityMemberType, DbFieldInfo fieldInfo)
    {
        //如果是nchar或者nvarcher
        if (fieldInfo.TypeName == "nchar"|| fieldInfo.TypeName == "nvarchar")
        {
            return MyConvertMethod.CharArrayConvertStringMethod;
        }
        if (entityMemberType.IsClass && entityMemberType!=typeof(string) && )
        {
            //如果是泛型方法，必须MakeGenericMethod
            return MyConvertMethod.StringConvertJsonMethod.MakeGenericMethod(entityMemberType);
        }
        //否则使用群主默认的
        return base.MatchDataRecordConvertMethod(returnType, entityMemberType, fieldInfo);
    }
}
var connectionString = @"server=127.0.0.1;user id=root;password=1024;database=sqlbatis;";
var connection = new MySqlConnection(connectionString);
//3.设置默认的转换器
SqlBatisSettings.DbEntityMapperProvider = new MyDbEntityMapperProvider();
```


