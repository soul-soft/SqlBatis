# SqlBatis

## 项目介绍
 
 该项目内置单表linq操作，xml动态sql解析，词法分析，类型映射等功能。

1. SqlMapper,用来处理sql与数据库操作，它设计的目标是支持mysql,sqlserver,sqllite,pgsql等.
 
2. TypeMapper用于完成将数据库的字段类型映射到C#类型，内部定义了类型转换函数和转换规则.
 
3. TypeConvert用于完成数据库记录到C#类型的转换。通过IL动态创建IDataReader对象到C#实体类的转换函数和将C#对象解构成Key-value的函数.
 
4. ExpressionContext是一个轻量的词法分析器，用于将字符串表达式生成C#表达式，进而生成委托.
 
