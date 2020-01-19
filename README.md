# SqlBatis

## 一、项目介绍
 
 该项目内置单表linq操作，xml动态sql解析，词法分析，类型映射等功能。

1. SqlMapper,用来处理sql与数据库操作，它设计的目标是支持mysql,sqlserver,sqllite,pgsql等.
 
2. TypeMapper用于完成将数据库的字段类型映射到C#类型，内部定义了类型转换函数和转换规则.
 
3. TypeConvert用于完成数据库记录到C#类型的转换。通过IL动态创建IDataReader对象到C#实体类的转换函数和将C#对象解构成Key-value的函数.
 
4. ExpressionContext是一个轻量的词法分析器，用于将字符串表达式生成C#表达式，进而生成委托.

5. 要想很好的掌握该项目，请阅读源码和单元测试:[document][https://github.com/1448376744/SqlBatis/wiki "example"]
 
## 二、ExpressionContext
 该类型的实例是线程安全的，可复用的。它的设计及其简单，功能也很有限，但是对于我们的需求足够了.
 它的实现逻辑如下：
 ``` C#
var expr = "(Age!=null) && (Name=='zs')";
 /**
 
1.通过正则匹配出每一个括号，因此你必须手动的给每个二元表达式加括号，它无法识别运算符的优先级.
 $1 = (Age!=null)
 $2 = (Name=='zs')
 $3 = $1 && $2

2.创建参数类型
 Expression.Parameter(typeof(T), "p");

3.逐个创建二元表达式
 Expression.MakeUnary(ExpressionType.Convert, expr, type)
 ...
 */
 ```

``` C#
var context = new ExpressionContext();
//Age必须是可以为null的类型:int?
var result = context.Create<Student>("(Age != null) && (Name=='zs')");
var flag= result.Func(new Student { Age = 1, Name = "zs" });
/**
常见错误:
1. (Age != null) && Name=='zs'

2. public class Student{ public int Age{get;set;} public string Name {get;set;}}
*/
```
## 配置DbContext
1. 第一种方式

``` C#
public class SqlDbContext : DbContext
{
    private static readonly IXmlResovle resovle;
    static SqlDbContext()
    {
        //for xml query
        resovle = new XmlResovle();
        resovle.Load(@"E:\SqlBatis\SqlBatis.Test", "*.xml");
    }
    //for linq
    public readonly IDbQuery<Student> Students;

    protected override void OnLogging(string message, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        //logger
    }
    protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
    {
        builder.Connection = new SqlConnection("Data Source=192.168.31.33;Initial Catalog=test;User ID=sa;Password=yangche!1234;Pooling=true");
        builder.XmlResovle = null;
        builder.DbContextType = DbContextType.SqlServer;
        return builder;
    }
    public SqlDbContext()
    {
        Students = new DbQuery<Student>(this);
    }
}
```
2. 第二种

``` C#
var db = new DbContext(new DbContextBuilder()
{ 
    DbContextType=DbContextType.Mysql,
    ...
});

```

3.asp.net core
``` C#
//SqlDbContext.cs
 public class SqlDbContext : DbContext
    {
        private ILogger<SqlDbContext> _logger = null;
        protected override void OnLogging(string message, IDataParameterCollection parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            _logger.LogInformation(message);
            if (parameter != null)
            {
                foreach (IDataParameter item in parameter)
                {
                    _logger.LogInformation($"{item.ParameterName} = {item.Value ?? "NULL"}");
                }
            }
        }

        public SqlDbContext(DbContextBuilder builder, ILogger<SqlDbContext> logger) :
            base(builder)
        {
            _logger = logger;
        }
    }
//Startup.cs
 public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IXmlResovle>(s =>
            {
                var resovle = new XmlResovle();
                resovle.Load("./Mappers", "*.xml");
                return resovle;
            });
            services.AddScoped(s =>
            {
                var logger = s.GetService<ILogger<SqlDbContext>>();
                var resovle = s.GetService<IXmlResovle>();
                return new SqlDbContext(new DbContextBuilder()
                {
                    Connection = new SqlConnection(Configuration.GetConnectionString("sqlserver")),
                    DbContextType = DbContextType.SqlServer,
                    XmlResovle = resovle,
                }, logger);
            });
            services.AddControllers();
        }
```
3. 基本使用

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<commands namespace="sutdent">
  <variable id="columns">
    Id,Age,Name
  </variable>

  <select id="list-dynamic">
    select ${columns} from student 
    <where>
     <if test="Id!=null" value="Id=@Id"/>
     <if test="Age!=null" value="Age>@Age"/>
     </where>
    limit 0,1   
  </select>

  <insert id="add">
    insert into student(name,age)values(@Name,@age)
  </insert>

</commands>
```

``` C#
//new 
var db = new SqlContext();
db.Open();
//linq1
db.From<Student>()
 .Select();
//linq2
db.Students.Select();
//xml
 var list = db.From("student.list-dynamic",new {Age=1,Id=(int?)null})
             .ExecuteQuery<Student>();
```
## sql查询

``` C#
var db = new SqlDbContext();
//只演示一个多结果集查询
using(var mutil = db.ExecuteMultiQuery("select * from student limit 0,1;selct count(1) from student"))
{
  var list = mutil.GetList<Student>();
  var count = mutil.Get<int>();
}
```

## linq查询

``` C#
var db = new SqlDbContext();
var flag = db.Student.Exists(a=>a.Id==2);
var count = db.Student.Where(a=>a.Id>2).Count();
var rows = db.Student.Where(a=>a.Id>=1).Delete();
//分页查询
var (list,count) = db.Students
                .Page(1,2)
                .SelectMany();
var parameter = new {Id=(int?)null,Age=20};
//动态查询
var list = db.Students
       .Where(a=>a.Id=parameter.Id,parameter.Id!=null)//当第二个条件成立，表达式有效，多个成立采用and连接
       .Where(a=>a.Id=parameter.Id,parameter.Id!=null)
       .Select();
```

