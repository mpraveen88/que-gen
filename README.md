
# Query Generator
Easy SQL Query Generator from C# Lambda Expressions.

# Installation
Download the `QueryGen.Core.dll` from `dist` folder and refer it in your .NET project.

# How To Use
- Define a POCO class. Let's say Person.cs.
- Initialize and get an instance of SQLBuilder
>`SQLBuilder<Person> builder = SQLBuilder<Person>.GetInstance;`

## Person.cs
`
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryGen.Model
{
    [Table("PersonDetails")]
    public class Person
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        
        [Column("BirthDate")]
        public DateTime? DOB { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}
`

## Example 1: To fetch all the person(s) whose firstname starts with letter "P"
**C# :**   `string query = builder.StartsWith(x => x.Firstname, "P").GetSQL();`
**Output :**  `WHERE  (LTRIM(RTRIM(LOWER([Firstname]))) LIKE 'p%' )`


## Example 2: To fetch all the person(s) whose firstname starts with letter "P" and age is between 5 and 20
**C# :**   `string query = builder.StartsWith(x => x.Firstname, "P").Between<DateTime>(x => x.DOB, DateTime.Now.AddYears(-20), DateTime.Now.AddYears(-5)).GetSQL();`
**Output :**  `WHERE  (LTRIM(RTRIM(LOWER([Firstname]))) LIKE 'p%' )  ( [DOB] BETWEEN 2000-03-08 AND 2015-03-08 )`

