# SqlBatis

## 一、项目介绍
 
 该项目内置单表linq操作，xml动态sql解析，词法分析，类型映射等功能。

1. SqlMapper,用来处理sql与数据库操作，它设计的目标是支持mysql,sqlserver,sqllite,pgsql等.
 
2. TypeMapper用于完成将数据库的字段类型映射到C#类型，内部定义了类型转换函数和转换规则.
 
3. TypeConvert用于完成数据库记录到C#类型的转换。通过IL动态创建IDataReader对象到C#实体类的转换函数和将C#对象解构成Key-value的函数.
 
4. ExpressionContext是一个轻量的词法分析器，用于将字符串表达式生成C#表达式，进而生成委托.
 
## 二、ExpressionContext
 该类型的实例是线程安全的，可复用的。它的设计及其简单，功能也很有限，但是对于我们的需求足够了，它的实现逻辑如下：
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
