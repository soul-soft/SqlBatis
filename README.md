# SqlBatis

## 全局设置
``` C#
//所有设置均有默认行为，可以按需配置，一下均为默认值
GlobalSettings.EntityMapperProvider = new EntityMapperProvider();

//自定义数据库元信息提供程序，默认从注解中获取，如果你不想使用注解可以通过自定义元数据提供程序
GlobalSettings.DbMetaInfoProvider = new AnnotationDbMetaInfoProvider();

//Xml命令提供程序，加载xml配置。建议将文件属性设置为嵌入的资源（vs文件属性->生成操作->嵌入的资源）
GlobalSettings.XmlCommandsProvider.Load(System.Reflection.Assembly.GetExecutingAssembly(), @".+\.xml");
```
## 创建DbContext

``` C#
/**
 * 使用之前必须Open，使用之后必须Close。DbContext由于是Connection对象的简单包装层因此生命周期等同于Connection，非线程安全的，
 * 在net core中必须注册为scope范围的生命周期
 */

var context = new DbContext(new DbContextBuilder
{
    Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
    DbContextType=DbContextType.Mysql,
});
 
```

## 执行sql脚本

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
    //由于未读完IDataReader中的结果集，如果此时执行context.ExecuteQuery("select * from student");会抛出异常
    //由于会自动移动到下一个结果级，我们不需要执行NextResult操作，当执行到最后一个结果级的时候，会自动关闭multi对象托管的IDataReader对象
    var count = multi.Get<long>();
    //由于上面已经读取完了multi的所有结果级，因此可以继续执行查询，multi托管的IDataReader已经被自动关闭
    context.Query("select * from student");
}
//默认的in查询
context.ExecuteNonQuery("delete from student where id in (@Id1,@Id2,@Id3)",new {Id1=1,Id2=2,Id3=3});
//简化的in查询,会自动生成上面的sql
context.ExecuteNonQuery("delete from student where id in @Id",new {Id=new int[]{1,2,3}});
/**
* sqlbatis支持的参数类型如下：
* 1.类类型（常用）
* 2.Dictionary<string,object>（常用）
* 3.匿名类型（常用）
* 4.IDataParameter类型
* 5.IEnumerable<IDbDataParameter>
*/
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
//在使用linq查询时必须定义实体，以及实体注解（如果你自定义了注解提供程序可以不使用注解)，常用实例：
//通过注解获取
var entity1 = context.Students.Get(1);
var list1 = context.Students
    .Where(a=>a.Age>20)
    .Select();
var (list2,count) = context.Students
    .Page(1,10)
    .SelectMany();
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
/**
* xml中的参数分两大类
*   1.数据库参数，已@开头的
*   2.xml表达式参数，if.test中的表达式参数
* if.test中的参数必须都在类中定义（只能是类类型，匿名类型），由于底层是解析字符串然后创建表达式树，进而生成委托（并缓存）限制如下：
*   1.如果要对值类型进行非空判断比如Id!=null，则Id必须是可以为null的类型比如：int?,long?
*   2.由于底层缓存了if.test表达式创建的委托，因此一个xml命令的id不能通过不同的参数类型去解析（一个id建议只能被一处调用）
*   3.if.test的表达式中必须收到加括号（底层通过正则分析）比如：<if test="(Id!=null)&&(Id>10)" value="Id=@Id">
*   4.if.test底层的解析器非常的轻量只有几百行代码，功能有限，基本满足使用
*/
使用xml功能必须先加载你的xml配置
GlobalSettings.XmlCommandsProvider.Load(System.Reflection.Assembly.GetExecutingAssembly(), @".+\.xml");
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

## 自定义提供程序

### 自定义数据库元信息提供程序

``` C#
//自定义提供程序
public class MyDbMetaInfoProvider : IDbMetaInfoProvider
{
    public List<DbColumnMetaInfo> GetColumns(Type type)
    {
        return type.GetProperties().Select(s => new DbColumnMetaInfo()
        {
            ColumnName = s.Name,
            CsharpName = s.Name,
            CsharpType = s.PropertyType,
            IsComplexType = false,
            IsConcurrencyCheck = false,
            IsDefault = false,
            IsIdentity = false,
            IsNotMapped = true,
            IsPrimaryKey = false,
        }).ToList();
    }

    public DbTableMetaInfo GetTable(Type type)
    {
        return new DbTableMetaInfo()
        {
            TableName = type.Name.ToUpper(),
            CsharpName = type.Name
        };
    }
}
//替换掉默认提供程序
GlobalSettings.DbMetaInfoProvider = new MyDbMetaInfoProvider();
```

### 自定义类型映射提供程序

``` C#
//自定义实现规则
public class MyEntityMapperProvider : IEntityMapperProvider
{
    /// <summary>
    /// 默认的提供程序是线程安全的
    /// </summary>
    private EntityMapperProvider defaultMapper = new EntityMapperProvider();
    public Func<object, Dictionary<string, object>> GetDeserializer(Type type)
    {
        return defaultMapper.GetDeserializer(type);
    }

    public Func<IDataRecord, T> GetSerializer<T>(IDataRecord record)
        {
            //如果是student类型
            if (typeof(T) == typeof(Student))
            {
                return new Func<IDataRecord, T>((r) =>
                {
                    var student = (object)new Student()
                    {
                        Id = r.GetInt32(r.GetOrdinal("id")),
                        CreateTime = r.GetDateTime(r.GetOrdinal("create_time")),
                        Name = r.GetString(r.GetOrdinal("stu_name"))
                    };
                    return (T)student;
                });
            }
            //否则使用默认实现
            return defaultMapper.GetSerializer<T>(record);
        }

    public Func<IDataRecord, dynamic> GetSerializer()
    {
        return defaultMapper.GetSerializer();
    }
}
//替换掉默认提供程序
GlobalSettings.EntityMapperProvider = new EntityMapperProvider();
```
