# SqlBatis

## 几大核心对象说明

### ExpressionActivator


``` C#
 //创建一个表达式生成器
 var activator = new ExpressionActivator();
 //生成参数为P类型的表达式，返回表达式委托，和表达式树
 //过程：（字符串->Lambda表达式->函数）
 var result = activator.Create<P>("(Age>20) && (Name=='zs')");
 //执行表达式
 var flag = result.Func(new P() 
 {
     Age=50,
     Name="zs"
 });
 //查看生成的LambdaExpression
 var expression = result.LambdaExpression;
```
### EntityMapper和EmitConverty

EntityMapper 用于完成数据库对象到C#对象的映射，底层才有EMIT创建委托，而非反射，性能极高（无反射）极小的减少拆装箱。中定义了如果映射数据库记录到CSharp类型的映射规则，其中定义了一下几种行为：

1. 选择适合的类型转换函数，比如db(bit)->csharp(bool)
2. 选择映射的属性，比如db(age)->csharp(Age)
3. 选择时候的构造器，自定义类型必须包含无参数构造器

EmitConverty根据EntityMapper定义的规则，动态生成DataReader到csharp类型转换函数（IL）,并缓存该函数
假设有表student(id,age,name)到csharp类型Student(Id,Age,Name),则自动采用IL技术生成以下代码

``` C#

//创建委托
public Student StudentSerializerafgjvvn385gjh(IDataReader reader)
{
    var student = new Student();
    student.Id = reader.GetInt32(0);
    student.Age = reader.GetInt32(1);
    student.Name = reader.GetString(2);
}

//使用示例
var connection = new SqlConnection("connectionString");
connection.Open();
var cmd = connection.CreateCommand();
cmd.CommandText = "select id,age,name from student";
var reader = cmd.ExecuteReader();
//使用IL创建IDataReader到Student类型的委托
var handler = EmitConvert.GetSerializer<Student>(reader);
while (reader.Read())
{
    Student student = handler(reader);
}

```

## Linq查询语法糖

### Attributes

1. TableAttribute("student")，该注解用于注解类型到表名
2. ColumnAttribute("id")，该注解用于注解字段到属性名
3. PrimaryKeyAttribute()，该注解用于注解字段为主键，在修改时更具该字段为更新条件
4. IdentityAttribute()，该注解用于注解字段为自增列，在新增时不向该字段设置值（sqlserver不能向该字段显示设置值）
5. NotMappedAttribute()，该注解用于移除该字段映射
6. ComplexTypeAttribute()计算列，新增和修改时自动忽略该字段
7. DefaultAttribute()默认值，新增时如果字段为null则忽略字段，否则使用指定的字段值
### Querybale

### 基本示例

``` C#
1. insert
//如果使用注解了indentity(mysql无碍)
context.From<Student>().Insert(new Student()
{
    Age = 20,
    Name = "zs"
});
//如果没有使用注解Identity,可以使用filter,表示不向该列设置值(sqlserver)
context.From<Student>()
.Filter(a=>a.Id)
.Insert(new Student()
{
    Age = 20,
    Name = "zs"
});
2. update
//如果使用PrimaryKey注解Id，则更具Id为条件更新
context.From<Student>().Update(new Student()
{
    Id = 1,
    Age = 20,
    Name = "zs"
});
//如果没有使用PrimaryKey注解Id,可以强制指定更新条件
context.From<Student>
.Where(a=>a.Id==1)
.Update(new Student()
{
    Age = 20,
    Name = "zs"
});
3. select
//动态查询，多个where成立则使用And连接每一个where条件
var p = new Student()
{
    Id = 2,
    Name = "zs"
};
//select Id,Name,Age from student where Id=@Id and Name=@Name
var list = context.From<Student>()
    .Where(a=>a.Id==p.Id,p.Id!=null)
    .Where(a=>a.Name==p.Name,p.Name!=null)
    .Select();
//分页
var (list,count) = context.From<Student>()
    .Page(1,10)
    .SelectMany();
```
### 并发更新
``` C#
//非并非安全
var student = context.From<Student>().Where(a=>a.Id==1).Single();
//因为student的余额随时可能被其它线程修改
//update student balance=@P_1 where id=1
context.From<Student>()
    .Set(a=>a.Balance,student+1)
    .Where(a=>a.Id==1)
    .Update();
//并发安全的
//update student balance=balace+1 where id=1
context.From<Student>()
   .Set(a=>a.Balance,a=>a.Balance+1)
   .Where(a=>a.Id==1)
   .Update();
//并发安全的,更新多个字段
//添加一个版本号字段，进行并发控制
student.Balance+=1;
student.Version = Guid.NewGuid().ToString();
context.From<Student>()
   .Where(a=>a.Id==1&&a.Version==student.Version)
   .Update(student);
```

