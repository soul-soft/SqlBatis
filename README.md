# SqlBatis

## 免责声明

1. 本人对所有因使用本产品而导致的损失概不负责,使用前请先测试和阅读源代码并自觉遵守开源协议，可用于学习交流二次开发

2. sqlbatis无疑是轻量的orm，太没有复杂的功能。麻雀虽小但五脏俱全，你可以像dapper那样使用它，也可以像linq那样使用它，甚至可以像mybatis那样去使用它。

3. 它的实体映射能力几乎接近手写代码（这在单元测试中有测试案例），sqlbatis从sql到返回查询结果这个过程所执行的代码行数极短（这也是高性能的指标之一）。实体创建采用IL，你可以像使用C#代码一样定制化你的映射规则。

## 创建DbContext

``` C#
/**
 * DbContext会在第一次执行命令时自动检查连接是否开启，未开启则自动开启，记住你必须释放DbContext，来关闭连接
 * 也可以通过容器来处理，在net core中必须注册为scope范围的生命周期
 */

var context = new DbContext(new DbContextBuilder
{
    Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
    DbContextType = DbContextType.Mysql,
});
 
```


## 执行sql脚本(Dapper那样使用)

``` C#
/**
* 1.对于字段到实体的映射规则如下：字段名必须和CSharp字段名一致，不分大小写。如果需要忽略下划线可以在创建DbContext时配置。
*   其他规则请实现IEntityMapper接口进行自定义。
*/
//查询和执行存储过程都可以使用：ExecuteQuery
//返回dynamic
var list0 = context.Query("select * from student");
//返回Student，底层采用IL，动态编写映射器并缓存
var list1 = context.Query<Student>("select id,stu_name as stuName,create_time as createTime from student");
//执行非查询操作，并返回受影响的行数
var row = context.Execute("delete from student");
//多结果集查询，一次请求多个结果集，性能较高
using(var multi = context.MultipleQuery("select * from student;select count(1) from student"))
{
    //获取集合，第一个结果集，数据阅读器在执行muit的获取操作，会自动移动到下一个结果集
    var list = multi.GetList<List>();
    //由于未读完IDataReader中的结果集，如果此时执行context.Query("select * from student");会抛出异常
    //由于会自动移动到下一个结果级，我们不需要执行NextResult操作，当执行到最后一个结果级的时候，会自动关闭multi对象托管的IDataReader对象
    var count = multi.Get<long>();
    //由于上面已经读取完了multi的所有结果级，因此可以继续执行查询，multi托管的IDataReader已经被自动关闭
    context.Query("select * from student");
}
//默认的in查询
context.Execute("delete from student where id in (@Id1,@Id2,@Id3)",new {Id1=1,Id2=2,Id3=3});
//简化的in查询,会自动生成上面的sql
context.Execute("delete from student where id in @Id",new {Id=new int[]{1,2,3}});
```
## Linq操作

``` C#
//我们可以自定义DbContext
public class MyDbContext : DbContext
{
    public IDbQuery<StudentDto> Students { get => new DbQuery<StudentDto>(this); }
    public MysqlDbContext(DbContextBuilder builder)
        :base(builder)
    {

    }
}
//对比使用区别
var context1 = new DbContext(...);
var context2 = new MyDbContext(...);
var list1 = context1.From<Student>()
    .Select(); 
var list2 = context2.Student
    .Select(); 
var list1 = context.Students
    .Where(a=>a.Age>20)
    .Select();
var (list2,count) = context.Students
    .Page(1,10)
    .SelectMany();
var row = context.Students
    .Set(a=>a.Name,"zs")
    .Set(a=>a.Age,a=>a.Age+1)
    .Set(a=>a.Gender,gender,gerder!=null)
    .Where(a=>a.Id==1)
    .Update();
var row = context.Student.Insert(new Student
{
    Name="zs",
    Age=50
});
```

## Xml操作
xml的优势是可以构建复杂的sql，灵活的sql，动态的sql,推荐将xml编译到程序集中。基本格式如下：

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<commands namespace="student">

  <!--
    框架只定义了if,var,where等标签
    insert,select可以随意，并没有实际意义
  -->
  
  <var id="offset">
    LIMIT 0,10
  </var>

  <select id="list">   
    <!-- 定义局部变量,where标签非常强大， -->
     <var id="where">
      <where>
        <if test="Id!=null">
            AND Id=@Id
        </if>
        <if test="Name!=null" value="AND Name=@Name"/>
      </where>
    </var>    
    select * from student ${where}${offset};
    select count(1) from student ${where}
  </select>

  <insert id="add">
    insert into student(name,age)values(@Name,@age)
  </insert>
</commands>
```

``` C#
使用xml功能必须先加载你的xml配置
SqlbatisSettings.XmlCommandsProvider.Load(System.Reflection.Assembly.GetExecutingAssembly(), @".+\.xml");
using(var multi = db.From("student.list",new Student(){Id=null,Name="zs"}).ExecuteMultiQuery<Student>())
{
    var list = multi.GetList<Student>();
    var count = multi.Get<long>();
}
//由于该student.list已今缓存了（考虑性能）表达式原型为Func<Student,bool>,因此下面的解析将出现错误，参数只能是Student类型（由第一次解析决定）
var multi = db.From("student.list",new {Id=(int?)null,Name="zs"}
//解析性能测试
var xmlProvider = GlobalSettings.XmlCommandsProvider;
for(var i=0;i<100000;i++)
{
    xmlProvider.Resolev("student.list",new {Id=(int?)i,Name="zs");
}
```

### 自定义类型映射提供程序

``` C#
/// <summary>
/// 1.定义转换器
/// </summary>
public static class MyConvertMethod
{
    public static MethodInfo CharArrayConvertStringMethod = typeof(MyConvertMethod).GetMethod(nameof(CharArrayConvertString));
    public static MethodInfo StringConvertJsonMethod = typeof(MyConvertMethod).GetMethod(nameof(StringConvertJson));
    /// <summary>
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
        if (entityMemberType?.IsClass && entityMemberType!=typeof(string) && )
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
var context = new DbContext(new DbContextBuilder
{
    Connection = connection,
    DbContextType = DbContextType.SqlServer2012,
});
var list = context.From<StudentDto>()
         .Select();
```
