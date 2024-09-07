namespace Furesoft.Core.GraphDb;

public class Sample
{
    class user(int age)
    {
        public int age { get; set; } = age;
    }

    class knows
    {
        public string some_property { get; set; }
    }

    private static void Run(string[] args)
    {
        using (var engine = new DbEngine())
        {
//                engine.DropDatabase();

            var user1 = new user(19);
            var user2 = new user(20);

            engine.AddRelation(user1, user2, new knows {some_property = "zzz"});

            engine.SaveChanges();

            var query = engine.CreateQuery();

            var a = query.Match<user>(user => user.age == 19);
            var x = query.To<knows>(k => k.some_property == "zzz");
            var b = query.Match(NodeDescription.Any());

            query.Execute();

            Console.WriteLine(a.Nodes.First()["age"]);
            Console.WriteLine(x.Relations.First()["some_property"]);
            Console.WriteLine(b.Nodes.First()["age"]);

            Console.ReadLine();

            engine.DropDatabase();
        }
    }
}