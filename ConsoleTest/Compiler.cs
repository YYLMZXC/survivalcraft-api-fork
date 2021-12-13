using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace ConsoleTest
{
    public class Compiler
    {
        public static void M() {
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();
            ICodeCompiler objICodeCompiler = objCSharpCodePrivoder.CreateCompiler();
            CompilerParameters objCompilerParameters = new CompilerParameters();
            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;
            CompilerResults cr = objICodeCompiler.CompileAssemblyFromSource(objCompilerParameters, GenerateCode());
            if (cr.Errors.HasErrors)
            {
                Console.WriteLine("编译错误：");
                foreach (CompilerError err in cr.Errors)
                {
                    Console.WriteLine(err.ErrorText);
                }
            }
            else
            {
                //  通过反射，调用HelloWorld的实例
                Assembly objAssembly = cr.CompiledAssembly;
                object objHelloWorld = objAssembly.CreateInstance("DynamicCodeGenerate.HelloWorld");
                MethodInfo objMI = objHelloWorld.GetType().GetMethod("OutPut");
                Console.WriteLine(objMI.Invoke(objHelloWorld, null));
            }

        }
        static string GenerateCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" using System; ");
            sb.Append(Environment.NewLine);
            sb.Append(" namespace DynamicCodeGenerate ");
            sb.Append(Environment.NewLine);
            sb.Append(" { ");
            sb.Append(Environment.NewLine);
            sb.Append("       public class HelloWorld ");
            sb.Append(Environment.NewLine);
            sb.Append("       { ");
            sb.Append(Environment.NewLine);
            sb.Append("           public string OutPut() ");
            sb.Append(Environment.NewLine);
            sb.Append("           { ");
            sb.Append(Environment.NewLine);
            sb.Append("                return \" Hello world! \" ; ");
            sb.Append(Environment.NewLine);
            sb.Append("           } ");
            sb.Append(Environment.NewLine);
            sb.Append("       } ");
            sb.Append(Environment.NewLine);
            sb.Append(" } ");
            string code = sb.ToString();
            return code;
        }
    }
}
