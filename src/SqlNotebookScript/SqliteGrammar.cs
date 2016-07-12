﻿// SQL Notebook
// Copyright (C) 2016 Brian Luft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using SqlNotebookCore;

namespace SqlNotebookScript {
    public static class SqliteGrammar {
        private static readonly Dictionary<string, SpecProd> _dict = new Dictionary<string, SpecProd>();
        public static IReadOnlyDictionary<string, SpecProd> Prods { get; } = _dict;

        static SqliteGrammar() {
            // sql-stmt ::= [ EXPLAIN [ QUERY PLAN ] ] ( <alter-table-stmt> | <analyze-stmt> | <attach-stmt> | 
            //      <begin-stmt> | <commit-stmt> | <create-index-stmt> | <create-table-stmt> | <create-trigger-stmt> |
            //      <create-view-stmt> | <create-virtual-table-stmt> | <delete-stmt> | 
            //      <detach-stmt> | <drop-index-stmt> | <drop-table-stmt> | <drop-trigger-stmt> | <drop-view-stmt> | 
            //      <insert-stmt> | <pragma-stmt> | <reindex-stmt> | <release-stmt> | <rollback-stmt> | 
            //      <savepoint-stmt> | <select-stmt> | <update-stmt> | <update-stmt-limited> | <vacuum-stmt> )
            _dict["sql-stmt"] = Prod(2,
                Opt(1, Tok(TokenType.Explain), Opt(1, Tok(TokenType.Query), Tok(TokenType.Plan))),
                Or(
                    SubProd("select-stmt"),
                    SubProd("update-stmt"),
                    SubProd("insert-stmt"),
                    SubProd("alter-table-stmt"),
                    SubProd("analyze-stmt"),
                    SubProd("attach-stmt"),
                    SubProd("begin-stmt"),
                    SubProd("commit-stmt"),
                    SubProd("create-index-stmt"),
                    SubProd("create-table-stmt"),
                    SubProd("create-trigger-stmt"),
                    SubProd("create-view-stmt"),
                    SubProd("create-virtual-table-stmt"),
                    SubProd("delete-stmt"),
                    SubProd("detach-stmt"),
                    SubProd("drop-index-stmt"),
                    SubProd("drop-table-stmt"),
                    SubProd("drop-trigger-stmt"),
                    SubProd("drop-view-stmt"),
                    SubProd("pragma-stmt"),
                    SubProd("reindex-stmt"),
                    SubProd("release-stmt"),
                    SubProd("rollback-stmt"),
                    SubProd("savepoint-stmt"),
                    SubProd("vacuum-stmt")
                )
            );

            // alter-table-stmt ::= 
            //      ALTER TABLE [ database-name "." ] table-name 
            //      ( RENAME TO new-table-name ) | ( ADD [COLUMN] <column-def> )
            _dict["alter-table-stmt"] = Prod(1,
                Tok(TokenType.Alter), Tok(TokenType.Table),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name"),
                Or(
                    Prod(1,
                        Tok(TokenType.Rename), Tok(TokenType.To),
                        Id("new table name")
                    ),
                    Prod(1,
                        Tok(TokenType.Add),
                        Opt(Tok(TokenType.ColumnKw)),
                        SubProd("column-def")
                    )
                )
            );

            // analyze-stmt ::= ANALYZE [ database-table-index-name [ "." table-or-index-name ] ]
            _dict["analyze-stmt"] = Prod(1,
                Tok(TokenType.Analyze),
                Opt(
                    Id("database, table, or index name"),
                    Opt(1, Tok(TokenType.Dot), Id("table or index name"))
                )
            );

            // attach-stmt ::= ATTACH [ DATABASE ] <expr> AS database-name
            _dict["attach-stmt"] = Prod(1,
                Tok(TokenType.Attach),
                Opt(Tok(TokenType.Database)),
                SubProd("expr"),
                Tok(TokenType.As),
                Id("database name")
            );

            // begin-stmt ::= BEGIN [ DEFERRED | IMMEDIATE | EXCLUSIVE ] [ TRANSACTION ]
            _dict["begin-stmt"] = Prod(1,
                Tok(TokenType.Begin),
                Opt(Or(Tok(TokenType.Deferred), Tok(TokenType.Immediate), Tok(TokenType.Exclusive))),
                Opt(Tok(TokenType.Transaction))
            );

            // commit-stmt ::= ( COMMIT | END ) [ TRANSACTION ]
            _dict["commit-stmt"] = Prod(1,
                Or(Tok(TokenType.Commit), Tok(TokenType.End)),
                Opt(Tok(TokenType.Transaction))
            );

            // rollback-stmt ::= ROLLBACK [ TRANSACTION ] [ TO [ SAVEPOINT ] savepoint-name ]
            _dict["rollback-stmt"] = Prod(1,
                Tok(TokenType.Rollback),
                Opt(Tok(TokenType.Transaction)),
                Opt(1,
                    Tok(TokenType.To),
                    Opt(Tok(TokenType.Savepoint)),
                    Id("savepoint name")
                )
            );

            // savepoint-stmt ::= SAVEPOINT savepoint-name
            _dict["savepoint-stmt"] = Prod(1,
                Tok(TokenType.Savepoint),
                Id("savepoint name")
            );

            // release-stmt ::= RELEASE [ SAVEPOINT ] savepoint-name
            _dict["release-stmt"] = Prod(1,
                Tok(TokenType.Release),
                Opt(Tok(TokenType.Savepoint)),
                Id("savepoint name")
            );

            // create-index-stmt ::= CREATE [ UNIQUE ] INDEX [ IF NOT EXISTS ]
            //      [ database-name "." ] index-name ON table-name "(" <indexed-column> [ "," <indexed-column> ]* ")"
            //      [ WHERE <expr> ]
            _dict["create-index-stmt"] = Prod(3,
                Tok(TokenType.Create),
                Opt(Tok(TokenType.Unique)),
                Tok(TokenType.Index),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Not), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("index name"),
                Tok(TokenType.On),
                Id("table name"),
                Tok(TokenType.Lp),
                Lst(TokenType.Comma, 1, SubProd("indexed-column")),
                Tok(TokenType.Rp),
                Opt(1, Tok(TokenType.Where), SubProd("expr"))
            );

