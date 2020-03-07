using QueryGen.Core;
using QueryGen.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryGen.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLBuilder<Person> builder = SQLBuilder<Person>.GetInstance;

            string query = builder
                            .Between<DateTime>(x => x.DOB, DateTime.Today.AddDays(2), DateTime.Today.AddDays(-2))

                .GroupConditions((x) =>
                {
                    return x.StartsWith(y => y.Firstname, "P").And().EndsWith(y => y.Lastname, "N").Or().Contains(y => y.Firstname, "ve");
                })
                            .GetSQL(true, x => x.City, x => x.Firstname);

            Console.WriteLine(query);
            Console.ReadKey();
        }
    }
}
