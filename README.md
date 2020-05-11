# SqlBatis

## 全局设置
``` C#
//所有设置均有默认行为，可以按需配置
//开启忽略下划线
GlobalSettings.EntityMapperProvider = new EntityMapperProvider(true);

//自定义数据库元信息提供程序，默认从注解中获取，如果你不想使用注解可以通过自定义元数据提供程序
GlobalSettings.DatabaseMetaInfoProvider = new MyDatabaseMetaInfoProvider();

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
var list0 = context.ExecuteQuery("select * from student");
//返回Student，底层采用IL，动态编写映射器并缓存
var list1 = context.ExecuteQuery<Student>("select id,stu_name as stuName,create_time as createTime from student");
//执行非查询操作，并返回受影响的行数
var row = context.ExecuteNonQuery("delete from student");
//多结果集查询，一次请求多个结果集，性能较高
using(var multi = context.ExecuteMultiQuery("select * from student;select count(1) from student"))
{
    //获取集合，第一个结果集，数据阅读器在执行muit的获取操作，会自动移动到下一个结果集
    var list = multi.GetList<List>();
    //由于未读完IDataReader中的结果集，如果此时执行context.ExecuteQuery("select * from student");会抛出异常
    //由于会自动移动到下一个结果级，我们不需要执行NextResult操作，当执行到最后一个结果级的时候，会自动关闭multi对象托管的IDataReader对象
    var count = multi.Get<long>();
    //由于上面已经读取完了multi的所有结果级，因此可以继续执行查询，multi托管的IDataReader已经被自动关闭
    context.ExecuteQuery("select * from student");
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