            // indexed-column ::= ( column-name | expr ) [ COLLATE collation-name ] [ ASC | DESC ]
            _dict["indexed-column"] = Prod(1,
                Or(SubProd("expr"), Id("column name")),
                Opt(1, Tok(TokenType.Collate), Id("collation name")),
                Opt(Or(Tok(TokenType.Asc), Tok(TokenType.Desc)))
            );

            // create-table-stmt ::= CREATE [ TEMP | TEMPORARY ] TABLE [ IF NOT EXISTS ]
            //      [database-name "."] table-name
            //      (
            //          "(" <column-def> [ "," <column-def> ]* [ "," <table-constraint> ]* ")" [WITHOUT ROWID] | 
            //          AS <select-stmt> 
            //      )
            _dict["create-table-stmt"] = Prod(3,
                Tok(TokenType.Create),
                Opt(Or(Tok(TokenType.Temp), Tok("temporary"))),
                Tok(TokenType.Table),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Not), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name"),
                Or(
                    Prod(1,
                        Tok(TokenType.Lp),
                        Lst(TokenType.Comma, 1, Or(SubProd("table-constraint"), SubProd("column-def"))),
                        // this definition allows column definitions to appear after table constraints, which is illegal.
                        // it would be nice to improve here.
                        Tok(TokenType.Rp),
                        Opt(1, Tok(TokenType.Without), Tok("rowid"))
                    ),
                    Prod(1,
                        Tok(TokenType.As),
                        SubProd("select-stmt")
                    )
                )
            );

            // column-def ::= column-name [ <type-name> ] [ <column-constraint> ]*
            _dict["column-def"] = Prod(1,
                Id("column name"),
                Opt(SubProd("type-name")),
                Lst(null, 0, SubProd("column-constraint"))
            );

            // type-name ::= name+ [ "(" <signed-number> ")" | "(" <signed-number> "," <signed-number> ")" ]
            // implemented as: name+ [ "(" <signed-number> [ "," <signed-number> ] ")" ]
            _dict["type-name"] = Prod(1,
                Lst(null, 1, Or(
                    Id("data type"),
                    Toks(
                        // these tokens are okay to enter as part of the data type.  the list was created by testing
                        // SQLite; these are not enumerated in the grammar.
                        TokenType.Explain, TokenType.Query, TokenType.Plan, TokenType.Begin, TokenType.Deferred,
                        TokenType.Immediate, TokenType.Exclusive, TokenType.End, TokenType.Rollback,
                        TokenType.Savepoint, TokenType.Release, TokenType.If, TokenType.Temp, TokenType.Without,
                        TokenType.Abort, TokenType.Action, TokenType.After, TokenType.Analyze, TokenType.Asc,
                        TokenType.Attach, TokenType.Before, TokenType.By, TokenType.Cascade, TokenType.Cast,
                        TokenType.ColumnKw, TokenType.Conflict, TokenType.Database, TokenType.Desc, TokenType.Detach,
                        TokenType.Each, TokenType.Fail, TokenType.For, TokenType.Ignore, TokenType.Initially,
                        TokenType.Instead, TokenType.LikeKw, TokenType.Match, TokenType.No, TokenType.Key,
                        TokenType.Of, TokenType.Offset, TokenType.Pragma, TokenType.Raise, TokenType.Recursive,
                        TokenType.Replace, TokenType.Restrict, TokenType.Row, TokenType.Trigger, TokenType.Vacuum,
                        TokenType.View, TokenType.Virtual, TokenType.With, TokenType.Reindex, TokenType.Rename,
                        TokenType.CtimeKw, TokenType.Any, TokenType.Rem, TokenType.Concat, TokenType.Autoincr,
                        TokenType.Deferrable, TokenType.ToText, TokenType.ToBlob, TokenType.ToNumeric, TokenType.ToInt,
                        TokenType.ToReal, TokenType.IsNot, TokenType.Function, TokenType.AggFunction,
                        TokenType.Register
                    )
                )),
                Opt(
                    Tok(TokenType.Lp),
                    SubProd("signed-number"),
                    Opt(Tok(TokenType.Comma), SubProd("signed-number")),
                    Tok(TokenType.Rp)
                )
            );

            // column-constraint ::= 
            //      [ CONSTRAINT name ]
            //      ( PRIMARY KEY [ASC | DESC] <conflict-clause> [AUTOINCREMENT] )
            //      |   ( NOT NULL <conflict-clause> )
            //      |   ( NULL <conflict-clause> )
            //              ^^^ this line isn't in the official grammar, but SQLite seems to accept it.
            //      |   ( UNIQUE <conflict-clause> )
            //      |   ( CHECK "(" <expr> ")" )
            //      |   ( DEFAULT ( <signed-number> | <literal-value> | "(" <expr> ")" ) )
            //      |   ( COLLATE collation-name )
            //      |   ( <foreign-key-clause> )
            _dict["column-constraint"] = Prod(2,
                Opt(1, Tok(TokenType.Constraint), Id("constraint name")),
                Or(
                    Prod(1,
                        Tok(TokenType.Primary), Tok(TokenType.Key),
                        Opt(Or(Tok(TokenType.Asc), Tok(TokenType.Desc))),
                        SubProd("conflict-clause"),
                        Opt(Tok(TokenType.Autoincr))
                    ),
                    Prod(1, Tok(TokenType.Not), Tok(TokenType.Null), SubProd("conflict-clause")),
                    Prod(1, Tok(TokenType.Null), SubProd("conflict-clause")),
                    Prod(1, Tok(TokenType.Unique), SubProd("conflict-clause")),
                    Prod(1, Tok(TokenType.Check), Tok(TokenType.Lp), SubProd("expr"), Tok(TokenType.Rp)),
                    Prod(1,
                        Tok(TokenType.Default),
                        Or(
                            Prod(1, SubProd("signed-number")),
                            Prod(1, SubProd("literal-value")),
                            Prod(1, Tok(TokenType.Lp), SubProd("expr"), Tok(TokenType.Rp))
                        )
                    ),
                    Prod(1, Tok(TokenType.Collate), Id("collation name")),
                    Prod(1, SubProd("foreign-key-clause"))
                )
            );

