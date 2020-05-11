# SqlBatis

## 创建DbContext

``` C#
/**
 * 使用之前必须Open，使用之后必须Close,DbContext由于是Connection对象的简单包装层因此生命周期等同于Connection，非线程安全的在net core中必须注册为
 * scope范围的生命周期
 * context.Open();
 */
 var context = new DbContext(new DbContextBuilder
 {
     Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
     DbContextType=DbContextType.Mysql,
 });
 
```
