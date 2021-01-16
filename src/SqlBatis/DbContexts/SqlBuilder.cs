using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlBatis.XUnit
{
    public class SqlBuilder
    {
        private readonly Dictionary<string, Clauses> _data = new Dictionary<string, Clauses>();
        private int _seq;

        private class Clause
        {
            public string Sql { get; set; }
            public bool IsInclusive { get; set; }
        }

        private class Clauses : List<Clause>
        {
            private readonly string _joiner, _prefix, _postfix;

            public Clauses(string joiner, string prefix = "", string postfix = "")
            {
                _joiner = joiner;
                _prefix = prefix;
                _postfix = postfix;
            }

            public string ResolveClauses()
            {
                return this.Any(a => a.IsInclusive)
                    ? _prefix +
                      string.Join(_joiner,
                          this.Where(a => !a.IsInclusive)
                              .Select(c => c.Sql)
                              .Union(new[]
                              {
                                  " ( " +
                                  string.Join(" OR ", this.Where(a => a.IsInclusive).Select(c => c.Sql).ToArray()) +
                                  " ) "
                              }).ToArray()) + _postfix
                    : _prefix + string.Join(_joiner, this.Select(c => c.Sql).ToArray()) + _postfix;
            }
        }

        public class Template
        {
            private readonly string _sql;
            private readonly SqlBuilder _builder;
            private int _dataSeq = -1; // Unresolved

            public Template(SqlBuilder builder, string sql)
            {
                _sql = sql;
                _builder = builder;
            }

            private static readonly Regex _regex = new Regex(@"\/\*\*.+?\*\*\/", RegexOptions.Compiled | RegexOptions.Multiline);

            private void ResolveSql()
            {
                if (_dataSeq != _builder._seq)
                {

                    rawSql = _sql;

                    foreach (var pair in _builder._data)
                    {
                        rawSql = rawSql.Replace("/**" + pair.Key + "**/", pair.Value.ResolveClauses());
                    }

                    // replace all that is left with empty
                    rawSql = _regex.Replace(rawSql, "");

                    _dataSeq = _builder._seq;
                }
            }

            private string rawSql;

            public string RawSql
            {
                get { ResolveSql(); return rawSql; }
            }
        }

        public Template Build(string sql) =>
            new Template(this, sql);

        protected SqlBuilder AddClause(string name, string sql, string joiner, string prefix = "", string postfix = "", bool isInclusive = false)
        {
            if (!_data.TryGetValue(name, out Clauses clauses))
            {
                clauses = new Clauses(joiner, prefix, postfix);
                _data[name] = clauses;
            }
            clauses.Add(new Clause { Sql = sql, IsInclusive = isInclusive });
            _seq++;
            return this;
        }

        public SqlBuilder Intersect(string sql) =>
            AddClause("intersect", sql, "\nINTERSECT\n ", "\n ", "\n", false);

        public SqlBuilder InnerJoin(string sql) =>
            AddClause("innerjoin", sql, "\nINNER JOIN ", "\nINNER JOIN ", "\n", false);

        public SqlBuilder LeftJoin(string sql) =>
            AddClause("leftjoin", sql, "\nLEFT JOIN ", "\nLEFT JOIN ", "\n", false);

        public SqlBuilder RightJoin(string sql) =>
            AddClause("rightjoin", sql, "\nRIGHT JOIN ", "\nRIGHT JOIN ", "\n", false);

        public SqlBuilder Where(string sql, bool condition = true)
        {
            if (condition)
            {
                AddClause("where", sql, " AND ", "WHERE ", "\n", false);
            }
            return this;
        }

        public SqlBuilder OrWhere(string sql, bool condition = true)
        {
            if (condition)
            {
                AddClause("where", sql, " OR ", "WHERE ", "\n", true);
            }
            return this;
        }

        public SqlBuilder OrderBy(string sql) =>
            AddClause("orderby", sql, " , ", "ORDER BY ", "\n", false);

        public SqlBuilder Select(string sql) =>
            AddClause("select", sql, " , ", "", "\n", false);

        public SqlBuilder Join(string sql) =>
            AddClause("join", sql, "\nJOIN ", "\nJOIN ", "\n", false);

        public SqlBuilder GroupBy(string sql) =>
            AddClause("groupby", sql, " , ", "\nGROUP BY ", "\n", false);

        public SqlBuilder Having(string sql, bool condition = true)
        {
            if (condition)
            {
                AddClause("having", sql, "\nAND ", "HAVING ", "\n", false);
            }
            return this;
        }

        public SqlBuilder Set(string sql, bool condition = true)
        {
            if (condition)
            {
                AddClause("set", sql, " , ", "SET ", "\n", false);
            }
            return this;
        }
    }
}
