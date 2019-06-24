namespace Microshaoft
{
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Diagnostics;
    using System.Threading;
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
                                        , "System.Threading"
                                    )
                                    ;

            var begin = Stopwatch.GetTimestamp();
            var script = CSharpScript
                                .Create
                                    (
        @"
                                            //Console.WriteLine(aaa);
                                            Console.WriteLine(Inputs.Value<DateTime>(""F3""));
                                            var f1 = (int) Inputs[""F1""];
                                            Console.WriteLine((int) Inputs[""F1""] * 100);
                                            Outputs[""F2""] = f1 * 100;
                                            Console.WriteLine($""in script:{Thread.CurrentThread.ManagedThreadId}"");
                                            //throw new Exception();
                                            Console.WriteLine($""inputs: {Inputs.ToString()}"");
        "

                                            , options
                                            , typeof(Globals)
                                    );
            var end = Stopwatch.GetTimestamp();
            Console.WriteLine($"create:{new TimeSpan(end - begin).TotalMilliseconds}");

            begin = Stopwatch.GetTimestamp();
            script.Compile();
            end = Stopwatch.GetTimestamp();
            Console.WriteLine($"compile:{new TimeSpan(end - begin).TotalMilliseconds}");


            Parallel
                .For
                    (
                        1
                        , 100
                        , new ParallelOptions
                        {
                             MaxDegreeOfParallelism = 8
                        }
                        , async (i) =>
                        {
                            var beginTime = Stopwatch.GetTimestamp();
                            var globals = new Globals();
                            globals.Inputs["F1"] = i;
                            Console.WriteLine($"in host:{Thread.CurrentThread.ManagedThreadId}");
                            globals.Inputs["F3"] = DateTime.Now;
                            globals.Inputs["F4"] = Guid.NewGuid();
                            _ = await
                                    script
                                        .RunAsync
                                            (
                                                globals
                                                , (x) =>
                                                {
                                                    Console.WriteLine(x);
                                                    return true;
                                                }
                                            );
                            var endTime = Stopwatch.GetTimestamp();
                            Console.WriteLine($"run:{new TimeSpan(endTime - beginTime).TotalMilliseconds}");
                            Console.WriteLine($"outputs F2:{(int) globals.Outputs["F2"]}");
                        }
                    );
            Console.ReadLine();
        }

        static async Task Main1(string[] args)
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
                                        //throw new Exception();
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
            catch (CompilationErrorException e)
            {
                Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics, e.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();


        }
    }

    public class Globals //: JToken
    {
        public JToken Context { get; } = new JObject();
        public JToken Inputs { get; } = new JObject();
        public JToken Outputs { get; } = new JObject();
        //public override JTokenType Type => throw new NotImplementedException();

        //public override bool HasValues => throw new NotImplementedException();

        //public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override JToken CloneToken()
        //{
        //    throw new NotImplementedException();
        //}

        //internal override bool DeepEquals(JToken node)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override int GetDeepHashCode()
        //{
        //    throw new NotImplementedException();
        //}
    }
}