## XmlResovle

1. 该实例用于管理整个应用的xml配置，应该采用单例模式
2. 解析器会忽略所有text片段中的空格
3. 如果是动态命令，则只能使用第一次获取的参数类型来获取，不能变化，因为XmlResovle会缓存动态表达式的委托，如果在多个地方获取则不能使用匿名类型

### xml语法格式说明

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<!--根节点(必须的)，可以指定namespace-->
<commands namespace="sutdent">

  <!--
    框架只定义了if,variable,where等标签
    insert,select可以随意，并没有实际意义,但是都必须指定id
    除了根节点其他都不是必须的
  -->
  <!--定义变量,插入语法：${id}-->
  <var id="columns">
    Id,Age,Name
  </var>
  
  <var id="offset">
    ORDER BY (SELECT 1) OFFSET @Index ROWS FETCH NEXT @Count ROWS ONLY
  </var>

 <!--你可以不用select来包裹，select并没有实际意义-->
  <select id="list-dynamic">
    <!--动态where必须用where包裹，试想如果if标签一个都不成立的时候，它能智能的不向sql写where-->
    <var id=local>
      <where>
          <!--if两种写法-->
          <!--注意Id必须是可以为null的类型-->
          <if test="Id!=null" value="Id>@Id"/>
          <if test="Age!=null">
            Age=@Age
          </if>
      </where>
     </var>
    select ${columns} from student ${local}   
    <!--使用变量-->
    ${offset}
   <!--同时生成计数语句-->
   <count>
    SELECT COUNT(1) FROM student
   </count>
  </select>
  
  <select id="getbyid">
    select Id,Name,Age form student where Id=@id limit 0,1
  </select>
  
  <insert id="add">
    insert into student(name,age)values(@Name,@age)
  </insert>

</commands>
```
### 代码示例

``` C#
var xmlresovle = new XmlResovle();
//加载配置
xmlresovle.Load("student.xml");
//普通sql,id=(namespace+id)
var sql1 = xmlresovle.Resolve("student.getbyid");
//动态sql
var sql2 = xmlresovle.Resolve("student.list-dynamic",new Student()
{ 
    Id=1,
    Age=9
});

```
### XmlQuery

使用之前必须配置context的XmlResovle

``` C#
//sql+参数，来获取一个执行器，参数可以是字典
//xmlresvle建议只创建一个，然后给所有的content都设置这个xmlresovle实例
var xmlresovle = new XmlResovle();
xmlresovle.Load("D:/xml","*.xml");
context.XmlResult = xmlresovle;
var executer = context.From("student.getbyid",new {Id = 1});
//执行该sql
var row = executer.ExecuteNonQuery();

//执行动态sql,传入sql所需要的所有参数，动态sql参数不能是字典，只能是class类型
//Id,Age必须是可以为空的类型，因为你的动态表达式中做了非空判断
//如果你的参数是匿名类型，则不能在其他位置编写多次下面的查询，出非你能保证每次都使用同一个匿名类型声明
//解决方案：不使用匿名类型就好了，原因是第一获取时会编译动态表达式生成委托会缓存（性能考虑），
//比如你第一次的匿名类型名是C，则生成的委托为Func<C,bool>参数类型是C，反会bool
var list = context.From("student.list-dynamic",new 
{
    Id = (int?)1,
    Age = (int?)90
}).ExecuteQuery();
```
### SqlQuery

``` C#
//参数类型可以是匿名类型，类类型，字典类型，DbParameter类型
var list = context.ExecuteQuery("select * from student Id=>@Id",new 
{
    Id=80
});
var row = context.ExecuteNonQuery("insert student(age,name)values(Age,Name)",new Student() 
{
    Age = 90,
    Name = "zs"
});
//返回多个结果集
var sql = "SELECT * FROM student;SELECT COUNT(1) FROM student";
using(var multi = context.ExecuteMultiQuery(sql))
{
     //获取第一个结果集
     var list = multi.GetList<Student>();
     //获取第二个结果集
     var count = multi.Get<long>();
}
```
