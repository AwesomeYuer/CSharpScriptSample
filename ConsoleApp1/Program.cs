namespace Microshaoft
{
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Threading.Tasks;
    public class Program
    {
        static async Task Main(string[] args)
        {
            var options = ScriptOptions
                                .Default
                                .AddReferences
                                    (
                                        "System.Linq"
                                        , "Newtonsoft.Json"
                                    )
                                .WithImports
                                    (
                                        "System"
                                        , "System.Linq"
                                    )
                                    ;

            var s0 = CSharpScript.Create("int x = 1;");
            var s1 = s0.ContinueWith("int y = 2;");
            var s2 = s1.ContinueWith<int>("x + y");
            Console.WriteLine(s2.RunAsync().Result.ReturnValue);
            var globals = new Globals();
            globals.Inputs["F3"] = DateTime.Now;
            globals.Inputs["F1"] = 100;
            var aa = globals.Inputs.Value<DateTime>("F3");


            try
            {
                _ = await CSharpScript
                                        .RunAsync
                                            (
@"
                                        //Console.WriteLine(aaa);
                                        Console.WriteLine(Inputs.Value<DateTime>(""F3""));
                                        Console.WriteLine((int) Inputs[""F1""] + 2);
                                        Outputs[""F2""] = 10;

"
                                                //, ScriptOptions
                                                //        .Default
                                                //        .AddImports("System")
                                                , options
                                                , globals
                                                , typeof(Globals)
                                            );

                Console.WriteLine((int) globals.Outputs["F2"] + 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            Console.ReadLine();


        }
    }

    public class Globals
    {
        public JToken Context   { get; }    = new JObject();
        public JToken Inputs    { get; }    = new JObject();
        public JToken Outputs   { get; }    = new JObject();
    }
}