            // signed-number ::= [ + | - ] numeric-literal
            _dict["signed-number"] = Prod(2,
                Opt(Or(Tok("+"), Tok("-"))),
                Or(
                    Tok(TokenType.Integer),
                    Tok(TokenType.Float)
                )
            );

            // table-constraint ::= [ CONSTRAINT name ]
            //      ( ( PRIMARY KEY | UNIQUE ) "(" <indexed-column> [ , <indexed-column> ]* ")" 
            //      <conflict-clause> | CHECK "(" <expr> ")" | FOREIGN KEY "(" column-name [ , column-name ]* ")" 
            //      <foreign-key-clause> )
            _dict["table-constraint"] = Prod(2,
                Opt(1, Tok(TokenType.Constraint), Id("constraint name")),
                Or(
                    Prod(1,
                        Or(
                            Prod(1, Tok(TokenType.Primary), Tok(TokenType.Key)),
                            Prod(1, Tok(TokenType.Unique))
                        ),
                        Tok(TokenType.Lp),
                        Lst(TokenType.Comma, 1, SubProd("indexed-column")),
                        Tok(TokenType.Rp),
                        SubProd("conflict-clause")
                    ),
                    Prod(1,
                        Tok(TokenType.Check),
                        Tok(TokenType.Lp),
                        SubProd("expr"),
                        Tok(TokenType.Rp)
                    ),
                    Prod(1,
                        Tok(TokenType.Foreign),
                        Tok(TokenType.Key),
                        Tok(TokenType.Lp),
                        Lst(TokenType.Comma, 1, Id("column name")),
                        Tok(TokenType.Rp),
                        SubProd("foreign-key-clause")
                    )
                )
            );

            // foreign-key-clause ::= 
            //      REFERENCES foreign-table 
            //      [ "(" column-name [ "," column-name ]* ")" ]
            //      [
            //          (
            //              ON ( DELETE | UPDATE )
            //              ( SET NULL | SET DEFAULT | CASCADE | RESTRICT | NO ACTION )
            //          ) | (
            //              MATCH name
            //          )
            //      ]*
            //      [ [NOT] DEFERRABLE [ INITIALLY DEFERRED | INITIALLY IMMEDIATE ] ]
            _dict["foreign-key-clause"] = Prod(1,
                Tok(TokenType.References), Id("foreign table name"),
                Opt(
                    Tok(TokenType.Lp),
                    Lst(TokenType.Comma, 1, Id("column name")),
                    Tok(TokenType.Rp)
                ),
                Lst(null, 0, Or(
                    Prod(1,
                        Tok(TokenType.On),
                        Or(Tok(TokenType.Delete), Tok(TokenType.Update)),
                        Or(
                            Prod(1, Tok(TokenType.Set), Or(Tok(TokenType.Null), Tok(TokenType.Default))),
                            Prod(1, Tok(TokenType.Cascade)),
                            Prod(1, Tok(TokenType.Restrict)),
                            Prod(1, Tok(TokenType.No), Tok(TokenType.Action))
                        )
                    ),
                    Prod(1, Tok(TokenType.Match), Id("match type"))
                )),
                Opt(
                    Opt(Tok(TokenType.Not)),
                    Tok(TokenType.Deferrable),
                    Opt(1, Tok(TokenType.Initially), Or(Tok(TokenType.Deferred), Tok(TokenType.Immediate)))
                )
            );

            // conflict-clause ::= [ ON CONFLICT ( ROLLBACK | ABORT | FAIL | IGNORE | REPLACE ) ]
            _dict["conflict-clause"] = Prod(1,
                Opt(2,
                    Tok(TokenType.On), Tok(TokenType.Conflict),
                    Or(Tok(TokenType.Rollback), Tok(TokenType.Abort), Tok(TokenType.Fail),
                        Tok(TokenType.Ignore), Tok(TokenType.Replace))
                )
            );

            // create-trigger-stmt ::= CREATE [ TEMP | TEMPORARY ] TRIGGER [ IF NOT EXISTS ]
            //      [database-name "."] trigger-name [BEFORE | AFTER | INSTEAD OF]
            //      ( DELETE | INSERT | UPDATE [OF column-name [ "," column-name ]* ] ) ON table-name
            //      [ FOR EACH ROW ] [ WHEN <expr> ]
            //      BEGIN ( ( <update-stmt> | <insert-stmt> | <delete-stmt> | <select-stmt> ) ";" )+ END
            _dict["create-trigger-stmt"] = Prod(3,
                Tok(TokenType.Create),
                Opt(Tok(TokenType.Temp), Tok("temporary")),
                Tok(TokenType.Trigger),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Not), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("trigger name"),
                Opt(Or(
                    Prod(1, Tok(TokenType.Before)),
                    Prod(1, Tok(TokenType.After)),
                    Prod(1, Tok(TokenType.Instead), Tok(TokenType.Of))
                )),
                Or(
                    Prod(1, Tok(TokenType.Delete)),
                    Prod(1, Tok(TokenType.Insert)),
                    Prod(1,
                        Tok(TokenType.Update),
                        Opt(1, Tok(TokenType.Of), Lst(TokenType.Comma, 1, Id("column name")))
                    )
                ),
                Tok(TokenType.On),
                Id("table name"),
                Opt(1, Tok(TokenType.For), Tok(TokenType.Each), Tok(TokenType.Row)),
                Opt(1, Tok(TokenType.When), SubProd("expr")),
                Tok(TokenType.Begin),
                Lst(null, 1,
                    Or(SubProd("update-stmt"), SubProd("insert-stmt"), SubProd("delete-stmt"), SubProd("select-stmt")),
                    Tok(TokenType.Semi)
                ),
                Tok(TokenType.End)
            );

            // create-view-stmt ::= CREATE [ TEMP | TEMPORARY ] VIEW [ IF NOT EXISTS ]
            //      [database-name "."] view-name [ "(" column-name [ "," column-name ]* ")" ] AS <select-stmt>
            _dict["create-view-stmt"] = Prod(3,
                Tok(TokenType.Create),
                Opt(Tok(TokenType.Temp), Tok("temporary")),
                Tok(TokenType.View),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Not), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("view name"),
                Opt(1,
                    Tok(TokenType.Lp),
                    Lst(TokenType.Comma, 1, Id("column name")),
                    Tok(TokenType.Rp)
                ),
                Tok(TokenType.As),
                SubProd("select-stmt")
            );

            // create-virtual-table-stmt ::= CREATE VIRTUAL TABLE [ IF NOT EXISTS ]
            //      [ database-name "." ] table-name
            //      USING module-name [ "(" module-argument [ "," module-argument ]* ")" ]
            _dict["create-virtual-table-stmt"] = Prod(2,
                Tok(TokenType.Create), Tok(TokenType.Virtual), Tok(TokenType.Table),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Not), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name"),
                Tok(TokenType.Using),
                Id("module name"),
                Opt(1,
                    Tok(TokenType.Lp),
                    Lst(TokenType.Comma, 1, SubProd("expr")),
                    Tok(TokenType.Rp)
                )
            );

            // with-clause ::= WITH [ RECURSIVE ] <cte-table-name> AS "(" <select-stmt> ")"
            //      [ "," <cte-table-name> AS "(" <select-stmt> ")" ]*
            _dict["with-clause"] = Prod(1,
                Tok(TokenType.With),
                Opt(Tok(TokenType.Recursive)),
                Lst(TokenType.Comma, 1,
                    SubProd("cte-table-name"),
                    Tok(TokenType.As),
                    Tok(TokenType.Lp),
                    SubProd("select-stmt"),
                    Tok(TokenType.Rp)
                )
            );

            // cte-table-name ::= table-name [ "(" column-name [ "," column-name ]* ")" ]
            _dict["cte-table-name"] = Prod(1,
                Id("table name"),
                Opt(1,
                    Tok(TokenType.Lp),
                    Lst(TokenType.Comma, 1, Id("column name")),
                    Tok(TokenType.Rp)
                )
            );

            // common-table-expression ::= table-name [ "(" column-name [ "," column-name ]* ")" ] 
            //      AS "(" <select-stmt> ")"
            _dict["common-table-expression"] = Prod(1,
                SubProd("cte-table-name"),
                Tok(TokenType.As),
                Tok(TokenType.Lp),
                SubProd("select-stmt"),
                Tok(TokenType.Rp)
            );

            // delete-stmt ::= [ <with-clause> ] DELETE FROM <qualified-table-name> [ WHERE <expr> ]
            //      [ 
            //          [ ORDER BY <ordering-term> [ "," <ordering-term> ]* ]
            //          LIMIT <expr> [ ( OFFSET | "," ) <expr> ]
            //      ]
            _dict["delete-stmt"] = Prod(2,
                Opt(SubProd("with-clause")),
                Tok(TokenType.Delete), Tok(TokenType.From),
                SubProd("qualified-table-name"),
                Opt(1, Tok(TokenType.Where), SubProd("expr")),
                Opt(2,
                    Opt(1,
                        Tok(TokenType.Order),
                        Tok(TokenType.By),
                        Lst(TokenType.Comma, 1, SubProd("ordering-term"))
                    ),
                    Tok(TokenType.Limit),
                    SubProd("expr"),
                    Opt(1,
                        Or(Tok(TokenType.Offset), Tok(TokenType.Comma)),
                        SubProd("expr")
                    )
                )
            );

            // detach-stmt ::= DETACH [ DATABASE ] database-name
            _dict["detach-stmt"] = Prod(1,
                Tok(TokenType.Detach),
                Opt(Tok(TokenType.Database)),
                Id("database name")
            );

            // drop-index-stmt ::= DROP INDEX [ IF EXISTS ] [ database-name "." ] index-name
            _dict["drop-index-stmt"] = Prod(2,
                Tok(TokenType.Drop),
                Tok(TokenType.Index),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("index name")
            );

            // drop-table-stmt ::= DROP TABLE [ IF EXISTS ] [ database-name "." ] table-name
            _dict["drop-table-stmt"] = Prod(2,
                Tok(TokenType.Drop),
                Tok(TokenType.Table),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name")
            );

            // drop-trigger-stmt ::= DROP TRIGGER [ IF EXISTS ] [ database-name "." ] trigger-name
            _dict["drop-trigger-stmt"] = Prod(2,
                Tok(TokenType.Drop),
                Tok(TokenType.Trigger),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("trigger name")
            );

            // drop-view-stmt ::= DROP VIEW [ IF EXISTS ] [ database-name "." ] view-name
            _dict["drop-view-stmt"] = Prod(2,
                Tok(TokenType.Drop),
                Tok(TokenType.View),
                Opt(1, Tok(TokenType.If), Tok(TokenType.Exists)),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("view name")
            );

            // The SQLite grammar for expressions does not express operator precedence.  Our expression grammar is
            // modified from the official SQLite grammar to take operator precedence into account.
            //
            // From https://www.sqlite.org/lang_expr.html:
            //      SQLite understands the following binary operators, in order from highest to lowest precedence:
            //          ||
            //          *    /    %
            //          +    -
            //          <<   >>  &    |
            //          <    <=   >    >=
            //          =    ==   !=   <>    IS   IS NOT   IN   LIKE   GLOB  MATCH   REGEXP
            //          AND
            //          OR
            //      ...
            //      The COLLATE operator is a unary postfix operator that assigns a collating sequence to an
            //      expression. The COLLATE operator has a higher precedence (binds more tightly) than any binary
            //      operator and any unary prefix operator except "~". (COLLATE and "~" are associative so their
            //      binding order does not matter.) ...

            // expr ::= or-expr
            _dict["expr"] = Prod(1,
                SubProd("or-expr")
            );

            // or-expr ::= and-expr [ OR and-expr ]*
            _dict["or-expr"] = Prod(1,
                Lst(TokenType.Or, 1, SubProd("and-expr"))
            );

            // and-expr ::= eq-expr [ AND eq-expr ]*
            _dict["and-expr"] = Prod(1,
                Lst(TokenType.And, 1, SubProd("eq-expr"))
            );

            // eq-expr ::= ineq-expr [ eq-expr-op | eq-expr-is | eq-expr-in | eq-expr-like | eq-expr-between ]*
            _dict["eq-expr"] = Prod(1,
                SubProd("ineq-expr"),
                Lst(null, 0,
                    Or(SubProd("eq-expr-op"), SubProd("eq-expr-is"), SubProd("eq-expr-in"), SubProd("eq-expr-like")))
            );

            // eq-expr-op ::= ( "=" | "==" | "!=" | "<>" ) ineq-expr
            _dict["eq-expr-op"] = Prod(1,
                Or(Tok("="), Tok("=="), Tok("!="), Tok("<>")),
                SubProd("ineq-expr")
            );

            // eq-expr-is ::= (IS [NOT] ineq-expr) | ISNULL | NOTNULL | (NOT NULL)
            _dict["eq-expr-is"] = Prod(1,
                Or(
                    Prod(1,
                        Tok(TokenType.Is),
                        Opt(Tok(TokenType.Not)),
                        SubProd("ineq-expr")
                    ),
                    Prod(1, Tok(TokenType.IsNull)),
                    Prod(1, Tok(TokenType.NotNull)),
                    Prod(1, Tok(TokenType.Not), Tok(TokenType.Null))
                )
            );

            // eq-expr-in ::= [NOT] IN
            //                  (
            //                      "(" [ <select-stmt> | <expr> [ "," <expr> ]* ] ")" | 
            //                      [database-name "."] table-name 
            //                  )
            _dict["eq-expr-in"] = Prod(2,
                Opt(Tok(TokenType.Not)),
                Tok(TokenType.In),
                Or(
                    Prod(1,
                        Tok(TokenType.Lp),
                        Opt(Or(
                            SubProd("select-stmt"),
                            Lst(TokenType.Comma, 1, SubProd("expr"))
                        )),
                        Tok(TokenType.Rp)
                    ),
                    Prod(2,
                        Opt(Id("database name"), Tok(TokenType.Dot)),
                        Id("table name")
                    )
                )
            );

            // eq-expr-like ::= [NOT] (LIKE | GLOB | REGEXP | MATCH) <ineq-expr> [ESCAPE <ineq-expr>]
            _dict["eq-expr-like"] = Prod(2,
                Opt(Tok(TokenType.Not)),
                Or(Tok("like"), Tok("glob"), Tok("regexp"), Tok("match")),
                SubProd("ineq-expr"),
                Opt(1, Tok(TokenType.Escape), SubProd("ineq-expr"))
            );

            // eq-expr-between ::= [NOT] BETWEEN <ineq-expr> AND <ineq-expr>
            _dict["eq-expr-between"] = Prod(2,
                Opt(Tok(TokenType.Not)),
                Tok(TokenType.Between),
                SubProd("ineq-expr"),
                Tok(TokenType.And),
                SubProd("ineq-expr")
            );

            // ineq-expr ::= <bitwise-expr> [ ( "<" | "<=" | ">" | ">=" ) <bitwise-expr> ]*
            _dict["ineq-expr"] = Prod(1,
                LstP(Or(Tok("<"), Tok("<="), Tok(">"), Tok(">=")), 1, SubProd("bitwise-expr"))
            );

            // bitwise-expr ::= <add-expr> [ ( "<<" | ">>" | "&" | "|" ) <add-expr> ]*
            _dict["bitwise-expr"] = Prod(1,
                LstP(Or(Tok("<<"), Tok(">>"), Tok("&"), Tok("|")), 1, SubProd("add-expr"))
            );

            // add-expr ::= <mult-expr> [ ( "+" | "-" ) <mult-expr> ]*
            _dict["add-expr"] = Prod(1,
                LstP(Or(Tok("+"), Tok("-")), 1, SubProd("mult-expr"))
            );

            // mult-expr ::= <concat-expr> [ ( "*" | "/" | "%" ) <concat-expr> ]*
            _dict["mult-expr"] = Prod(1,
                LstP(Or(Tok("*"), Tok("/"), Tok("%")), 1, SubProd("concat-expr"))
            );

            // concat-expr ::= <unary-expr> [ "||" <unary-expr> ]*
            _dict["concat-expr"] = Prod(1,
                LstP(Tok("||"), 1, SubProd("unary-expr"))
            );

            // unary-expr ::= [ "-" | "+" | "NOT" ] <collate-expr>
            _dict["unary-expr"] = Prod(2,
                Opt(Or(Tok("-"), Tok("+"), Tok(TokenType.Not))),
                SubProd("collate-expr")
            );

            // collate-expr ::= ["~"] <expr-term> [COLLATE collation-name]
            _dict["collate-expr"] = Prod(2,
                Opt(Tok("~")),
                SubProd("expr-term"),
                Opt(1, Tok(TokenType.Collate), Id("collation name"))
            );

            // expr-term ::=
            //      <literal-value> |
            //      <bind-parameter> |
            //      [ [ database-name "." ] table-name "." ] column-name |
            //      function-name "(" [ [DISTINCT] <expr> [ "," <expr> ]* | "*" ] ")" |
            //      "(" <expr> ")" |
            //      CAST "(" <expr> AS <type-name> ")" |
            //      [ [NOT] EXISTS ] ( <select-stmt> ) |
            //      CASE [ <expr> ] WHEN <expr> THEN <expr> [ ELSE <expr> ] END |
            //      <raise-function>
            _dict["expr-term"] = Prod(1,
                Or(
                    // <literal-value>
                    Prod(1, SubProd("literal-value")),
                    // <bind-parameter>
                    Prod(1, Id("variable name", allowVar: true)),
                    // [ [ database-name "." ] table-name "." ] column-name
                    Prod(2,
                        Opt(
                            Opt(Id("database name"), Tok(TokenType.Dot)),
                            Id("table name"), Tok(TokenType.Dot)
                        ),
                        Id("column name")
                    ),
                    // expr ::= function-name "(" [ [DISTINCT] <expr> [ "," <expr> ]* | "*" ] ")"
                    Prod(2,
                        Id("function name"),
                        Tok(TokenType.Lp),
                        Opt(Or(
                            Prod(1, Tok(TokenType.Asterisk)),
                            Prod(2,
                                Opt(Tok(TokenType.Distinct)),
                                Lst(TokenType.Comma, 1, SubProd("expr"))
                            )
                        )),
                        Tok(TokenType.Rp)
                    ),
                    // expr ::= "(" <expr> ")"
                    Prod(1,
                        Tok(TokenType.Lp),
                        SubProd("expr"),
                        Tok(TokenType.Rp)
                    ),
                    // expr ::= CAST "(" <expr> AS <type-name> ")"
                    Prod(1,
                        Tok(TokenType.Cast), Tok(TokenType.Lp),
                        SubProd("expr"), Tok(TokenType.As), SubProd("type-name"),
                        Tok(TokenType.Rp)
                    ),
                    // expr ::= [ [NOT] EXISTS ] ( <select-stmt> )
                    Prod(2,
                        Opt(
                            Opt(Tok(TokenType.Not)),
                            Tok(TokenType.Exists)
                        ),
                        Tok(TokenType.Lp),
                        SubProd("select-stmt"),
                        Tok(TokenType.Rp)
                    ),
                    // expr ::= CASE [ <expr> ] WHEN <expr> THEN <expr> [ ELSE <expr> ] END
                    Prod(1,
                        Tok(TokenType.Case),
                        Opt(SubProd("expr")),
                        Tok(TokenType.When),
                        SubProd("expr"),
                        Tok(TokenType.Then),
                        SubProd("expr"),
                        Opt(1, Tok(TokenType.Else), SubProd("expr"))
                    ),
                    // expr ::= <raise-function>
                    Prod(1, SubProd("raise-function"))
                )
            );

            // raise-function ::= RAISE ( IGNORE | (( ROLLBACK | ABORT | FAIL ) "," error-message) )
            _dict["raise-function"] = Prod(1,
                Tok(TokenType.Raise),
                Tok(TokenType.Lp),
                Or(
                    Prod(1, Tok(TokenType.Ignore)),
                    Prod(1,
                        Or(Tok(TokenType.Rollback), Tok(TokenType.Abort), Tok(TokenType.Fail)),
                        Tok(TokenType.Comma),
                        LitStr("error message")
                    )
                ),
                Tok(TokenType.Rp)
            );

            // literal-value ::= numeric-literal
            // literal-value ::= string-literal
            // literal-value ::= blob-literal
            // literal-value ::= NULL
            // literal-value ::= CURRENT_TIME
            // literal-value ::= CURRENT_DATE
            // literal-value ::= CURRENT_TIMESTAMP
            _dict["literal-value"] = Prod(1,
                Or(
                    Tok(TokenType.Integer),
                    Tok(TokenType.Float),
                    Tok(TokenType.String),
                    Tok(TokenType.Blob),
                    Tok(TokenType.Null),
                    Tok("current_time"),
                    Tok("current_date"),
                    Tok("current_timestamp")
                )
            );

            // insert-stmt ::= [ <with-clause> ] 
            //      ( INSERT | REPLACE | INSERT OR REPLACE | INSERT OR ROLLBACK | 
            //          INSERT OR ABORT | INSERT OR FAIL | INSERT OR IGNORE ) INTO
            //      [ database-name "." ] table-name [ "(" column-name [ "," column-name ]* ")" ]
            //      ( 
            //          VALUES "(" <expr> [ "," <expr> ]* ")" [ "," "(" <expr> [ "," <expr> ]* ")" ]* | 
            //          <select-stmt> |
            //          DEFAULT VALUES
            //      )
            _dict["insert-stmt"] = Prod(1,
                Opt(SubProd("with-clause")),
                Or(
                    Prod(1,
                        Tok(TokenType.Insert),
                        Opt(1,
                            Tok(TokenType.Or),
                            Or(Tok(TokenType.Replace), Tok(TokenType.Rollback), Tok(TokenType.Abort),
                                Tok(TokenType.Fail), Tok(TokenType.Ignore)
                            )
                        )
                    ),
                    Prod(1, Tok(TokenType.Replace))
                ),
                Tok(TokenType.Into),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name"),
                Opt(
                    Tok(TokenType.Lp),
                    Lst(TokenType.Comma, 1, Id("column name")),
                    Tok(TokenType.Rp)
                ),
                Or(
                    Prod(1,
                        Tok(TokenType.Values),
                        Lst(TokenType.Comma, 1,
                            Tok(TokenType.Lp),
                            Lst(TokenType.Comma, 1, SubProd("expr")),
                            Tok(TokenType.Rp)
                        )
                    ),
                    Prod(1, SubProd("select-stmt")),
                    Prod(1, Tok(TokenType.Default), Tok(TokenType.Values))
                )
            );

            // pragma-stmt ::= PRAGMA [ database-name "." ] pragma-name
            //      [ "=" <pragma-value> | "(" <pragma-value> ")" ]
            _dict["pragma-stmt"] = Prod(1,
                Tok(TokenType.Pragma),
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("pragma name"),
                Opt(Or(
                    Prod(1,
                        Tok(TokenType.Eq),
                        SubProd("pragma-value")
                    ),
                    Prod(1,
                        Tok(TokenType.Lp),
                        SubProd("pragma-value"),
                        Tok(TokenType.Rp)
                    )
                ))
            );

            // pragma-value ::= <signed-number>
            // pragma-value ::= name
            // pragma-value ::= string-literal
            _dict["pragma-value"] = Prod(1,
                Or(
                    SubProd("signed-number"),
                    Id("name"),
                    LitStr("string")
                )
            );

            // reindex-stmt ::= REINDEX [ [ database-name "." ] table-or-index-or-collation-name ]
            _dict["reindex-stmt"] = Prod(1,
                Tok(TokenType.Reindex),
                Opt(
                    Opt(Id("database name"), Tok(TokenType.Dot)),
                    Id("table, index, or collation name")
                )
            );

            // select-stmt ::= [ WITH [ RECURSIVE ] <common-table-expression> [ , <common-table-expression> ]* ]
            // [ SELECT [ DISTINCT | ALL ] <result-column> [ , <result-column> ]*
            // [ FROM [ <table-or-subquery> [ , <table-or-subquery> ]* | <join-clause> ]1 ]
            // [ WHERE <expr> ]
            // [ GROUP BY <expr> [ , <expr> ]* [ HAVING <expr> ] ] | VALUES ( <expr> [ , <expr> ]* ) [ , ( <expr> [ , <expr> ]* ) ]* ]1 [ <compound-operator> [ SELECT [ DISTINCT | ALL ] <result-column> [ , <result-column> ]*
            // [ FROM [ <table-or-subquery> [ , <table-or-subquery> ]* | <join-clause> ]1 ]
            // [ WHERE <expr> ]
            // [ GROUP BY <expr> [ , <expr> ]* [ HAVING <expr> ] ] | VALUES ( <expr> [ , <expr> ]* ) [ , ( <expr> [ , <expr> ]* ) ]* ]1 ]*
            // [ ORDER BY <ordering-term> [ , <ordering-term> ]* ]
            // [ LIMIT <expr> [ [ OFFSET | , ]1 <expr> ] ]
            _dict["select-stmt"] = Prod(1,
                Opt(1,
                    Tok(TokenType.With),
                    Opt(Tok(TokenType.Recursive)),
                    Lst(TokenType.Comma, 1, SubProd("common-table-expression"))
                ),
                LstP(SubProd("compound-operator"), 1,
                    Or(
                        Prod(1,
                            Tok(TokenType.Select),
                            Opt(Or(Tok(TokenType.Distinct), Tok(TokenType.All))),
                            Lst(TokenType.Comma, 1, SubProd("result-column")),
                            Opt(1,
                                Tok(TokenType.From),
                                Or(
                                    SubProd("join-clause"),
                                    Lst(TokenType.Comma, 1, SubProd("table-or-subquery"))
                                )
                            ),
                            Opt(1,
                                Tok(TokenType.Where),
                                SubProd("expr")
                            ),
                            Opt(1,
                                Tok(TokenType.Group), Tok(TokenType.By),
                                Lst(TokenType.Comma, 1, SubProd("expr")),
                                Opt(1, Tok(TokenType.Having), SubProd("expr"))
                            )
                        ),
                        Prod(1,
                            Tok(TokenType.Values),
                            Lst(TokenType.Comma, 1, 
                                Tok(TokenType.Lp),
                                Lst(TokenType.Comma, 1, SubProd("expr")),
                                Tok(TokenType.Rp)
                            )
                        )
                    )
                ),
                Opt(1,
                    Tok(TokenType.Order), Tok(TokenType.By),
                    Lst(TokenType.Comma, 1, SubProd("ordering-term"))
                ),
                Opt(1,
                    Tok(TokenType.Limit),
                    SubProd("expr"),
                    Opt(1,
                        Or(Tok(TokenType.Offset), Tok(TokenType.Comma)),
                        SubProd("expr")
                    )
                )
            );

            // join-clause ::= <table-or-subquery> ( <join-operator> <table-or-subquery> <join-constraint> )+
            // note: the official grammar allows only a single join-operator. that seems like a mistake.
            // we've also changed the optional join-operator into an at-least-one, because the select-stmt production
            // already contains a case for a single table-or-subquery, so join-clause is only needed if a join-operator
            // is present.
            _dict["join-clause"] = Prod(1,
                SubProd("table-or-subquery"),
                Lst(null, 1,
                    SubProd("join-operator"),
                    SubProd("table-or-subquery"),
                    SubProd("join-constraint")
                )
            );

            // table-or-subquery ::= [ database-name "." ] table-function-name "(" [ <expr> ["," <expr>]* ")"
            //                                  [ [AS] table-alias ]
            // table-or-subquery ::= [ database-name "." ] table-name [ [ AS ] table-alias ]
            //      [ INDEXED BY index-name | NOT INDEXED ]
            // table-or-subquery ::= "(" ( <table-or-subquery> [ "," <table-or-subquery> ]* | <join-clause> ) ")"
            // table-or-subquery ::= "(" <select-stmt> ")" [ [ AS ] table-alias ]
            // note: the table-function-name production is described in the syntax diagram but not the text BNF.
            _dict["table-or-subquery"] = Prod(1,
                Or(
                    Prod(3,
                        Opt(Id("database name"), Tok(TokenType.Dot)),
                        Id("table function name"),
                        Tok(TokenType.Lp),
                        Lst(TokenType.Comma, 0, SubProd("expr")),
                        Tok(TokenType.Rp),
                        Opt(
                            Opt(Tok(TokenType.As)),
                            Id("table alias")
                        )
                    ),
                    Prod(2,
                        Opt(Id("database name"), Tok(TokenType.Dot)),
                        Id("table name"),
                        Opt(
                            Opt(Tok(TokenType.As)),
                            Id("table alias")
                        ),
                        Opt(Or(
                            Prod(1, Tok(TokenType.Indexed), Tok(TokenType.By), Id("index name")),
                            Prod(1, Tok(TokenType.Not), Tok(TokenType.Indexed))
                        ))
                    ),
                    Prod(2,
                        Tok(TokenType.Lp),
                        SubProd("select-stmt"),
                        Tok(TokenType.Rp),
                        Opt(
                            Opt(Tok(TokenType.As)),
                            Id("table alias")
                        )
                    ),
                    Prod(1,
                        Tok(TokenType.Lp),
                        Or(
                            Lst(TokenType.Comma, 1, SubProd("table-or-subquery")),
                            SubProd("join-clause")
                        ),
                        Tok(TokenType.Rp)
                    )
                )
            );

            // result-column ::= *
            // result-column ::= table-name . *
            // result-column ::= <expr> [ [ AS ] column-alias ]
            _dict["result-column"] = Prod(1,
                Or(
                    Prod(1, Tok(TokenType.Star)),
                    Prod(3,
                        Id("table name"),
                        Tok(TokenType.Dot),
                        Tok(TokenType.Star)
                    ),
                    Prod(1,
                        SubProd("expr"),
                        Opt(
                            Opt(Tok(TokenType.As)),
                            Id("column alias")
                        )
                    )
                )
            );

            // join-operator ::= ,
            // join-operator ::= [ NATURAL ] [ LEFT [ OUTER ] | INNER | CROSS ] JOIN
            _dict["join-operator"] = Prod(1,
                Or(
                    Prod(1, Tok(TokenType.Comma)),
                    Prod(3,
                        Opt(Tok("natural")),
                        Opt(
                            Or(
                                Prod(1, Tok("left"), Opt(Tok("outer"))),
                                Prod(1, Tok("inner")),
                                Prod(1, Tok("cross"))
                            )
                        ),
                        Tok("join")
                    )
                )
            );

            // join-constraint ::= [ ON <expr> | USING ( column-name [ , column-name ]* ) ]
            _dict["join-constraint"] = Prod(1,
                Opt(Or(
                    Prod(1, Tok(TokenType.On), SubProd("expr")),
                    Prod(1,
                        Tok(TokenType.Using),
                        Tok(TokenType.Lp),
                        Lst(TokenType.Comma, 1, Id("column name")),
                        Tok(TokenType.Rp)
                    )
                ))
            );

            // ordering-term ::= <expr> [ COLLATE collation-name ] [ ASC | DESC ]
            _dict["ordering-term"] = Prod(1,
                SubProd("expr"),
                Opt(1, Tok(TokenType.Collate), Id("collation name")),
                Opt(Or(Tok(TokenType.Asc), Tok(TokenType.Desc)))
            );

            // compound-operator ::= UNION
            // compound-operator ::= UNION ALL
            // compound-operator ::= INTERSECT
            // compound-operator ::= EXCEPT
            _dict["compound-operator"] = Prod(1,
                Or(
                    Prod(1, Tok(TokenType.Union), Opt(Tok(TokenType.All))),
                    Prod(1, Tok(TokenType.Intersect)),
                    Prod(1, Tok(TokenType.Except))
                )
            );

            // update-stmt ::= [ <with-clause> ] UPDATE
            //      [ OR ROLLBACK | OR ABORT | OR REPLACE | OR FAIL | OR IGNORE ] <qualified-table-name>
            //      SET column-name = <expr> [ , column-name = <expr> ]* [ WHERE <expr> ]
            //      [
            //          [ ORDER BY <ordering-term> [ , <ordering-term> ]* ]
            //          LIMIT <expr> [ ( OFFSET | , ) <expr> ]
            //      ]
            _dict["update-stmt"] = Prod(2,
                Opt(SubProd("with-clause")),
                Tok(TokenType.Update),
                Opt(1,
                    Tok(TokenType.Or),
                    Opt(Tok(TokenType.Rollback), Tok(TokenType.Abort), Tok(TokenType.Replace), Tok(TokenType.Fail),
                        Tok(TokenType.Ignore))
                ),
                SubProd("qualified-table-name"),
                Tok(TokenType.Set),
                Lst(TokenType.Comma, 1, Id("column name"), Tok(TokenType.Eq), SubProd("expr")),
                Opt(1, Tok(TokenType.Where), SubProd("expr")),
                Opt(2,
                    Opt(
                        Tok(TokenType.Order), Tok(TokenType.By),
                        Lst(TokenType.Comma, 1, SubProd("ordering-term"))
                    ),
                    Tok(TokenType.Limit),
                    SubProd("expr"),
                    Opt(1,
                        Or(Tok(TokenType.Offset), Tok(TokenType.Comma)),
                        SubProd("expr")
                    )
                )
            );

            // qualified-table-name ::= [ database-name . ] table-name [ INDEXED BY index-name | NOT INDEXED ]
            _dict["qualified-table-name"] = Prod(2,
                Opt(Id("database name"), Tok(TokenType.Dot)),
                Id("table name"),
                Opt(Or(
                    Prod(1, Tok(TokenType.Indexed), Tok(TokenType.By), Id("index name")),
                    Prod(1, Tok(TokenType.Not), Tok(TokenType.Indexed))
                ))
            );

            // vacuum-stmt ::= VACUUM
            _dict["vacuum-stmt"] = Prod(1, Tok(TokenType.Vacuum));
        }

        private static SpecProd Prod(int numReq, params SpecTerm[] terms) {
            return new SpecProd { NumReq = numReq, Terms = terms };
        }

        private static IdentifierTerm Id(string desc, bool allowVar = false) {
            return new IdentifierTerm { Desc = desc, AllowVariable = allowVar };
        }

        private static KeyTokenTerm Tok(TokenType type) {
            return new KeyTokenTerm { Type = type };
        }

        private static StringTokenTerm Tok(string text) {
            return new StringTokenTerm { Text = text };
        }

        private static TokenSetTerm Toks(params TokenType[] types) {
            return new TokenSetTerm { Types = types };
        }

        private static OptionalTerm Opt(int numReq, params SpecTerm[] terms) {
            return new OptionalTerm { Prod = new SpecProd { NumReq = numReq, Terms = terms } };
        }

        private static OptionalTerm Opt(params SpecTerm[] terms) {
            return new OptionalTerm { Prod = new SpecProd { NumReq = terms.Length, Terms = terms } };
        }

        private static OrTerm Or(params SpecProd[] prods) {
            return new OrTerm { Prods = prods };
        }

        private static OrTerm Or(params SpecTerm[] terms) {
            return new OrTerm { Prods = terms.Select(x => new SpecProd { NumReq = 1, Terms = new[] { x } }).ToArray() };
        }

        private static ProdTerm SubProd(string name) {
            return new ProdTerm { ProdName = name };
        }

        private static ListTerm Lst(TokenType? separator, int min, params SpecTerm[] terms) {
            return new ListTerm {
                SeparatorProd = separator.HasValue ? Prod(1, Tok(separator.Value)) : null,
                Min = min,
                ItemProd = Prod(terms.Length, terms)
            };
        }

        private static ListTerm LstP(SpecProd separatorProd, int min, params SpecTerm[] terms) {
            return new ListTerm {
                SeparatorProd = separatorProd,
                Min = min,
                ItemProd = Prod(terms.Length, terms)
            };
        }

        private static ListTerm LstP(SpecTerm separatorTerm, int min, params SpecTerm[] terms) {
            return new ListTerm {
                SeparatorProd = Prod(1, separatorTerm),
                Min = min,
                ItemProd = Prod(terms.Length, terms)
            };
        }

        private static LiteralStringTerm LitStr(string desc) {
            return new LiteralStringTerm { Desc = desc };
        }

        private static BreakpointTerm Breakpoint() {
            return new BreakpointTerm();
        }
    }
}